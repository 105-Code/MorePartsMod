using SFS.Variables;
using UnityEngine;

namespace MorePartsMod.Utils
{
    // Attach to the pivot GO (the hinge point). The visual mesh GO should be a child,
    // offset so it orbits this transform when Angle changes.
    public class VisualRotate : MonoBehaviour
    {
        public Float_Reference Angle; // degrees on local Z axis

        private void Start()
        {
            Angle.OnChange += ApplyRotation;
            ApplyRotation();
        }

        private void OnDestroy()
        {
            Angle.OnChange -= ApplyRotation;
        }

        private void ApplyRotation()
        {
            Vector3 euler = transform.localEulerAngles;
            //euler.x = transform.localEulerAngles.x;
            euler.z = Angle.Value;
            //euler.y = -Angle.Value;
            transform.localEulerAngles = euler;
        }
    }
}
