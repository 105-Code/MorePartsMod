

using MorePartsMod.Managers;
using SFS.Parts.Modules;
using SFS.World;
using UnityEngine;
using static MorePartsMod.Buildings.ColonyComponent;

namespace MorePartsMod.Buildings
{
    class RefineryComponent : MonoBehaviour, INJ_PlayerNear, INJ_PlayerInPlanet, INJ_Rocket
    {

        private bool playerNear;
        private bool playerInPlanet;
        private ResourceModule rocketTanks;
        private Rocket rocket;

        public bool PlayerNear { 
            set
            {
                this.rocketTanks = null;
                this.playerNear = value;
            } 
        }
        public bool PlayerInPlanet {
            set
            {
                this.rocketTanks = null;
                this.playerInPlanet = value;
            }
        }

        public Rocket Rocket { set => this.rocket = value; }

        private void FixedUpdate()
        {
            if(!this.playerNear || !this.playerInPlanet)
            {
                return;
            }

            if(this.rocketTanks == null)
            {
                this.rocketTanks = this.GetRocketTanks();
                return;
            }

            if (this.rocketTanks.resourcePercent.Value == 1.0)
            {
                return;
            }
 
            this.rocketTanks.AddResource(0.02);
        }

        private ResourceModule GetRocketTanks()
        {
            foreach (ResourceModule resource in this.rocket.resources.globalGroups)
            {
                if (resource.resourceType.name == "Liquid_Fuel")
                {
                    return resource; 
                }
            }

            return null;
        }

        public static void Setup(GameObject colonyPrefab)
        {
            GameObject building = colonyPrefab.transform.FindChild("Holder").gameObject.transform.FindChild("Refinery").gameObject;
            building.AddComponent<RefineryComponent>();
        }
    }
}
