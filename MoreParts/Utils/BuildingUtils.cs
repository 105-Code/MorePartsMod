using SFS.World;
using SFS.World.Maps;
using SFS.WorldBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MorePartsMod.Utils
{
    public class BuildingUtils
    {

        public static void AddBuildingToWorld(GameObject building, Planet planet, Double2 position)
        {
            WorldLocation location = building.GetComponent<WorldLocation>();
            location.Value = new Location(planet, position, default(Double2));
        }

        public static void AddBuildingToWorld(GameObject building, Planet planet, Double2 position, float angleRad)
        {
            WorldLocation location = building.GetComponent<WorldLocation>();
            location.Value = new Location(planet, position, default(Double2));
            building.transform.eulerAngles = new Vector3(0, 0, (angleRad - 1.5708f)* Mathf.Rad2Deg);
        }

        public static Double2 GetPositionOnPlanetSurface(float angleRad, Planet planet)
        {
            return Double2.CosSin(angleRad) * (planet.Radius + planet.GetTerrainHeightAtAngle(angleRad));
        }

        public static float FindAngleForXDistance(Double2 form,float distance)
        {
            return (float) Math.Atan(distance/form.magnitude);
        }

    }
}
