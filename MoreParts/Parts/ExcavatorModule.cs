using MorePartsMod.Managers;
using MorePartsMod.Parts.Types;
using SFS.Parts;
using SFS.Parts.Modules;
using SFS.UI;
using SFS.Variables;
using SFS.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MorePartsMod.World.PlanetResourceData;
using static SFS.World.Rocket;

namespace MorePartsMod.Parts
{
    public class ExcavatorModule : ElectricalModule, INJ_Location, INJ_Rocket
    {
        
        private VariableList<double>.Variable _target_state;
        private VariableList<double>.Variable _state;
        private VariableList<double>.Variable _flow_rate;

        public Location Location { set; get; }
        public Rocket Rocket { set; get; }

        private const double _extractionCount = 0.009f;

        public ResourceModule _material_container;

        private Transform _excavator_object;

        public override void Awake()
        {
            base.Awake();
            
        }

        private void Start()
        {
            if (GameManager.main == null)
            {
                base.enabled = false;
                return;
            }
            this._material_container = this.GetMaterialContainer();
            this._target_state = this.getDoubleVariable("target_state");
            this._state = this.getDoubleVariable("state");
            this._flow_rate = this.getDoubleVariable("flow_rate");

            this.Part.onPartUsed.AddListener(this.Toggle);
            this.FlowModule.onStateChange += this.CheckOutOfFuel;
            this.CheckOutOfFuel();

            this._excavator_object= this.transform.Find("Stick").Find("Excavator");
        }

        private ResourceModule GetMaterialContainer() {
            if(this.Rocket == null || this.Rocket.resources == null)
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
            if (GameManager.main == null || this._state.Value == 0 || this.Location == null)
            {
                return;
            }

            if(this._material_container == null)
            {
                this._material_container = this.GetMaterialContainer();
                if(this._material_container == null)
                {
                    MsgDrawer.main.Log("You need a Material Container");
                    this._target_state.Value = 0;
                    return;
                }
            }


            ReourceDeposit deposit = ResourcesManger.Main.CurrentDeposit;

            if(deposit == null || !deposit.Active)
            {
                MsgDrawer.main.Log("There is not Resource Deposit");
                this._target_state.Value = 0;
                return;
            }

            bool thereAreMore = deposit.takeRsources(_extractionCount);
            if (!thereAreMore)
            {
                MsgDrawer.main.Log("Resource Deposit Exhausted!");
            }

            this._material_container.AddResource(_extractionCount);
            this._excavator_object.eulerAngles = new Vector3(0f, 0f, this._excavator_object.eulerAngles.z + 4);
            if (this._material_container.resourcePercent.Value == 1f)
            {
                MsgDrawer.main.Log("Container Full");
                this._target_state.Value = 0;
            }
        }

        private void Toggle(UsePartData data)
        {
            //off
            if(this._state.Value == 0f)
            {
                this._flow_rate.Value = 0.1;
                this._target_state.Value = 1;
            }
            else
            {
                this._target_state.Value = 0;
                this._flow_rate.Value = 0;
            }


            data.successfullyUsedPart = true;
        }

        public override void CheckOutOfFuel()
        {
            if (this._state.Value == 1 && !this.HasFuel(this.Logger))
            {
                this._target_state.Value = 0;
                this._flow_rate.Value = 0;
            }
        }


    }
}
