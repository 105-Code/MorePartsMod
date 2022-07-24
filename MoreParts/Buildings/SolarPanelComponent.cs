

using MorePartsMod.Managers;
using SFS.Parts.Modules;
using SFS.World;
using UnityEngine;
using static MorePartsMod.Buildings.ColonyComponent;

namespace MorePartsMod.Buildings
{
    class SolarPanelComponent : MonoBehaviour, INJ_PlayerNear, INJ_Rocket
    {

        private bool _playerNear;
        private ResourceModule _rocketBateries;
        private Rocket _rocket;

        public bool PlayerNear { 
            set
            {
                this._rocketBateries = null;
                this._playerNear = value;
            } 
        }

        public Rocket Rocket { set => this._rocket = value; }

        private void FixedUpdate()
        {
            if(!this._playerNear)
            {
                return;
            }

            if(this._rocketBateries == null)
            {
                this._rocketBateries = this.GetRocketBateries();
                return;
            }

            if (this._rocketBateries.resourcePercent.Value == 1.0)
            {
                return;
            }
 
            this._rocketBateries.AddResource(0.02);
        }

        private ResourceModule GetRocketBateries()
        {
            foreach (ResourceModule resource in this._rocket.resources.globalGroups)
            {
                if (resource.resourceType.name == "Electricity_Resource")
                {
                    return resource; 
                }
            }

            return null;
        }

        public static void Setup(GameObject colonyPrefab)
        {
            GameObject building = colonyPrefab.transform.FindChild("Holder").gameObject.transform.FindChild("Solar Panels").gameObject;
            building.AddComponent<SolarPanelComponent>();
        }
    }
}
