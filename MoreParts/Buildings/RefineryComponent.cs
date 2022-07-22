

using MorePartsMod.Managers;
using SFS.Parts.Modules;
using SFS.World;
using UnityEngine;
using static MorePartsMod.Buildings.ColonyComponent;

namespace MorePartsMod.Buildings
{
    class RefineryComponent : MonoBehaviour, INJ_PlayerNear, INJ_PlayerInPlanet, INJ_Rocket, INJ_HasEnergy
    {

        private bool _playerNear;
        private bool _playerInPlanet;
        private bool _hasEnergy;
        private ResourceModule _rocketTanks;
        private Rocket _rocket;

        public bool PlayerNear { 
            set
            {
                this._rocketTanks = null;
                this._playerNear = value;
            } 
        }
        public bool PlayerInPlanet {
            set
            {
                this._rocketTanks = null;
                this._playerInPlanet = value;
            }
        }

        public Rocket Rocket { set => this._rocket = value; }
        public bool HasEnergy { set => this._hasEnergy = value; }

        private void FixedUpdate()
        {
            if(!this._playerNear || !this._playerInPlanet || !this._hasEnergy)
            {
                return;
            }

            if(this._rocketTanks == null)
            {
                this._rocketTanks = this.GetRocketTanks();
                return;
            }

            if (this._rocketTanks.resourcePercent.Value == 1.0)
            {
                return;
            }
 
            this._rocketTanks.AddResource(0.02);
        }

        private ResourceModule GetRocketTanks()
        {
            foreach (ResourceModule resource in this._rocket.resources.globalGroups)
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
