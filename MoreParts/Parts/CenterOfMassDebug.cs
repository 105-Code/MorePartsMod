using SFS.World;
using UnityEngine;

namespace MorePartsMod.Parts
{
    public class CenterOfMassDebug : MonoBehaviour
    {
        private const float MarkerSize = 1f;

        private Rocket _rocket;
        private LineRenderer _horizontal;
        private LineRenderer _vertical;

        private void Awake()
        {
            _rocket = GetComponent<Rocket>();
            var mat = new Material(Shader.Find("Sprites/Default"));
            _horizontal = CreateLine(mat);
            _vertical   = CreateLine(mat);
        }

        private LineRenderer CreateLine(Material mat)
        {
            var child = new GameObject("CoMMarker");
            child.transform.SetParent(transform, false);
            var lr = child.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.startWidth = 0.2f;
            lr.endWidth = 0.2f;
            lr.material = mat;
            lr.startColor = Color.magenta;
            lr.endColor = Color.magenta;
            return lr;
        }

        private void LateUpdate()
        {
            if (_rocket == null || _rocket.rb2d == null || !_rocket.rb2d.simulated || !KeySettings.Main.Show_center_of_mass)
            {
                _horizontal.enabled = false;
                _vertical.enabled = false;
                return;
            }

            Vector2 com = _rocket.rb2d.worldCenterOfMass;
            _horizontal.SetPosition(0, new Vector3(com.x - MarkerSize, com.y, 0f));
            _horizontal.SetPosition(1, new Vector3(com.x + MarkerSize, com.y, 0f));
            _vertical.SetPosition(0, new Vector3(com.x, com.y - MarkerSize, 0f));
            _vertical.SetPosition(1, new Vector3(com.x, com.y + MarkerSize, 0f));
            _horizontal.enabled = true;
            _vertical.enabled = true;
        }
    }
}
