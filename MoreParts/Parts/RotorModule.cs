using SFS;
using SFS.Parts;
using SFS.Translations;
using SFS.Variables;
using SFS.World;
using System;
using UnityEngine;
using SFS.Parts.Modules;
using SFS.UI;
using static SFS.World.Rocket;


namespace MorePartsMod.Parts
{
    public class RotorModule : MonoBehaviour, INJ_IsPlayer, INJ_Location, INJ_Throttle, INJ_Physics
    {
        public Animator Animator;
        public Bool_Reference IsOn;
        public Float_Reference Throttle_current;
        public Float_Reference RPM;

        public Transform Base;
        public Part Part;

        private const float Radius = 0.6f;
        private float _diskArea;
        private float _diskVelocity;

        public Rigidbody2D Rb2d { set; get; }
        public Location Location { set; get; }
        public bool IsPlayer { set; get; }
        public float Throttle { set => Throttle_current.Value = value; }

        public I_MsgLogger Logger => IsPlayer ? (I_MsgLogger)MsgDrawer.main : new MsgNone();

        public void Awake()
        {
            Animator = GetComponent<Animator>();
            _diskArea = (float)(Math.PI * Radius * Radius);
        }

        private void Start()
        {
            if (GameManager.main == null)
            {
                enabled = false;
                return;
            }

            IsOn.OnChange += RecalculateRotor;
            Throttle_current.OnChange += RecalculateRotor;
            Part.onPartUsed.AddListener(Toggle);
        }

        private void RecalculateRotor()
        {
            float throttle = (float)Throttle_current.Value;
            // Angular velocity (rad/s) plugged into the actuator-disk formula as the
            // through-disk velocity Vd. Treating omega numerically as m/s is a tuning
            // shortcut: RPM (set per-prefab) is the knob that scales thrust.
            _diskVelocity = (float)((RPM.Value * throttle / 60.0) * 2.0 * Math.PI);

            Animator.speed = 100f * throttle;

            if (!IsOn.Value)
            {
                Throttle_current.Value = 0f;
                Animator.SetBool("isOn", false);
                return;
            }

            Animator.SetBool("isOn", throttle > 0f);
        }

        private void Toggle(UsePartData data)
        {
            if (IsOn.Value)
            {
                DisableRotor(Logger);
                return;
            }
            EnableRotor(Logger);
            data.successfullyUsedPart = true;
        }

        private void EnableRotor(I_MsgLogger logger)
        {
            IsOn.Value = true;
            if (Throttle_current.Value == 0f)
            {
                logger.Log(Loc.main.Engine_Module_State.InjectField(IsOn.Value.State_ToOnOff(), "state"));
            }
        }

        private void DisableRotor(I_MsgLogger logger)
        {
            bool wasIdle = Throttle_current.Value == 0f;
            IsOn.Value = false;
            if (wasIdle)
            {
                logger.Log(Loc.main.Engine_Module_State.InjectField(IsOn.Value.State_ToOnOff(), "state"));
            }
        }

        // Actuator-disk thrust: T = 0.5 * rho * A * (V_exit^2 - V0^2),
        // with V_exit = 2*Vd - V0 -- see https://www.grc.nasa.gov/www/k-12/airplane/propth.html
        private void FixedUpdate()
        {
            if (Rb2d == null || !IsOn.Value || Location == null || Location.planet == null)
            {
                return;
            }

            float density = (float)Location.planet.GetAtmosphericDensity(Location.Height);
            if (density <= 0f)
            {
                return;
            }

            Vector2 thrustAxis = Base.up;
            float v0 = Vector2.Dot(Location.velocity.ToVector2, thrustAxis);

            float vExit = 2f * _diskVelocity - v0;
            float thrust = 0.5f * density * _diskArea * (vExit * vExit - v0 * v0);
            if (thrust <= 0f)
            {
                return;
            }

            Rb2d.AddForceAtPosition(thrustAxis * thrust, transform.position, ForceMode2D.Force);
        }
    }
}
