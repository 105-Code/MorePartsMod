using MorePartsMod.Managers;
using MorePartsMod.Parts.Types;
using SFS.Parts;
using SFS.UI;
using SFS.Variables;
using SFS.World;
using static SFS.World.Rocket;


namespace MorePartsMod.Parts
{
    class ScannerModule : ElectricalModule, INJ_Rocket, INJ_Location
    {

        private VariableList<bool>.Variable _active;
        private VariableList<double>.Variable _flowRate;

        public Rocket Rocket { set; get; }

        public Location Location { set; get; }

        public override void Awake()
        {
            base.Awake();
            this._active = this.getBoolVariable("active");
            this._flowRate = this.getDoubleVariable("flow_rate");
            this.Part.onPartUsed.AddListener(this.OnPartUsed);
        }

        private void Start()
        {
            this.FlowModule.onStateChange += this.CheckOutOfFuel;
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

        public override void CheckOutOfFuel()
        {
            if (this._active.Value && !this.HasFuel(this.Logger))
            {
                this._active.Value = false;
                this._flowRate.Value = 0;
            }
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
            this.CheckOutOfFuel();
            data.successfullyUsedPart = true;
		}

    }
}
