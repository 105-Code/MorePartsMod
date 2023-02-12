using MorePartsMod.Parts.Types;
using SFS.Parts;
using SFS.Parts.Modules;
using SFS.Translations;
using SFS.Variables;
using SFS.World;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SFS.World.Rocket;


namespace MorePartsMod.Parts
{
    class HingeModule : BaseModule, INJ_Rocket, I_PartMenu
    {

        private VariableList<double>.Variable _max_opening;
        private VariableList<double>.Variable _opening_velocity;
        private VariableList<double>.Variable _opening;
        private OrientationModule _orientation;
        private HingeGroup _topGroup;
        private bool _isMoving;
        private bool _isClosing;
        private Transform _connector;
        private bool _validGroup;
        private const float radians = 0.01745f;
        public float OpeningVelocity { set => this._opening_velocity.Value = value; get => (float)this._opening_velocity.Value; }
        public float MaxOpening { set => this._max_opening.Value = value; get => (float)this._max_opening.Value; }
        public Rocket Rocket { set; get; }

   

        public override void Awake()
        {
            base.Awake();
            this._orientation = this.Part.orientation;
            this._opening = this.getDoubleVariable("opening");
            this._max_opening = this.getDoubleVariable("max_opening");
            this._opening_velocity = this.getDoubleVariable("opening_velocity");
            this.Part.onPartUsed.AddListener(this.OnPartUsed);
            this._topGroup = new HingeGroup();
            this._isMoving = false;
            this._isClosing = false;
            this._connector = this.transform.Find("Connector");
        }

        private void Start()
        {
            if (GameManager.main == null)
            {
                base.enabled = false;
                return;
            }

            this._topGroup.basePart = this.getTopParts().ToArray();
            this._validGroup = this.getTopPartGroup(this._topGroup.basePart);
            
            this._isClosing = this._opening.Value >= this._max_opening.Value;
            if (this._isClosing)
            {
                this._connector.transform.localEulerAngles = new Vector3(0, 0, -90 + (float)this._opening.Value);
            }
        }

        private void Update()
        {
            if (GameManager.main == null)
            {
                base.enabled = false;
                return;
            }

            if (this._isMoving)
            {
                this.MoveParts();
                
            }

        }

        private Vector2 getPartRotationVector()
        {
            Orientation hingeOrientaion = this._orientation.orientation.Value;
            float orientationX=hingeOrientaion.x, z=hingeOrientaion.z,x,y;
            
            z = z* radians;
            x = Mathf.Cos(z);
            y = Mathf.Sin(z);
            Vector2 result = new Vector2(x, y);

            if (orientationX < 0)
            {
                return -result;
            }  
            return result;
        }

        private void MoveParts()
        {
            Orientation partOrientation;
            Quaternion rotation;
            Vector3 hingePosition = this.Part.transform.localPosition;
            Orientation orientation = this._orientation.orientation.Value;
            Vector2 hingeRotation = this.getPartRotationVector();
            hingePosition.x += hingeRotation.x * 0.25f;
            hingePosition.y += hingeRotation.y * 0.25f;
           
            if (this._isClosing)
            {
                this._opening.Value -= this.OpeningVelocity;
                rotation = Quaternion.AngleAxis(this.OpeningVelocity, new Vector3(0, 0, orientation.y * orientation.x * - 1) );
            }
            else
            {
                this._opening.Value += this.OpeningVelocity;
                rotation = Quaternion.AngleAxis(this.OpeningVelocity, new Vector3(0, 0, orientation.y * orientation.x * 1));
            }
            

            foreach (Part part in this._topGroup.getParts())
            {
                partOrientation = part.orientation.orientation.Value;
                if (this._isClosing)
                {
                    partOrientation.z -= orientation.y * orientation.x * this.OpeningVelocity;
                }
                else
                {
                    partOrientation.z += orientation.y * orientation.x * this.OpeningVelocity;
                }

                part.transform.localEulerAngles = new Vector3(0,0, partOrientation.z);
                // movement
                Vector3 hingeToPart = part.transform.localPosition - hingePosition;
                part.transform.localPosition = (rotation* hingeToPart) + hingePosition;
            }
            // el -90 puede arreglarse si cambio la parte para que este en 0 grados de rotación
            this._connector.localEulerAngles = new Vector3(0,0, -90+(float) this._opening.Value);

            if(this._opening.Value >= this.MaxOpening || this._opening.Value <= 0)
            {
                this._isClosing = !this._isClosing;
                this._isMoving = false;
            }
           
        }

