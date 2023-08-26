
using SFS.Parts.Modules;
using SFS.World;
using UnityEngine;
using static MorePartsMod.Buildings.ColonyComponent;

namespace MorePartsMod.Buildings
{
    class RefineryComponent : MonoBehaviour, INJ_Rocket, INJ_HasEnergy
    {

        private ResourceModule _rocketTanks;

        public Rocket Rocket { set; get; }
        public bool HasEnergy { set; get; }

        private void FixedUpdate()
        {
            if(Rocket == null || !HasEnergy){
                return;
            }

            if (this._rocketTanks == null)
            {
                this._rocketTanks = this.GetRocketTanks();
                return;
            }

            if (this._rocketTanks.resourcePercent.Value == 1.0)
            {
                return;
            }

            this._rocketTanks.AddResource(0.005 * WorldTime.main.timewarpSpeed);
        }

        private ResourceModule GetRocketTanks()
        {
            foreach (ResourceModule resource in this.Rocket.resources.globalGroups)
            {
                if (resource.resourceType.name == "Liquid_Fuel")
                {
                    return resource;
                }
            }

            return null;
        }
    }
}
