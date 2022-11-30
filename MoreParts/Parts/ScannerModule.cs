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
            if(GameManager.main == null || !this._active.Value || this.Location == null || this.Location.planet == null)
            {
                return;
            }

            if (this.Location.planet.IsInsideAtmosphere(this.Location.position))
            {
                this.Toggle("You can't use in the atmosphere", false);
                return;
            }
            double max_altitud = this.Location.planet.data.basics.timewarpHeight + 50000;
            if (this.Location.Height > max_altitud)
            {
                this.Toggle("Max altitude " + max_altitud/1000+"km", false);
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
                this.Toggle("GeoEye turn OFF", false);
            }
            else
            {
                this.Toggle("GeoEye turn ON", true);
            }
            this.CheckOutOfFuel();
            data.successfullyUsedPart = true;
		}

    }
}
