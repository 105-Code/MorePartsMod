using SFS.Parts.Modules;
using SFS.World;
using UnityEngine;
using static MorePartsMod.Buildings.ColonyComponent;

namespace MorePartsMod.Buildings
{
    class VABComponent : MonoBehaviour, INJ_PlayerNear, INJ_Rocket, INJ_Colony
    {

        private Rocket _rocket;
        private ColonyComponent _colony;
        private bool _alreadyTook;

        public bool PlayerNear { 
            set {
                if (!this.isActiveAndEnabled)
                {
                    return;
                }

                if (!value)
                {
                    this._alreadyTook = false;
                    return;
                }

                if ( this._alreadyTook)
                {
                    return;
                }
                this._alreadyTook = true;
                this.ReduceRocketPartsCargo();
            } 
        }

        public Rocket Rocket { set => this._rocket = value; }
        public ColonyComponent Colony { set => this._colony = value; }

        private void ReduceRocketPartsCargo()
        {
            foreach (ResourceModule resource in this._rocket.resources.globalGroups)
            {
                if (resource.resourceType.name == "Rocket_Material")
                {
                    double resourceQuantity = resource.ResourceAmount;
                    resource.TakeResource(resourceQuantity);
                    this._colony.data.rocketParts += resourceQuantity;
                }
            }

        }

        public static void Setup(GameObject colonyPrefab)
        {
            GameObject building = colonyPrefab.transform.FindChild("Holder").gameObject.transform.FindChild("VAB").gameObject;
            building.AddComponent<VABComponent>();
        }
    }
}
