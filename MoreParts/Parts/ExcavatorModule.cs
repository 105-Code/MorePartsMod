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

        public ResourceModule[] _material_containers;

        public override void Awake()
        {
            base.Awake();
            
        }

        private void Start()
        {
            this._material_containers = this.Rocket.partHolder.GetModules<ResourceModule>();
            this._target_state = this.getDoubleVariable("target_state");
            this._state = this.getDoubleVariable("state");
            this._flow_rate = this.getDoubleVariable("flow_rate");

            this.Part.onPartUsed.AddListener(this.Toggle);
            this.FlowModule.onStateChange += this.CheckOutOfFuel;
            this.CheckOutOfFuel();
        }

        private void Update()
        {
            if (GameManager.main == null || this._state.Value == 0 || this.Location == null)
            {
                return;
            }

            ReourceDeposit deposit = ResourcesManger.Main.CurrentDeposit;

            if(deposit == null || !deposit.Active)
            {
                MsgDrawer.main.Log("There are not Resource Deposit");
                this._target_state.Value = 0;
                return;
            }

            bool thereAreMore = deposit.takeRsources(0.1f);
            if (!thereAreMore)
            {
                MsgDrawer.main.Log("Resource Deposit Exhausted!");
            }
            foreach (ResourceModule resourceModule in this._material_containers)
            {
                if(resourceModule.resourceType.name == ResourcesTypes.MATERIAL)
                {
                    resourceModule.AddResource(resourceModule.ResourceSpace * 0.1f);
                }
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
