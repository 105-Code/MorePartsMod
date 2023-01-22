using MorePartsMod.Parts.Types;
using SFS.Parts;
using SFS.Parts.Modules;
using SFS.Variables;
using SFS.World;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SFS.World.Rocket;


namespace MorePartsMod.Parts
{
    class HingeModule : BaseModule, INJ_Rocket
    {

        private VariableList<double>.Variable _max_opening;
        private VariableList<double>.Variable _opening;
        private OrientationModule _orientation;
        private HingeGroup _topGroup;
        private bool _isMoving;
        private bool _isClosing;
        private Transform _connector;
        private float _openingVelocity;
        private bool _validGroup;

        public Rocket Rocket { set; get; }

        public override void Awake()
        {
            base.Awake();
            this._orientation = this.Part.orientation;
            this._opening = this.getDoubleVariable("opening");
            this._max_opening = this.getDoubleVariable("max_opening");
            this.Part.onPartUsed.AddListener(this.OnPartUsed);
            this._topGroup = new HingeGroup();
            this._isMoving = false;
            this._isClosing = true;
            this._connector = this.transform.Find("Connector");
            this._openingVelocity = 2;
        }

        private void Start()
        {
            
            if(this._max_opening.Value > 180)
            {
                this._max_opening.Value = 180;
            }

            this._topGroup.basePart = this.getTopParts(this.getPartRotation()).ToArray();
            this._validGroup = this.getTopPartGroup(this._topGroup.basePart);
            
            this._isClosing = this._opening.Value >= this._max_opening.Value;
            if (this._isClosing)
            {
                this._connector.transform.localEulerAngles = new Vector3(0, 0, (float)this._opening.Value);
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

        private float getPartRotation()
        {
            if (this._orientation.orientation.Value.y < 0)
            {
                return Mathf.Abs(180 - this._orientation.orientation.Value.z);
            }
            return this._orientation.orientation.Value.z;
        }

        private Orientation getAbsoluteOrientation()
        {
            float x = this._orientation.orientation.Value.x, y = this._orientation.orientation.Value.y, z = this._orientation.orientation.Value.z;

            if(z == 90)
            {
                y = 0;
                if (x < 0)
                {
                    x = 1;
                }
                else
                {
                    x = -1;
                }
                
            }

            if (z == 270)
            {
                y = 0;
                if(x < 0)
                {
                    x = 1;
                }
            }


            if (z == 180)
            {
                if (y > 0)
                {
                    y = -1;
                }
                else
                {
                    y = 1;
                }
                x = y * x;
            }

            return new Orientation(x,y,z);
        }

        private void MoveParts()
        {
            // esto es una mierda de código y lo tengo que arreglar en el futuro.

            Orientation partOrientation;
            Part part;
            Quaternion rotation;
            Vector3 hingePosition = this.Part.transform.localPosition;
            Orientation hingeOrientation = this.getAbsoluteOrientation();
            if (hingeOrientation.y == 0)
            {
                hingePosition.x += hingeOrientation.x * 0.25f;
            }
            hingePosition.y += hingeOrientation.y * 0.25f;


            if (!this._isClosing)
            {
                this._opening.Value -= this._openingVelocity;
                //rotation = Quaternion.AngleAxis(this._openingVelocity, Vector3.back);
                if(hingeOrientation.x > 0)
                {
                    if (hingeOrientation.y == 0 && hingeOrientation.z == 90 || hingeOrientation.z == 270)
                    {
                        rotation = Quaternion.AngleAxis(this._openingVelocity, Vector3.forward);
                    }
                    else
                    {
                        if (hingeOrientation.y < 0)
                        {
                            rotation = Quaternion.AngleAxis(this._openingVelocity, Vector3.forward);
                        }
                        else
                        {
                            rotation = Quaternion.AngleAxis(this._openingVelocity, Vector3.back);
                        }
                    }
                    
                }
                else
                {
                    if (hingeOrientation.y == 0 && hingeOrientation.z == 90 || hingeOrientation.z == 270)
                    {
                        rotation = Quaternion.AngleAxis(this._openingVelocity, Vector3.back);
                    }
                    else
                    {
                        if (hingeOrientation.y < 0)
                        {
                            rotation = Quaternion.AngleAxis(this._openingVelocity, Vector3.back);
                        }
                        else
                        {
                            rotation = Quaternion.AngleAxis(this._openingVelocity, Vector3.forward);
                        }
                    }
                }

            }
            else
            {
                this._opening.Value += this._openingVelocity;
                if (hingeOrientation.x > 0)
                {
                    if(hingeOrientation.y == 0 && hingeOrientation.z == 90 || hingeOrientation.z == 270)
                    {
                        rotation = Quaternion.AngleAxis(this._openingVelocity, Vector3.back);
                    }
                    else
                    {
                        if (hingeOrientation.y < 0)
                        {
                            rotation = Quaternion.AngleAxis(this._openingVelocity, Vector3.back);
                        }
                        else
                        {
                            rotation = Quaternion.AngleAxis(this._openingVelocity, Vector3.forward);
                        }
                    }
                }
                else
                {
                    if (hingeOrientation.y == 0 && hingeOrientation.z == 90 || hingeOrientation.z == 270)
                    {
                        rotation = Quaternion.AngleAxis(this._openingVelocity, Vector3.forward);
                    }
                    else
                    {
                        if (hingeOrientation.y <= 0)
                        {
                            rotation = Quaternion.AngleAxis(this._openingVelocity, Vector3.forward);
                        }
                        else
                        {
                            rotation = Quaternion.AngleAxis(this._openingVelocity, Vector3.back);
                        }
                    }
                }
            }
            
            float y = hingeOrientation.y == 0? hingeOrientation.z == 90 || hingeOrientation.z == 270? -1: 1: hingeOrientation.y;
            foreach (Tuple<Vector2,Part> value in this._topGroup.getParts())
            {
                part = value.Item2;
                partOrientation = part.orientation.orientation.Value;

                if (!this._isClosing)
                {
                    partOrientation.z -= y * hingeOrientation.x*  this._openingVelocity;
                }
                else
                {
                    partOrientation.z += y * hingeOrientation.x *  this._openingVelocity;
                }
     
                part.transform.localEulerAngles = new Vector3(0,0, partOrientation.z);

                // movement
                Vector3 hingeToPart = part.transform.localPosition - hingePosition;
                part.transform.localPosition = (rotation* hingeToPart) + hingePosition;
            }

            this._connector.localEulerAngles = new Vector3(0,0, (float) this._opening.Value);


            if (this._opening.Value >= this._max_opening.Value || this._opening.Value <= 0)
            {
                this._isMoving = false;
            }
        }

        private List<Part> getTopParts(float z)
        {
            List<Part> result = new List<Part>();
            if (this.Rocket == null)
            {
                Debug.Log("Rocket null");
                return result;
            }
          
            foreach (PartJoint partJoint in this.Rocket.jointsGroup.GetConnectedJoints(this.Part))
            {
                if (partJoint.a == this.Part)
                {
                    if (this.IsTopPart(partJoint.anchor, z))
                    {
                        result.Add(partJoint.GetOtherPart(this.Part));
                    }
                    continue;
                }

                if (this.IsTopPart(partJoint.anchor * -1, z))
                {
                    result.Add(partJoint.GetOtherPart(this.Part));
                }
            }
            return result;
        }
 
        private bool IsTopPart(Vector2 anchor, float z)
        {
            if(z == 0 || z ==  360)
            {
                return anchor.y > 0;
            }

            if (z == 90)
            {
                return anchor.x < 0;
            }

            if (z == 180)
            {
                return anchor.y < 0;
            }

            if (z == 270)
            {
                return anchor.x > 0;
            }

            return false;
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
                this._isClosing = !this._isClosing;
            }
            data.successfullyUsedPart = true;
		}

        private class HingeGroup
        {
            public List<Part> parts;
            public List<Vector2> initialPositions;
            public Part[] basePart;

            public HingeGroup()
            {
                this.parts = new List<Part>();
                this.basePart = new Part[] { };
                this.initialPositions = new List<Vector2>();
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
                this.initialPositions.Add(part.transform.localPosition);
                this.parts.Add(part);
            }

            public IEnumerable<Tuple<Vector2,Part>> getParts() {
                List<Tuple<Vector2, Part>> result = new List<Tuple<Vector2, Part>>();
                Part[] parts = this.parts.ToArray();
                for (int index = 0; index < parts.Length; index++)
                {
                    result.Add(Tuple.Create( this.initialPositions[index], this.parts[index] ));
                }
                return result;
            }

        }
  

    }
}
