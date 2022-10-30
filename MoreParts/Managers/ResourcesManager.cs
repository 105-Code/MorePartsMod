using System.Collections.Generic;
using MorePartsMod.Buildings;
using MorePartsMod.UI;
using SFS;
using SFS.Input;
using SFS.IO;
using SFS.Parsers.Json;
using SFS.Parts.Modules;
using SFS.UI;
using SFS.UI.ModGUI;
using UnityEngine;
using SFS.World;
using SFS.WorldBase;

using static MorePartsMod.Buildings.ColonyComponent;
using MorePartsMod.World;
using static MorePartsMod.World.PlanetResourceData;
using SFS.World.Maps;
using MorePartsMod.Parts;

namespace MorePartsMod.Managers
{
    class ResourcesManger : MonoBehaviour
    {
        public static ResourcesManger Main;
        public Dictionary<string, PlanetResourceData> PlanetResourcesData { private set; get; }

        public Player_Local Player { set; get; }

        public Planet CurrentPlanet { set; get; }



        private void Awake()
        {
            Main = this;
            this.PlanetResourcesData = new Dictionary<string, PlanetResourceData>();
            this.Player = PlayerController.main.player;
        }


        private void Start()
        {
            Debug.Log("Loading Planet Resources");
            this.LoadPlanetResourcesInfo();
            this.Player.OnChange += this.OnPlayerChange;
        }

        public bool AnalyzePlanet(WorldLocation location)
        {
            Double2 target = Double2.zero;
            Double2 origin = location.Value.position;

            PlanetResourceData planetData = this.PlanetResourcesData[this.CurrentPlanet.codeName];
            foreach (ReourceDeposit deposit in planetData.ResourceDeposits)
            {
                if (deposit.Active && deposit.Discovered)
                {
                    continue;
                }

                bool intersect = Utils.LineIntersectCircle(deposit.Size, deposit.Location, origin, target);

                if (intersect)
                {
                    Debug.Log("Rocket:" + origin.ToString() + "  Planet:" + target.ToString() + " deposit" + deposit.Location.ToString() + " Angle:" + deposit.AngleDegree);

                    deposit.Discovered = true;
                    return true;
                }

            }
            return false; 
        }

        public void OnPlayerChange()
        {
            this.Player.Value.location.planet.OnChange += this.OnPlanetChange;
           
        }

        public void OnPlanetChange()
        {
            this.CurrentPlanet = this.Player.Value.location.planet.Value;
        }

        public void DrawInMap()
        {
            if (this.CurrentPlanet == null || GameManager.main == null)
            {
                return;
            }

            PlanetResourceData planetData = this.PlanetResourcesData[this.CurrentPlanet.codeName];

            foreach (ReourceDeposit deposit in planetData.ResourceDeposits)
            {
                if (deposit.Active && deposit.Discovered)
                {
                    Utils.DrawLandmarkInPlanet(this.CurrentPlanet, deposit.AngleDegree, deposit.Location, "Resource Deposit", Color.red);
                }
            }

            Rocket rocket = (Rocket)this.Player.Value;
            ScannerModule[] modules = rocket.partHolder.GetModules<ScannerModule>();
            if (modules.Length <= 0)
            {
                return;
            }

            if (!modules[0].IsActive())
            {
                return;
            }

            Vector3 rocketPosition = rocket.location.Value.position/1000;
            Vector3[] points = new Vector3[] { Vector3.zero, rocketPosition };
            Map.solidLine.DrawLine(points, this.CurrentPlanet, Color.red, Color.red);

        }


        public void SavePlanetResourcesInfo(Dictionary<string, PlanetResourceData> deposits)
        {
            this.PlanetResourcesData = deposits;
            MorePartsModMain.SaveWorldPersistent("PlanetResources.json", deposits);
        }

        public void LoadPlanetResourcesInfo()
        {

            Dictionary<string, PlanetResourceData> data;
            MorePartsModMain.LoadWorldPersistent<Dictionary<string, PlanetResourceData>>("PlanetResources.json", out data);
            if (data == null)
            {
                this.GeneratePlanetResourcesInfo();
                return;
            }
            this.PlanetResourcesData = data;
        }

        private void GeneratePlanetResourcesInfo()
        {
            Dictionary<string, PlanetResourceData> data = new Dictionary<string, PlanetResourceData>();
            foreach (Planet planet in Base.planetLoader.planets.Values)
            {
                PlanetResourceData resources = new PlanetResourceData(planet);
                resources.Initialize();
                data.Add(planet.codeName, resources);
            }
            this.PlanetResourcesData = data;
            this.SavePlanetResourcesInfo(this.PlanetResourcesData);
        }
    }
}