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

    }
}
