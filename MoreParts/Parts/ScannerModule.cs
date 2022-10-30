using MorePartsMod.ARPA;
using MorePartsMod.Buildings;
using MorePartsMod.Managers;
using SFS;
using SFS.Parts;
using SFS.Parts.Modules;
using SFS.UI;
using SFS.Variables;
using SFS.World;
using UnityEngine;
using static SFS.Parts.Modules.FlowModule;
using static SFS.World.Rocket;

namespace MorePartsMod.Parts
{
    class ScannerModule : MonoBehaviour, INJ_Rocket,INJ_Location
    {
        private VariableList<bool>.Variable _active;
        private VariableList<double>.Variable _flowRate;
        private FlowModule _source;
        public Rocket Rocket { set; get; }

        private I_MsgLogger Logger
        {
            get
            {
                if (!this.Rocket.isPlayer)
                {
                    return new MsgNone();
                }
                return MsgDrawer.main;
            }
        }

        public Location Location { set; get; }

        private void Awake()
        {
            Part part = this.GetComponent<Part>();
            this._source = this.GetComponent<FlowModule>();

            this._active = part.variablesModule.boolVariables.GetVariable("active");
            this._flowRate = part.variablesModule.doubleVariables.GetVariable("flow_rate");
            part.onPartUsed.AddListener(this.OnPartUsed);
        }

        private void Start()
        {
            this._source.onStateChange += this.CheckOutOfElectricity;
        }

        private void Update()
        {
            if(GameManager.main == null || !this._active.Value)
            {
                return;
            }

            if (this.Location.planet.IsInsideAtmosphere(this.Location.position))
            {
                this.Toggle("You can't use in the atmosphere", false);
                return;
            }

            if(this.Location.Height > 200000)
            {
                this.Toggle("Over max altitud 200km",false);
                return;
            }

            if (ResourcesManger.Main.AnalyzePlanet(this.Rocket.location))
            {
                MsgDrawer.main.Log("Found Resource Deposit");
            }

        }

        private void CheckOutOfElectricity()
        {
            if (this._active.Value && !this.HasElectricity(this.Logger))
            {
                this._active.Value = false;
                this._flowRate.Value = 0;
            }
        }

        private bool HasElectricity(I_MsgLogger logger)
        {
            return this._source.CanFlow(logger);
        }

        public bool IsActive()
        {
            return this._active.Value;
        }

        private void Toggle(string message,bool operation)
        {
            MsgDrawer.main.Log(message);
            this._active.Value = operation;

            if (!operation)
            {
                this._flowRate.Value = 0;
                return;
            }
            this._flowRate.Value = 0.2;
        }


		public void OnPartUsed(UsePartData data)
		{
            if (this._active.Value)
            {
                this.Toggle("Part turn OFF", false);
            }
            else
            {
                this.Toggle("Part turn ON", true);
            }
            this.CheckOutOfElectricity();
            data.successfullyUsedPart = true;
		}



	}
}
