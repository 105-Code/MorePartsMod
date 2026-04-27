using MorePartsMod.Managers;
using SFS;
using SFS.Parts;
using SFS.UI;
using SFS.Variables;
using SFS.World;
using UnityEngine;
using static SFS.World.Rocket;


namespace MorePartsMod.Parts
{
    class ScannerModule : MonoBehaviour, INJ_IsPlayer, INJ_Rocket, INJ_Location
    {

        public Bool_Reference Active;

        public Rocket Rocket { set; get; }

        public Location Location { set; get; }
        public bool IsPlayer { set; get; }

        public Part Part;

        public void Awake()
        {
            this.Part.onPartUsed.AddListener(this.OnPartUsed);
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
            if (Base.worldBase.settings.difficulty.difficulty == SFS.WorldBase.Difficulty.DifficultyType.Realistic)
            {
                max_altitud += 100000;
            }

            if (this.Location.GetTerrainHeight(false) > max_altitud)
            {
                this.Toggle("Max use altitude " + max_altitud / 1000 + "km", false);
                return;
            }

            if (ResourcesManger.Main.AnalyzePlanet(this.Rocket.location))
            {
                MsgDrawer.main.Log("Found Resource Deposit");
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
                return;
            }
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
            data.successfullyUsedPart = true;
        }

    }
}
