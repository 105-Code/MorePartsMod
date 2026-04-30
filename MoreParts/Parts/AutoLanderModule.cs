using SFS.Parts;
using SFS.Parts.Modules;
using SFS.UI;
using SFS.Variables;
using SFS.World;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SFS.World.Rocket;

namespace MorePartsMod.Parts
{
    public class AutoLanderModule : MonoBehaviour, INJ_Rocket, INJ_Location, INJ_Physics, INJ_IsPlayer
    {
        // Rockets actively landing — used by the WorldLoader patch to keep physics alive.
        public static readonly HashSet<Rocket> ActiveAutoLanders = new HashSet<Rocket>();

        public Part Part;
        public Bool_Reference IsOn;

        public Rocket Rocket { get; set; }
        public Location Location { get; set; }
        public Rigidbody2D Rb2d { get; set; }
        public bool IsPlayer { get; set; }

        public Float_Reference TargetLandingSpeed; // 5f m/s
        public Float_Reference TriggerThreshold; // 0.8f ignite when engines need to be at 90%+ to make the landing

        public Float_Reference MaxAngularVel; //60f  deg/s

        private const float OrientTolerance = 5f;     // degrees

        private enum State { Idle, Orienting, Waiting, Burning }
        private State _state = State.Idle;
        private bool[] _engineStates;

        private void Start()
        {
            if (GameManager.main == null)
            {
                base.enabled = false;
                return;
            }
            Part.onPartUsed.AddListener(Toggle);
        }

        private void OnDestroy() => Deactivate();

        public void Toggle(UsePartData data)
        {
            bool wasIdle = _state == State.Idle;
            if (wasIdle)
                Activate();
            else
                Deactivate();
            data.successfullyUsedPart = wasIdle ? _state != State.Idle : true;
        }


        private void Activate()
        {
            if (Location == null || Location.planet == null)
            {
                Log("Auto Lander: Not near a planet");
                return;
            }

            _state = State.Orienting;
            IsOn.Value = true;
            ActiveAutoLanders.Add(Rocket);
            SaveEngineStates();
            Debug.Log($"Auto Lander: TargetLandingSpeed:{TargetLandingSpeed.Value} TriggerThreshold:{TriggerThreshold.Value} MaxAngularVel:{MaxAngularVel.Value} ");
            Log("Auto Lander: Activated");
        }

        private void Deactivate()
        {
            if (_state == State.Idle) return;
            _state = State.Idle;
            IsOn.Value = false;
            ActiveAutoLanders.Remove(Rocket);
            RestoreEngineStates();
            if (Rocket != null)
                Rocket.throttle.throttleOn.Value = false;
            Log("Auto Lander: Deactivated");
        }

        private void SaveEngineStates()
        {
            EngineModule[] engines = Rocket.partHolder.GetModules<EngineModule>();
            _engineStates = engines.Select(e => e.engineOn.Value).ToArray();
        }

        private void RestoreEngineStates()
        {
            if (_engineStates == null || Rocket == null) return;
            EngineModule[] engines = Rocket.partHolder.GetModules<EngineModule>();
            for (int i = 0; i < Mathf.Min(engines.Length, _engineStates.Length); i++)
                engines[i].engineOn.Value = _engineStates[i];
            _engineStates = null;
        }

        private void Log(string msg)
        {
            if (IsPlayer && MsgDrawer.main != null) MsgDrawer.main.Log(msg);
        }

        // Distance from the CoM to the lowest collider point along the world y-axis.
        // The rocket is vertical during landing so world-y ≈ radial-up, making this valid.
        private float GetBottomOffset()
        {
            if (Rb2d == null) return 0f;
            float comY = Rb2d.worldCenterOfMass.y;
            float minY = comY;
            foreach (Collider2D col in Rocket.partHolder.GetComponentsInChildren<Collider2D>())
                minY = Mathf.Min(minY, col.bounds.min.y);
            return comY - minY;
        }

        private float GetThrustAccel()
        {
            if (Rb2d == null || Rb2d.mass <= 0f) return 0f;
            // Sum ALL engines regardless of current on/off state — burn will turn them all on.
            float thrust = Rocket.partHolder.GetModules<EngineModule>()
                .Sum(e => e.thrust.Value * 9.8f);
            return thrust / Rb2d.mass;
        }

