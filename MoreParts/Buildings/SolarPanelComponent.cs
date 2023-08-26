
using SFS.Parts.Modules;
using SFS.World;
using UnityEngine;
using static MorePartsMod.Buildings.ColonyComponent;

namespace MorePartsMod.Buildings
{
    class SolarPanelComponent : MonoBehaviour, INJ_Rocket, OnInit, INJ_Colony, INJ_Building
    {
        private ResourceModule _rocketBateries;

        public Rocket Rocket { get; set; }
        public ColonyComponent Colony { get; set; }
        public Building Building { get; set; }

        public void OnInit()
        {
            foreach (Building item in Colony.Data.GetBuildings())
            {
                if (item.GameObject == Building.GameObject)
                {
                    continue;
                }

                if (Vector2.Distance(Building.position, item.position) > 100)
                {
                    continue;
                }

                INJ_HasEnergy buildingNeedEnergy = item.GameObject.GetComponent<INJ_HasEnergy>();
                if(buildingNeedEnergy == null){
                    return;
                }
                buildingNeedEnergy.HasEnergy = true;
            }
        }

        private void FixedUpdate()
        {
            if (Rocket == null)
            {
                return;
            }

            if (this._rocketBateries == null)
            {
                this._rocketBateries = this.GetRocketBateries();
                return;
            }

            if (this._rocketBateries.resourcePercent.Value == 1.0)
            {
                return;
            }

            this._rocketBateries.AddResource(0.005 * WorldTime.main.timewarpSpeed);
        }

        private ResourceModule GetRocketBateries()
        {
            foreach (ResourceModule resource in Rocket.resources.globalGroups)
            {
                if (resource.resourceType.name == "Electricity_Resource")
                {
                    return resource;
                }
            }

            return null;
        }

    }
}
