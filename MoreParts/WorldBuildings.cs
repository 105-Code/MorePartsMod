using SFS.World;
using SFS.WorldBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MorePartsMod
{
    public class WorldBuildings
    {
        public static void addBuildingToWorld(GameObject building,string holder,Planet planet,Double2 position)
        {
            WorldLocation location = building.AddComponent<WorldLocation>();

            WorldLoader worldLoader = building.AddComponent<WorldLoader>();
            worldLoader.location = location; // where is the antenna placed
            worldLoader.loadDistance = 8000; // max render distance
            worldLoader.holder = building.transform.FindChild(holder).gameObject;

            StaticWorldObject buildingObject = building.AddComponent<StaticWorldObject>();
            buildingObject.location = location;

            // set the antenna in the world
            buildingObject.location.Value = new Location(planet, position, default(Double2));
        }

    }
}