        private void FixedUpdate()
        {
            if (_state == State.Idle || Rb2d == null || !Rb2d.simulated)
                return;
            if (Location == null || Location.planet == null)
                return;

            double altitude = Location.GetTerrainHeight(true) - GetBottomOffset();

            // During burn, hold the rocket vertical (radially up) so lateral drift from
            // chasing a noisy retrograde vector at low speeds doesn't tilt it sideways.
            // Before ignition, orient retrograde to pre-align for the burn.
            Vector2 desiredUp;
            if (_state == State.Burning)
            {
                Vector2 radialUp = new Vector2((float)Location.position.x, (float)Location.position.y).normalized;
                Vector2 totalVel = new Vector2((float)Location.velocity.x, (float)Location.velocity.y);
                Vector2 horizontalVel = totalVel - Vector2.Dot(totalVel, radialUp) * radialUp;
                float horizontalSpeed = horizontalVel.magnitude;
                if (horizontalSpeed > 0.1f)
                {
                    // Tilt opposite to horizontal velocity: 3 deg per m/s, capped at 20 deg.
                    float tiltRad = Mathf.Min(horizontalSpeed * 3f, 20f) * Mathf.Deg2Rad;
                    desiredUp = (radialUp * Mathf.Cos(tiltRad) + (-horizontalVel / horizontalSpeed) * Mathf.Sin(tiltRad)).normalized;
                }
                else
                {
                    desiredUp = radialUp;
                }
            }
            else
            {
                Vector2 vel = new Vector2((float)Location.velocity.x, (float)Location.velocity.y);
                desiredUp = vel.magnitude > 0.5f ? -vel.normalized : (Vector2)Rb2d.transform.up;
            }
            float targetAngle = Vector2.SignedAngle(Vector2.up, desiredUp);
            float angleDiff = Mathf.DeltaAngle(Rb2d.rotation, targetAngle);
            Rb2d.angularVelocity = Mathf.Clamp(angleDiff * 0.5f, -MaxAngularVel.Value, MaxAngularVel.Value);

            bool oriented = Mathf.Abs(angleDiff) < OrientTolerance;

            if (_state == State.Orienting)
            {
                if (oriented)
                {
                    _state = State.Waiting;
                    Log("Auto Lander: Waiting for burn");
                }
                return;
            }

            float vertSpeed = -(float)Location.VerticalVelocity; // positive = falling toward surface
            float gravity = (float)Location.planet.GetGravity(Location.position).magnitude;
            float thrustAccel = GetThrustAccel();

            // Kinematics: compute throttle to arrive at TargetLandingSpeed at ground level.
            // effectiveAlt is the full remaining distance to terrain; clamped to avoid divide-by-zero.
            float effectiveAlt = Mathf.Max((float)altitude, 0.1f);
            float requiredDecel = vertSpeed > TargetLandingSpeed.Value
                ? (vertSpeed * vertSpeed - TargetLandingSpeed.Value * TargetLandingSpeed.Value) / (2f * effectiveAlt)
                : 0f;
            float requiredThrottle = thrustAccel > 0f ? (requiredDecel + gravity) / thrustAccel : 1f;

            if (_state == State.Waiting)
            {
                // Ignite once we need 90%+ throttle — last-moment ignition like a suicide burn.
                if (requiredThrottle >= TriggerThreshold.Value)
                {
                    _state = State.Burning;
                    foreach (EngineModule engine in Rocket.partHolder.GetModules<EngineModule>())
                        engine.engineOn.Value = true;
                    Rocket.throttle.throttleOn.Value = true;
                    Log("Auto Lander: Ignition!");
                }
                return;
            }

            // Shutdown when velocity reached target and within 1 m of ground.
            // If speed is already at target but still high, hover throttle holds it there until close.
            if (vertSpeed <= TargetLandingSpeed.Value && altitude <= 1.0)
            {
                Deactivate();
                Log("Auto Lander: Landed!");
                return;
            }

            // Burning: throttle tapers continuously as speed drops toward TargetLandingSpeed.
            // Accounts for drag and decreasing mass (rb2d.mass updates as fuel burns).
            Rocket.throttle.throttlePercent.Value = Mathf.Clamp(requiredThrottle, 0f, 1f);
        }
    }
}
