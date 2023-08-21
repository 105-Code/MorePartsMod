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

        public Bool_Reference Active;
        public Float_Reference FlowRate;

        public Rocket Rocket { set; get; }

        public Location Location { set; get; }
        public Part Part;

        public void Awake()
        {

            this.Part.onPartUsed.AddListener(this.OnPartUsed);
        }

        private void Start()
        {
            this.FlowModule.onStateChange += this.CheckOutOfFuel;
        }

        private void Update()
        {
            if (GameManager.main == null || !this.Active.Value || this.Location == null || this.Location.planet == null || this.Rocket.location == null)
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
                this.Toggle("Max altitude " + max_altitud / 1000 + "km", false);
                return;
            }

            if (ResourcesManger.Main.AnalyzePlanet(this.Rocket.location))
            {
                MsgDrawer.main.Log("Found Resource Deposit");
            }

        }

        public override void CheckOutOfFuel()
        {
            if (this.Active.Value && !this.HasFuel(this.Logger))
            {
                this.Active.Value = false;
                this.FlowRate.Value = 0;
            }
        }

        public bool IsActive()
        {
            return this.Active.Value;
        }

        private void Toggle(string message, bool operation)
        {
            MsgDrawer.main.Log(message);
            this.Active.Value = operation;

            if (!operation)
            {
                this.FlowRate.Value = 0;
                return;
            }
            this.FlowRate.Value = 0.2f;
        }

        public void OnPartUsed(UsePartData data)
        {
            if (this.Active.Value)
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
