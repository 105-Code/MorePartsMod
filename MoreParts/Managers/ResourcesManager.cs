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

        public ReourceDeposit CurrentDeposit { private set; get; }


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

        private void OnDestroy()
        {
            this.SavePlanetResourcesInfo();
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
                    deposit.Discovered = true;
                    this.SavePlanetResourcesInfo();
                    return true;
                }

            }
            return false; 
        }

        public void Update()
        {
            if(GameManager.main == null || this.Player.Value == null)
            {
                return;
            }

            if (this.CurrentDeposit == null)
            {
                foreach (ReourceDeposit deposit in this.PlanetResourcesData[this.CurrentPlanet.codeName].ResourceDeposits)
                {
                    if(Vector2.Distance(deposit.Location, this.Player.Value.location.Value.position) > deposit.Size)
                    {
                        continue;
                    }

                    MsgDrawer.main.Log("Enter resource deposit");
                    this.CurrentDeposit = deposit;
                    break;
                }
                return;
            }

            if (Vector2.Distance(this.CurrentDeposit.Location, this.Player.Value.location.Value.position) > this.CurrentDeposit.Size)
            {
                MsgDrawer.main.Log("Exit resource deposit");
                this.CurrentDeposit = null;
            }


        }

        #region Listeners

        public void OnPlayerChange()
        {
            this.CurrentDeposit = null;
            if (this.Player.Value == null)
            {
                return;
            }

            this.Player.Value.location.planet.OnChange += this.OnPlanetChange;
           
        }

        public void OnPlanetChange()
        {
            this.CurrentDeposit = null;
            this.CurrentPlanet = this.Player.Value.location.planet.Value;
        }

        #endregion

        #region Map Drawers

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

        #endregion

        #region Save Resources info

        public void SavePlanetResourcesInfo()
        {
            MorePartsModMain.SaveWorldPersistent("PlanetResources.json", this.PlanetResourcesData);
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
            this.SavePlanetResourcesInfo();
        }

        #endregion
    }
}