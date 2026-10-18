
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

        // Re-injected by SFS whenever the rocket is rebuilt (docking, separation, part destroyed),
        // so the top group is refreshed against the new joint graph.
        public Rocket Rocket
        {
            get => _rocket;
            set
            {
                _rocket = value;
                if (_topGroup != null)
                    _topGroup = RefreshTopParts();
            }
        }
        public Part Part;

        private Rocket _rocket;

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

            // Positions and orientations of the top parts are saved already rotated, so nothing is re-applied here.
            _topGroup = CollectTopParts();
            _isClosing = Opening.Value >= MaxOpening.Value;
            _connector.localEulerAngles = new Vector3(0, 0, -90 + Opening.Value);
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
        // We convert its world position to partHolder local space so it matches part.Position.
        private Vector2 GetPivotLocalPosition()
        {
            return Part.transform.parent.InverseTransformPoint(_connector.position);
        }

        // Geometric classification, used on Start. Undoes the current opening so a top neighbor is found
        // where it sat with the hinge closed; a neighbor still clearly below the hinge stays a base part.
        private bool IsOnTopSide(Part neighbor)
        {
            Vector2 up = GetHingeUpDirection();
            Orientation o = _orientation.orientation.Value;
            Quaternion undo = Quaternion.AngleAxis(-Opening.Value * o.x * o.y, Vector3.forward);
            Vector2 pivot = GetPivotLocalPosition();

            Vector2 current = neighbor.Position - Part.Position;
            Vector2 closed = pivot + (Vector2)(undo * (neighbor.Position - pivot)) - Part.Position;

            return Vector2.Dot(closed.normalized, up) > 0.5f && Vector2.Dot(current.normalized, -up) <= 0.5f;
        }

        private HashSet<Part> CollectTopParts()
        {
            if (Rocket == null) return new HashSet<Part>();

            var seeds = new List<Part>();
            foreach (PartJoint joint in Rocket.jointsGroup.GetConnectedJoints(Part))
            {
                Part neighbor = joint.GetOtherPart(Part);
                if (IsOnTopSide(neighbor))
                    seeds.Add(neighbor);
            }
            return ExpandTopParts(seeds);
        }

        // Rebuilds the top group after the rocket changed, keeping the hinge's direct top neighbors that are
        // still attached. Avoids re-classifying geometrically, which is ambiguous while the hinge is open.
        private HashSet<Part> RefreshTopParts()
        {
            if (Rocket == null) return new HashSet<Part>();

            var seeds = new List<Part>();
            foreach (PartJoint joint in Rocket.jointsGroup.GetConnectedJoints(Part))
            {
                Part neighbor = joint.GetOtherPart(Part);
                if (_topGroup.Contains(neighbor))
                    seeds.Add(neighbor);
            }
            return ExpandTopParts(seeds);
        }

        // BFS from the seeds, collecting every transitively connected part without crossing back through the hinge.
        private HashSet<Part> ExpandTopParts(List<Part> seeds)
        {
            var result = new HashSet<Part>();
            var pending = new List<Part>();
            foreach (Part seed in seeds)
            {
                if (result.Add(seed))
                    pending.Add(seed);
            }

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

        // Rotates the top parts around the pivot. Writes Position, Orientation and joint anchors (not just the
        // transform) because JointGroup.RepositionParts rebuilds the rocket from them on every dock/split.
        private void RotateTopParts(float angleDegrees)
        {
            Quaternion rotation = Quaternion.AngleAxis(angleDegrees, Vector3.forward);
            Vector2 pivot = GetPivotLocalPosition();

            foreach (Part part in _topGroup)
            {
                part.Position = pivot + (Vector2)(rotation * (part.Position - pivot));
                Orientation o = part.orientation.orientation.Value;
                part.orientation.orientation.Value = new Orientation(o.x, o.y, Mathf.Repeat(o.z + angleDegrees, 360f));
            }

            foreach (Part part in _topGroup)
            {
                foreach (PartJoint joint in Rocket.jointsGroup.GetConnectedJoints(part))
                    joint.anchor = joint.b.Position - joint.a.Position;
            }
        }

        private void MoveParts()
        {
            float delta = _isClosing ? -OpeningVelocity.Value : OpeningVelocity.Value;
            Orientation o = _orientation.orientation.Value;
            RotateTopParts(delta * o.x * o.y);

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
                _topGroup = RefreshTopParts();

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
