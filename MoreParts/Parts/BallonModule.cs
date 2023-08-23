
using SFS.Parts;
using SFS.Parts.Modules;
using SFS.UI;
using SFS.Variables;
using SFS.World;
using System;
using UnityEngine;
using static SFS.World.Rocket;

namespace MorePartsMod.Parts
{
    public class BalloonModule : MonoBehaviour, INJ_Location, INJ_Physics
    {
        public Double_Reference State;
        public Double_Reference TargetState;
        public Part Part;
        public OrientationModule Orientation;
        public Transform DeployedBallon;
        public Location Location { get; set; }
        public Rigidbody2D Rb2d { get; set; }


        private double _volumn = 1.33f * Math.PI * Math.Pow(300, 3);
        private bool _isOpen;

        public void Awake()
        {
            this._isOpen = false;
            this.Part.onPartUsed.AddListener(this.Deploy);
        }

        private void Start()
        {
            if (GameManager.main == null)
            {
                base.enabled = false;
                return;
            }
            this._isOpen = this.State.Value == 1;
            TargetState.OnChange += this.UpdateEnabled;
        }

        private void FixedUpdate()
        {
            if (!this._isOpen || this.Location.planet == null || this.Location == null)
            {
                return;
            }

            Vector2 force;

            float airDensity = (float)this.Location.planet.GetAtmosphericDensity(this.Location.Height);
            float gravity = (float)this.Location.planet.GetGravity(this.Location.position).y * -1;
            float ascensionForce = ((airDensity * gravity * (float)this._volumn) - this.Rb2d.mass * gravity * 1000) / 1000;
            float aceleration = (float)Math.Sqrt(ascensionForce / 0.5f * this.Rb2d.mass);

            if (this.Location.VerticalVelocity < 30)
                force = DeployedBallon.transform.TransformVector(Vector2.up * (aceleration - gravity));
            else
                force = DeployedBallon.transform.TransformVector(Vector2.up);

            Vector2 relativePoint = this.Rb2d.GetRelativePoint(Transform_Utility.LocalToLocalPoint(base.transform, this.Rb2d, new Vector2(-0.5f, -0.047f)));
            this.Rb2d.AddForceAtPosition(force, relativePoint, ForceMode2D.Force);
        }

        private void LateUpdate()
        {
            if (GameManager.main == null || this.Location.planet == null || this.Location == null)
            {
                return;
            }

            float newRotation = (Orientation.orientation.Value.z * -1 * Orientation.orientation.Value.x) - (this.Rb2d.transform.localEulerAngles.z * Orientation.orientation.Value.x);
            DeployedBallon.localEulerAngles = new Vector3(1, 1, newRotation + Mathf.Sin(Time.time) * 3f * DeployedBallon.parent.lossyScale.x * DeployedBallon.parent.lossyScale.y);
        }

        public void Deploy(UsePartData data)
        {
            bool flag = false;

            if (this.TargetState.Value == 0f && this.State.Value == 0f)
            {
                if (!this.Location.planet.HasAtmospherePhysics || this.Location.Height > this.Location.planet.AtmosphereHeightPhysics * 0.9)
                {
                    MsgDrawer.main.Log("Not atmosphere");
                    flag = false;
                }
                else
                {
                    MsgDrawer.main.Log("Deployed");
                    this.TargetState.Value = 1f;
                    flag = true;
                }


            }
            else if (this.TargetState.Value == 1f && this.State.Value == 1f)
            {
                MsgDrawer.main.Log("Cut");
                this.TargetState.Value = 2f;
                this.State.Value = 2f;
                flag = true;
            }
            else if (this.TargetState.Value == 2f)
            {
                flag = true;
            }

            data.successfullyUsedPart = flag;
        }

        private void UpdateEnabled()
        {
            base.enabled = this._isOpen = (this.TargetState.Value == 1f);
        }
    }
}
