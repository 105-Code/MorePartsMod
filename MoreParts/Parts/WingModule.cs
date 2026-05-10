using SFS.Input;
using SFS.Variables;
using SFS.World;
using UnityEngine;
using static SFS.World.Rocket;

namespace MorePartsMod.Parts
{
    public class WingModule : MonoBehaviour, INJ_Location, INJ_Physics
    {
        public Float_Reference Length;             // wing span, meters
        public Float_Reference Width;              // wing chord, meters
        public Float_Reference MaxLiftCoefficient; // peak Cl
        public Float_Reference StallAngle;         // degrees
        public Float_Reference IncidenceAngle;     // degrees, built-in tilt of the wing relative to the fuselage

        public Bool_Reference DebugDrawLift;       // show lift/wind debug lines
        public Float_Reference FlapAngle;          // current flap deflection, written for the visual rotator

        public float FlapMaxAngle = 20f;  // degrees, max deflection in either direction
        public float FlapRate = 30f;  // degrees per second


        public Location Location { get; set; }
        public Rigidbody2D Rb2d { get; set; }

        private float _flapDeflection = 0f;

        private LineRenderer _liftLine;
        private LineRenderer _windLine;


        // Below this airspeed lift is negligible and the velocity direction is too noisy
        // to derive a meaningful angle of attack from.
        private const float MinAirspeed = 1f;

        private void Awake()
        {
            if (Length == null)
                Debug.LogError($"[{name}] Length reference is not set.");
            if (Width == null)
                Debug.LogError($"[{name}] Width reference is not set.");
            if (MaxLiftCoefficient == null)
                Debug.LogError($"[{name}] MaxLiftCoefficient reference is not set.");
            if (StallAngle == null)
                Debug.LogError($"[{name}] StallAngle reference is not set.");
            if (IncidenceAngle == null)
                Debug.LogError($"[{name}] IncidenceAngle reference is not set.");
            if (FlapAngle == null)
                Debug.LogError($"[{name}] FlapAngle reference is not set.");
            if (DebugDrawLift == null)
            {
                Debug.LogError($"[{name}] DebugDrawLift reference is not set.");
                return;
            }

            if (DebugDrawLift.Value)
            {
                var mat = new Material(Shader.Find("Sprites/Default"));
                _liftLine = CreateDebugLine("LiftLine", mat, Color.cyan, 0.3f, 0.1f);
                _windLine = CreateDebugLine("WindLine", mat, Color.green, 0.2f, 0.05f);
            }
        }

        private LineRenderer CreateDebugLine(string childName, Material mat, Color color, float startWidth, float endWidth)
        {
            var child = new GameObject(childName);
            child.transform.SetParent(transform, false);
            var lr = child.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.startWidth = startWidth;
            lr.endWidth = endWidth;
            lr.material = mat;
            lr.startColor = color;
            lr.endColor = color;
            lr.enabled = false;
            return lr;
        }

        private void Update()
        {
            float dir = 0f;
            if (((I_Key)KeybindingsPC.keys.Turn_Rocket[1]).IsKeyStay()) dir += 1f; // E
            if (((I_Key)KeybindingsPC.keys.Turn_Rocket[0]).IsKeyStay()) dir -= 1f; // Q
            if (dir != 0f)
                _flapDeflection = Mathf.Clamp(
                    _flapDeflection + dir * FlapRate * Time.deltaTime,
                    -FlapMaxAngle, FlapMaxAngle);
            else
                _flapDeflection = Mathf.MoveTowards(_flapDeflection, 0f, FlapRate * Time.deltaTime);

            FlapAngle.Value = -_flapDeflection;
        }

        private void HideDebugLines()
        {
            if (_liftLine != null) _liftLine.enabled = false;
            if (_windLine != null) _windLine.enabled = false;
        }

        private void FixedUpdate()
        {
            if (Location == null || Location.planet == null || Rb2d == null || !Rb2d.simulated)
            { HideDebugLines(); return; }
            if (!Location.planet.HasAtmospherePhysics)
            { HideDebugLines(); return; }

            float density = (float)Location.planet.GetAtmosphericDensity(Location.Height);
            if (density <= 0f)
            { HideDebugLines(); return; }

            Vector2 velocity = new Vector2((float)Location.velocity.x, (float)Location.velocity.y);
            float speed = velocity.magnitude;
            if (speed < MinAirspeed)
            { HideDebugLines(); return; }

            Vector2 velDir = velocity / speed;
            Vector2 chord = transform.right;

            // AoA = angle from velocity to chord, plus built-in incidence.
            // Normalize to [-90, 90]: chord direction doesn't matter for a symmetric airfoil.
            float aoa = Vector2.SignedAngle(velDir, chord) + IncidenceAngle.Value + _flapDeflection;
            if (aoa > 90f) aoa -= 180f;
            else if (aoa < -90f) aoa += 180f;

            float cl = ComputeLiftCoefficient(aoa);
            float liftMag = 0.5f * density * speed * speed * (Length.Value * Width.Value) * cl;

            // Lift acts perpendicular to the relative wind. The side is determined by
            // the sign of AoA (encoded in liftMag via cl), so a fixed CCW perpendicular
            // is correct regardless of how the wing prefab is oriented.
            Vector2 perp = new Vector2(-velDir.y, velDir.x);

            // Apply at the wing position so it creates a pitching moment around the CoM.
            // This is what gives an aircraft pitch stability — the tail's torque counteracts
            // any nose-up/down motion. Requires both a main wing (near CoM) and a tail wing
            // (well behind CoM) for stable flight; a single wing alone will spin the rocket.
            Rb2d.AddForceAtPosition(perp * liftMag, transform.position, ForceMode2D.Force);

            if (DebugDrawLift.Value && _liftLine != null)
            {
                Vector3 origin = transform.position;

                // Cyan: lift direction, scaled by Cl so it's visible at any speed
                Vector3 liftTip = origin + (Vector3)(perp * (cl * 5f));
                _liftLine.SetPosition(0, origin);
                _liftLine.SetPosition(1, liftTip);
                _liftLine.enabled = true;

                // Green: relative wind (opposite to velocity)
                Vector3 windTip = origin + (Vector3)(-velDir * 5f);
                _windLine.SetPosition(0, origin);
                _windLine.SetPosition(1, windTip);
                _windLine.enabled = true;

            }
        }

        // Symmetric airfoil lift curve:
        //   [0, stall]  → smoothstep so small inclinations are forgiving
        //   [stall, 90] → linear taper back to 0 (post-stall)
        //   beyond 90°  → 0
        private float ComputeLiftCoefficient(float aoaDeg)
        {
            float abs = Mathf.Abs(aoaDeg);
            float stall = Mathf.Max(StallAngle.Value, 0.01f);
            float maxCl = MaxLiftCoefficient.Value;

            float magnitude;
            if (abs <= stall)
            {
                float t = abs / stall;
                magnitude = maxCl * t * t * (3f - 2f * t); // smoothstep
            }
            else if (abs < 90f)
                magnitude = maxCl * (1f - (abs - stall) / (90f - stall));
            else
                magnitude = 0f;

            return magnitude * Mathf.Sign(aoaDeg);
        }
    }
}