        private List<Part> getTopParts()
        {
            List<Part> result = new List<Part>();
            if (this.Rocket == null)
            {
                Debug.Log("Rocket null");
                return result;
            }
            Vector2 rotationPos = this.getPartRotationVector();
            foreach (PartJoint partJoint in this.Rocket.jointsGroup.GetConnectedJoints(this.Part))
            {
                if (partJoint.a == this.Part)
                {
                    if (this.IsTopPart(partJoint.anchor, rotationPos))
                    {
                        result.Add(partJoint.GetOtherPart(this.Part));
                    }
                    continue;
                }

                if (this.IsTopPart(partJoint.anchor * -1, rotationPos))
                {
                    result.Add(partJoint.GetOtherPart(this.Part));
                }
            }
            return result;
        }
 
        private bool IsTopPart(Vector2 anchor, Vector2 rotation)
        {
            return Vector2.Dot(anchor, rotation) >= 0.03;
        }

        private bool getTopPartGroup(Part[] toSearch)
        {
            bool thereIsLoop = false;
            bool isBaseGroup = false;
            foreach(Part part in toSearch)
            {
                if(this._topGroup.ExistInGroup(part))
                {
                    // already exist in the group

                    continue;
                }

                List<PartJoint> nextJoints = this.Rocket.jointsGroup.GetConnectedJoints(part);
                if (!this._topGroup.ExistInBaseGroup(part))
                {
                    // it's not part of the base part
                    if (nextJoints.Any(item => item.GetOtherPart(part) == this.Part))
                    {
                        // it's connected to hinge part
                        return false; // there is a loop
                    }
                }
                else
                {
                    isBaseGroup = true;
                }
                // add in the group to prevent loops
                this._topGroup.AddPartToGroup(part);
                thereIsLoop = this.getTopPartGroup(this.getPartFromPartJoint(nextJoints, part, isBaseGroup));
                if (!thereIsLoop)
                {
                    return thereIsLoop;
                }
            }
            return true;
        }

        private Part[] getPartFromPartJoint(List<PartJoint> nextJoints, Part part, bool isBaseGroup)
        {
            List<Part> parts = new List<Part>();
            Part otherPart;
            foreach (PartJoint joint in nextJoints)
            {
                otherPart = joint.GetOtherPart(part);
                if (isBaseGroup && otherPart == this.Part)
                {
                    continue;
                }
                parts.Add(otherPart);
            }
            return parts.ToArray();
        }

		public void OnPartUsed(UsePartData data)
        {
            if (this._validGroup)
            {
                this._isMoving = true;
            }
            data.successfullyUsedPart = true;
		}

        public void Draw(StatsMenu drawer, PartDrawSettings settings)
        {
            if (settings.build)
            {
                float GetMaxOpeningPercentage() => (float)(this.MaxOpening / 360);
                float GetMaxVelocityPercentage() => (float)(this._opening_velocity.Value / 10);
                drawer.DrawSlider(1, () => "Max Opening: "+  (int)this.MaxOpening, () => "", GetMaxOpeningPercentage, this.UpdateMaxOpeningValue, update => this._max_opening.onValueChange += update, update => this._max_opening.onValueChange -= update);
                drawer.DrawSlider(2, () => "Opening Velocity: " + (int)this._opening_velocity.Value, () => "", GetMaxVelocityPercentage, this.UpdateOpeningVelocityValue, update => this._opening_velocity.onValueChange += update, update => this._opening_velocity.onValueChange -= update);
            }
        }

        private void UpdateMaxOpeningValue(float newValue, bool value)
        {
            this.MaxOpening = newValue * 360;
        }

        private void UpdateOpeningVelocityValue(float newValue, bool value)
        {
            this._opening_velocity.Value = newValue * 10;
        }

        private class HingeGroup
        {
            public List<Part> parts;
            public Part[] basePart;

            public HingeGroup()
            {
                this.parts = new List<Part>();
                this.basePart = new Part[] { };
            }

            public bool ExistInGroup(Part part)
            {
                return this.parts.Any(item => item == part);
            }

            public bool ExistInBaseGroup(Part part)
            {
                return this.basePart.Any(item => item == part);
            }

            public void AddPartToGroup(Part part)
            {
   
                this.parts.Add(part);
            }

            public IEnumerable<Part> getParts() {
                List<Part> result = new List< Part>();
                Part[] parts = this.parts.ToArray();
                for (int index = 0; index < parts.Length; index++)
                {
                    result.Add(this.parts[index]);
                }
                return result;
            }

        }
  

    }
}
