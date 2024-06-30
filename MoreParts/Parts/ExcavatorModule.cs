using MorePartsMod.Managers;
using MorePartsMod.Parts.Types;
using SFS.Parts;
using SFS.Parts.Modules;
using SFS.UI;
using SFS.Variables;
using SFS.World;
using UnityEngine;
using static MorePartsMod.World.PlanetResourceData;
using static SFS.World.Rocket;

namespace MorePartsMod.Parts
{
    public class ExcavatorModule : ElectricalModule, INJ_Location, INJ_Rocket
    {

        public Float_Reference TargetState;
        public Float_Reference State;
        public Float_Reference FlowRate;

        public Location Location { set; get; }
        public Rocket Rocket { set; get; }

        private const double _extractionCount = 0.009f;

        private ResourceModule _material_container;

        public Transform ExcavatorObject;
        public Part Part;

        private void Start()
        {
            if (GameManager.main == null)
            {
                base.enabled = false;
                return;
            }
            this._material_container = this.GetMaterialContainer();

            this.Part.onPartUsed.AddListener(this.Toggle);
            this.FlowModule.onStateChange += this.CheckOutOfFuel;
            this.CheckOutOfFuel();

            this.ExcavatorObject = this.transform.Find("Stick").Find("Excavator");
        }

        private ResourceModule GetMaterialContainer()
        {
            if (this.Rocket == null || this.Rocket.resources == null)
            {
                return null;
            }

            foreach (ResourceModule resourceModule in this.Rocket.resources.globalGroups)
            {

                if (resourceModule.resourceType.name == MorePartsTypes.MATERIAL)
                {
                    return resourceModule;
                }
            }
            return null;
        }

        private void Update()
        {
            if (GameManager.main == null || this.State.Value == 0 || this.Location == null)
            {
                return;
            }

            if (this._material_container == null)
            {
                this._material_container = this.GetMaterialContainer();
                if (this._material_container == null)
                {
                    MsgDrawer.main.Log("You need a Material Container");
                    this.TargetState.Value = 0;
                    return;
                }
            }


            ReourceDeposit deposit = ResourcesManger.Main.CurrentDeposit;

            if (deposit == null || !deposit.Active)
            {
                MsgDrawer.main.Log("There is not Resource Deposit");
                this.TargetState.Value = 0;
                return;
            }
            double countToExtract = _extractionCount * WorldTime.main.timewarpSpeed;
            bool thereAreMore = deposit.takeRsources(countToExtract);
            if (!thereAreMore)
            {
                MsgDrawer.main.Log("Resource Deposit Exhausted!");
            }

            this._material_container.AddResource(countToExtract);
            this.ExcavatorObject.eulerAngles = new Vector3(0f, 0f, this.ExcavatorObject.eulerAngles.z + 4);
            if (this._material_container.resourcePercent.Value == 1f)
            {
                MsgDrawer.main.Log("Container Full");
                this.TargetState.Value = 0;
            }
        }

        private void Toggle(UsePartData data)
        {
            //off
            if (this.State.Value == 0f)
            {
                this.FlowRate.Value = 0.1f;
                this.TargetState.Value = 1;
            }
            else
            {
                this.TargetState.Value = 0;
                this.FlowRate.Value = 0;
            }


            data.successfullyUsedPart = true;
        }

        public override void CheckOutOfFuel()
        {
            if (this.State.Value == 1 && !this.HasFuel(this.Logger))
            {
                this.TargetState.Value = 0;
                this.FlowRate.Value = 0;
            }
        }


    }
}
