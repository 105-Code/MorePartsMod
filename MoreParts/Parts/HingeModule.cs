
using SFS.Parts;
using SFS.Parts.Modules;
using SFS.Variables;
using SFS.World;
using System.Collections.Generic;
using UnityEngine;
using static SFS.World.Rocket;

namespace MorePartsMod.Parts
{
    public class HingeModule : MonoBehaviour, INJ_Rocket, I_PartMenu
    {
        public Float_Reference MaxOpening;
        public Float_Reference OpeningVelocity;
        public Float_Reference Opening;

        private OrientationModule _orientation;
        private HashSet<Part> _topGroup;
        private bool _isMoving;
        private bool _isClosing;
        private Transform _connector;

        private const float DegToRad = 0.01745329f;

        public Rocket Rocket { set; get; }
        public Part Part;

        public void Awake()
        {
            _orientation = Part.orientation;
            Part.onPartUsed.AddListener(OnPartUsed);
            _connector = transform.Find("Connector");
        }

        private void Start()
        {
            if (GameManager.main == null)
            {
                enabled = false;
                return;
            }

            _topGroup = CollectTopParts();
            _isClosing = Opening.Value >= MaxOpening.Value;
            _connector.localEulerAngles = new Vector3(0, 0, -90 + Opening.Value);

            if (Opening.Value > 0 && _topGroup.Count > 0)
                ApplyOpeningAngle(Opening.Value);
        }

        private void Update()
        {
            if (GameManager.main == null)
            {
                enabled = false;
                return;
            }

            if (_isMoving)
                MoveParts();
        }

        // Returns the unit vector pointing toward the hinge's "top" side in partHolder local space.
        // For a part at z=0 this is (1,0); the x-flip negates it so mirrored hinges open symmetrically.
        private Vector2 GetHingeUpDirection()
        {
            Orientation o = _orientation.orientation.Value;
            float rad = o.z * DegToRad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            return o.x < 0 ? -dir : dir;
        }

        // The pivot is the Connector child's origin — placed by the designer at the exact joint center.
        // We convert its world position to partHolder local space so it matches part.transform.localPosition.
        private Vector3 GetPivotLocalPosition()
        {
            return Part.transform.parent.InverseTransformPoint(_connector.position);
        }

        // BFS starting from the hinge's immediate "top" neighbors, collecting every transitively
        // connected part that should rotate with the hinge.
        private HashSet<Part> CollectTopParts()
        {
            var result = new HashSet<Part>();
            if (Rocket == null) return result;

            Vector2 up = GetHingeUpDirection();
            var pending = new List<Part>();

            // Seed: direct neighbors whose anchor points in the opening direction.
            foreach (PartJoint joint in Rocket.jointsGroup.GetConnectedJoints(Part))
            {
                Vector2 anchor = joint.GetRelativeAnchor(Part);
                if (Vector2.Dot(anchor.normalized, up) > 0.5f)
                {
                    Part neighbor = joint.GetOtherPart(Part);
                    if (result.Add(neighbor))
                        pending.Add(neighbor);
                }
            }

            // Expand to all parts reachable from the seed without crossing back through the hinge.
            while (pending.Count > 0)
            {
                Part current = pending[pending.Count - 1];
                pending.RemoveAt(pending.Count - 1);
                foreach (PartJoint joint in Rocket.jointsGroup.GetConnectedJoints(current))
                {
                    Part neighbor = joint.GetOtherPart(current);
                    if (neighbor == Part) continue;
                    if (result.Add(neighbor))
                        pending.Add(neighbor);
                }
            }

            return result;
        }

        // Applies a total rotation of angleDegrees around the pivot to all top parts.
        // Used on Start to restore a saved non-zero opening angle.
        private void ApplyOpeningAngle(float angleDegrees)
        {
            Orientation o = _orientation.orientation.Value;
            Quaternion rotation = Quaternion.AngleAxis(angleDegrees * o.x * o.y, Vector3.forward);
            Vector3 pivot = GetPivotLocalPosition();

            foreach (Part part in _topGroup)
            {
                part.transform.localPosition = pivot + rotation * (part.transform.localPosition - pivot);
                part.transform.localRotation = rotation * part.transform.localRotation;
            }
        }

        private void MoveParts()
        {
            float delta = _isClosing ? -OpeningVelocity.Value : OpeningVelocity.Value;
            Orientation o = _orientation.orientation.Value;
            float angleDelta = delta * o.x * o.y;

            Quaternion rotation = Quaternion.AngleAxis(angleDelta, Vector3.forward);
            Vector3 pivot = GetPivotLocalPosition();

            foreach (Part part in _topGroup)
            {
                part.transform.localPosition = pivot + rotation * (part.transform.localPosition - pivot);
                part.transform.localRotation = rotation * part.transform.localRotation;
            }

            Opening.Value += delta;
            _connector.localEulerAngles = new Vector3(0, 0, -90 + Opening.Value);

            if (Opening.Value >= MaxOpening.Value || Opening.Value <= 0)
            {
                _isClosing = !_isClosing;
                _isMoving = false;
            }
        }

        public void OnPartUsed(UsePartData data)
        {
            if (!_isMoving)
                _topGroup = CollectTopParts();

            if (_topGroup.Count > 0)
                _isMoving = true;

            data.successfullyUsedPart = true;
        }

        public void Draw(StatsMenu drawer, PartDrawSettings settings)
        {
            if (settings.build)
            {
                float GetMaxOpeningPercentage() => MaxOpening.Value / 360;
                float GetMaxVelocityPercentage() => OpeningVelocity.Value / 10;
                drawer.DrawSlider(1, () => "Max Opening: " + (int)MaxOpening.Value, () => "", GetMaxOpeningPercentage, UpdateMaxOpeningValue, update => MaxOpening.OnChange += update, update => MaxOpening.OnChange -= update);
                drawer.DrawSlider(2, () => "Opening Velocity: " + (int)OpeningVelocity.Value, () => "", GetMaxVelocityPercentage, UpdateOpeningVelocityValue, update => OpeningVelocity.OnChange += update, update => OpeningVelocity.OnChange -= update);
            }
        }

        private void UpdateMaxOpeningValue(float newValue, bool _) => MaxOpening.Value = newValue * 360;
        private void UpdateOpeningVelocityValue(float newValue, bool _) => OpeningVelocity.Value = newValue * 10;
    }
}
