using System.Collections.Generic;
using SFS;
using SFS.UI;
using UnityEngine;
using SFS.World;
using SFS.WorldBase;

using MorePartsMod.World;
using static MorePartsMod.World.PlanetResourceData;
using SFS.World.Maps;
using MorePartsMod.Parts;
using MorePartsMod.Utils;

namespace MorePartsMod.Managers
{
    class ResourcesManager : MonoBehaviour
    {
        public static ResourcesManager Main;
        public Dictionary<string, PlanetResourceData> PlanetResourcesData { private set; get; }

        public Player_Local Player { set; get; }

        public Planet CurrentPlanet { set; get; }

        public ResourceDeposit CurrentDeposit { private set; get; }


        private void Awake()
        {
            Main = this;
            this.PlanetResourcesData = new Dictionary<string, PlanetResourceData>();
            this.Player = PlayerController.main.player;
        }


        private void Start()
        {
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
            foreach (ResourceDeposit deposit in planetData.ResourceDeposits)
            {
                if (deposit.Active && deposit.Discovered)
                {
                    continue;
                }

                int size = deposit.Size;
                if (Base.worldBase.settings.difficulty.difficulty == SFS.WorldBase.Difficulty.DifficultyType.Realistic)
                {
                    size = size * 5;
                }


                bool intersect = MathUtils.LineIntersectCircle(size, deposit.Location, origin, target);

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
            if (GameManager.main == null || this.Player.Value == null)
            {
                return;
            }

            if (this.CurrentDeposit == null)
            {
                foreach (ResourceDeposit deposit in this.PlanetResourcesData[this.CurrentPlanet.codeName].ResourceDeposits)
                {
                    if (Vector2.Distance(deposit.Location, this.Player.Value.location.Value.position) > deposit.Size)
                    {
                        continue;
                    }

                    MsgDrawer.main.Log("Entered resource deposit");
                    this.CurrentDeposit = deposit;
                    break;
                }
                return;
            }

            if (Vector2.Distance(this.CurrentDeposit.Location, this.Player.Value.location.Value.position) > this.CurrentDeposit.Size)
            {
                MsgDrawer.main.Log("Left resource deposit");
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
            if (this.Player.Value == null)
            {
                return;
            }
            this.CurrentPlanet = this.Player.Value.location.planet.Value;
        }

        #endregion

        #region Map Drawers

        public void DrawInMap()
        {
            if (this.CurrentPlanet == null || GameManager.main == null || PlanetResourcesData == null)
            {
                return;
            }

            PlanetResourceData planetData = this.PlanetResourcesData[this.CurrentPlanet.codeName];

            if (planetData == null)
            {
                return;
            }

            foreach (ResourceDeposit deposit in planetData.ResourceDeposits)
            {
                if (deposit.Active && deposit.Discovered)
                {
                    MapUtils.DrawLandmarkInPlanet(this.CurrentPlanet, deposit.AngleDegree, deposit.Location,  deposit.GetDepositSizeName(), Color.red);
                }
            }

            Rocket rocket = (Rocket)this.Player.Value;
            if (rocket == null)
            {
                return;
            }
            ScannerModule[] modules = rocket.partHolder.GetModules<ScannerModule>();
            if (modules.Length <= 0)
            {
                return;
            }

            if (!modules[0].IsActive())
            {
                return;
            }

            Vector3 rocketPosition = rocket.location.Value.position / 1000;
            Vector3[] points = new Vector3[] { Vector3.zero, rocketPosition };
            Map.solidLine.DrawLine(points, this.CurrentPlanet, Color.red, Color.red);

        }

        #endregion

        #region Save Resources info

        public void SavePlanetResourcesInfo()
        {
            FileUtils.SaveWorldPersistent("PlanetResources.json", this.PlanetResourcesData);
        }

        public void LoadPlanetResourcesInfo()
        {

            Dictionary<string, PlanetResourceData> data;
            FileUtils.LoadWorldPersistent<Dictionary<string, PlanetResourceData>>("PlanetResources.json", out data);
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