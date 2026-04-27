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

        public I_MsgLogger Logger
        {
            get
            {
                if (!this.IsPlayer)
                {
                    return new MsgNone();
                }
                return MsgDrawer.main;
            }
        }
        public Animator Animator;
        public Bool_Reference IsOn;
        public Float_Reference Throttle_current;
        public Float_Reference RPM;

        public Transform Base;
        public float Throttle { set => this.Throttle_current.Value = value; }

        private const float Radius = 0.6f;
        private float _area;
        private float _rotorVelocity;
        public Part Part;

        public Rigidbody2D Rb2d { set; get; }

        public Location Location { set; get; }
        public bool IsPlayer { set; get; }

        public void Awake()
        {
            Animator = this.GetComponent<Animator>();
            this._area = (float)(Math.PI * Radius * Radius);
        }

        private void Start()
        {
            if (GameManager.main == null)
            {
                base.enabled = false;
                return;
            }

            this.IsOn.OnChange += this.RecalculateRotor_throttle;
            this.Throttle_current.OnChange += this.RecalculateRotor_throttle;
            Part.onPartUsed.AddListener(this.Toggle);
        }

        private void RecalculateRotor_throttle()
        {
            Animator.speed = 100 * (float)this.Throttle_current.Value;
            this._rotorVelocity = (float)(((RPM.Value * this.Throttle_current.Value) / 60) * 2 * Math.PI);
            if (!this.IsOn.Value)
            {
                this.Throttle_current.Value = 0f;
                Animator.SetBool("isOn", false);
                return;
            }

            if (this.Throttle_current.Value > 0)
            {
                Animator.SetBool("isOn", true);
                return;
            }

            Animator.SetBool("isOn", false);
        }

        private void Toggle(UsePartData data)
        {
            if (this.IsOn.Value)
            {
                this.DisableRotor(this.Logger);
                return;
            }
            this.EnableRotor(this.Logger);

            data.successfullyUsedPart = true;
        }

        private void EnableRotor(I_MsgLogger logger)
        {
            this.IsOn.Value = true;

            if (this.Throttle_current.Value == 0f)
            {
                logger.Log(Loc.main.Engine_Module_State.InjectField(this.IsOn.Value.State_ToOnOff(), "state"));
            }
        }

        private void DisableRotor(I_MsgLogger logger)
        {
            bool flag = this.Throttle_current.Value == 0f;
            this.IsOn.Value = false;
            if (flag)
            {
                logger.Log(Loc.main.Engine_Module_State.InjectField(this.IsOn.Value.State_ToOnOff(), "state"));
            }
        }
        private void FixedUpdate()
        {
            if (this.Rb2d == null || !this.IsOn.Value || this.Location.planet == null)
            {
                return;
            }
            // check https://www.grc.nasa.gov/www/k-12/airplane/propth.html
            float exitVelocity = (float)((2 * this._rotorVelocity) - this.Location.VerticalVelocity);
            float density = (float)this.Location.planet.GetAtmosphericDensity(this.Location.GetTerrainHeight(true));
            double thrust = 0.5 * density * this._area * ((exitVelocity * exitVelocity) - (this.Location.VerticalVelocity * this.Location.VerticalVelocity));

            Vector2 force = Base.transform.TransformVector(Vector2.up * (float)thrust);
            Vector2 relativePoint = this.Rb2d.GetRelativePoint(Transform_Utility.LocalToLocalPoint(this.transform, this.Rb2d, new Vector2(0, 0)));
            this.Rb2d.AddForceAtPosition(force, relativePoint, ForceMode2D.Force);
        }

    }
}
