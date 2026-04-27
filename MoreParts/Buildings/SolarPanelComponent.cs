
using SFS.Parts.Modules;
using SFS.World;
using UnityEngine;
using static MorePartsMod.Buildings.ColonyComponent;

namespace MorePartsMod.Buildings
{
    class SolarPanelComponent : MonoBehaviour, INJ_Rocket, OnInit, INJ_Colony, INJ_Building
    {

        public Rocket Rocket { get; set; }
        public ColonyComponent Colony { get; set; }
        public Building Building { get; set; }

        public void OnInit()
        {
            foreach (Building item in Colony.Data.GetBuildings())
            {
                if (Vector2.Distance(Building.position, item.position) > 100)
                {
                    continue;
                }

                INJ_HasEnergy buildingNeedEnergy = item.GameObject.GetComponent<INJ_HasEnergy>();
                if (buildingNeedEnergy == null)
                {
                    continue;
                }
                buildingNeedEnergy.HasEnergy = true;
            }
        }
    }
}
