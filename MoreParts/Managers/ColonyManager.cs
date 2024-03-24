using System.Collections.Generic;
using MorePartsMod.Buildings;
using MorePartsMod.UI;
using SFS.Input;
using SFS.Parts.Modules;
using SFS.UI;
using SFS.UI.ModGUI;
using UnityEngine;
using SFS.World;
using SFS.WorldBase;

using static MorePartsMod.Buildings.ColonyComponent;
using MorePartsMod.Utils;
using System;

namespace MorePartsMod.Managers
{
    class ColonyManager : MonoBehaviour
    {
        // public
        public static ColonyManager Main;
        public Player_Local Player;

        public List<ColonyComponent> Colonies { get; private set; }

        //private
        private SFS.UI.ModGUI.Button _createColonyButton;
        private ColonyGUI _ui;
        private bool _extractFlow;

        void OnDestroy()
        {
            this.SaveColonies();
        }

        void Awake()
        {
            Main = this;
            Player = PlayerController.main.player;
            Colonies = new List<ColonyComponent>();
            InitGUI();
        }

        void Start()
        {
            this.LoadColonies();
            KeySettings.AddOnKeyDown_World(KeySettings.Main.Open_Colony, this.OpenColony);
            KeySettings.AddOnKeyDown_World(KeySettings.Main.Toggle_Colony_Flow, this.ToggleColonyFlow);
            Player.OnChange += this.OnPlayerChange;
        }

        public bool DeleteColony(ColonyComponent colony)
        {
            bool result = this.Colonies.Remove(colony);
            if (result)
            {
                GameObject.Destroy(colony.gameObject);
                this.SaveColonies();
            }
            return result;
        }

        public void SaveColonies()
        {
            List<ColonyData> data = new List<ColonyData>();
            foreach (ColonyComponent colony in this.Colonies)
            {
                data.Add(colony.Data);
            }
            MorePartsPack.Main.ColoniesInfo = data;
            FileUtils.SaveWorldPersistent("Colonies.json", data);
        }

        public bool CheckAndReduceMaterials(float constructionRequired, float electronicRequired)
        {
            Rocket rocket = Player.Value as Rocket;
            ResourceModule electronic = null;
            ResourceModule construction = null;
            foreach (ResourceModule resourceGroup in rocket.resources.globalGroups)
            {
                if (resourceGroup.resourceType.name == MorePartsTypes.ELECTRONIC_COMPONENT)
                {
                    if (resourceGroup.ResourceAmount < electronicRequired)
                    {
                        MsgDrawer.main.Log("Insufficient Electronic Components");
                        return false;
                    }
                    electronic = resourceGroup;
                }

                if (resourceGroup.resourceType.name == MorePartsTypes.CONSTRUCTION_MATERIAL)
                {
                    if (resourceGroup.ResourceAmount < constructionRequired)
                    {
                        MsgDrawer.main.Log("Insufficient Construction MAterial");
                        return false;
                    }
                    construction = resourceGroup;
                }

            }

            if (electronic == null || construction == null)
            {
                return false;
            }

            electronic.TakeResource(electronicRequired);
            construction.TakeResource(constructionRequired);
            return true;
        }

        public void DrawInMap()
        {
            foreach (ColonyComponent colony in Colonies)
            {
                MapUtils.DrawLandmarkInPlanet(colony.Data.GetPlanet(), colony.Data.LandmarkAngle, colony.Data.position, colony.Data.name, Color.white);
            }
        }

        public ColonyComponent SpawnColony(ColonyData colonyData)
        {
            GameObject minBuildingOnWorld = GameObject.Instantiate(MorePartsPack.Main.ColonyBuildingFactory.ColonyBuilding);
            ColonyComponent component = minBuildingOnWorld.GetComponent<ColonyComponent>();

            BuildingUtils.AddBuildingToWorld(minBuildingOnWorld, colonyData.GetPlanet(), colonyData.position, colonyData.angle);
            component.Data = colonyData;


            component.RestoreBuildings();
            return component;
        }

        private ColonyComponent GetNearestColony()
        {
            foreach (ColonyComponent colony in this.Colonies)
            {
                if (colony.Data.address != Player.Value.location.planet.Value.codeName)
                {
                    continue;
                }

                float distance = Vector2.Distance(colony.Data.position, Player.Value.location.position.Value);
                if (distance > ColonyData.SIZE)
                {
                    continue;
                }

                return colony;
            }
            return null;
        }

        private void ToggleColonyFlow()
        {
            MsgDrawer.main.Log(this._extractFlow ? "Refilling rocket resources" : "Extracting rocket resources");

            ColonyComponent colony = GetNearestColony();

            if (colony == null)
            {
                return;
            }

            Rocket rocket = (Rocket)Player;
            string typeName;
            foreach (ResourceModule resource in rocket.resources.globalGroups)
            {
                typeName = resource.resourceType.name;
                if (this._extractFlow)
                {
                    double addToRocket = colony.Data.TakeResource(typeName, resource.TotalResourceCapacity);
                    resource.AddResource(addToRocket);
                    continue;
                }
                if (colony.Data.AddResource(typeName, resource.ResourceAmount))
                {
                    resource.TakeResource(resource.ResourceAmount);
                }
            }
            this.SaveColonies();

            this._extractFlow = !this._extractFlow;
        }

        private void LoadColonies()
        {
            foreach (ColonyData colony in MorePartsPack.Main.ColoniesInfo)
            {
                ColonyComponent component = SpawnColony(colony);
                Colonies.Add(component);
            }
        }

        private void OpenColony()
        {
            ColonyComponent colony = GetNearestColony();

            if (colony == null)
            {
                return;
            }

            this._ui.Colony = colony;
            ScreenManager.main.OpenScreen(() => this._ui);
        }

        private void OnPlayerChange()
        {
            try
            {
                this._createColonyButton.gameObject.SetActive(false);
                if (Player.Value == null)
                {
                    return;
                }
                ResourceModule[] resources = (Player.Value as Rocket).partHolder.GetModules<ResourceModule>();
                bool flag = false, flag2 = false;
                foreach (ResourceModule resource in resources)
                {
                    if (resource.resourceType.name == MorePartsTypes.ELECTRONIC_COMPONENT)
                    {
                        flag = true;
                        continue;
                    }

                    if (resource.resourceType.name == MorePartsTypes.CONSTRUCTION_MATERIAL)
                    {
                        flag2 = true;
                    }
                }

                if (flag && flag2)
                {
                    if (Player.Value.location != null && Player.Value.location.planet.Value != null)
                    {
                        Player.Value.location.planet.OnChange += OnPlanetChange;
                    }

                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void OnPlanetChange()
        {
            if (Player.Value == null)
            {
                return;
            }

            if (Player.Value.location.planet.Value == null)
            {
                return;
            }

            if (Player.Value.location.planet.Value.codeName == "Earth")
            {
                return;
            }
            Player.Value.location.velocity.OnChange += this.CheckPlayerVelocity;
        }

        private void CheckPlayerVelocity()
        {
            if (Player.Value == null || Player.Value.location.Value == null)
            {
                return;
            }

            if (Player.Value.location.Value.TerrainHeight > 50)
            {
                this._createColonyButton.gameObject.SetActive(false);
                return;
            }

            if (Player.Value.location.velocity.Value.magnitude > 0.5f)
            {
                this._createColonyButton.gameObject.SetActive(false);
                return;
            }


            this._createColonyButton.gameObject.SetActive(true);
        }

        private void CreateColony()
        {
            WorldLocation playerLocation = Player.Value.location;
            Planet planet = Player.Value.location.planet.Value;

            if (!this.CheckColonyDistance(planet.codeName, playerLocation.position.Value))
            {
                MsgDrawer.main.Log("Too close to another colony");
                return;
            }

            if (!CheckAndReduceMaterials(7, 7))
            {
                return;
            }

            float angle = (float)playerLocation.position.Value.AngleRadians;
            Double2 colonyPosition = BuildingUtils.GetPositionOnPlanetSurface(angle, planet);
            ColonyData colonyData = new ColonyData(angle, planet.codeName, colonyPosition);
            colonyData.name = "Colony";
            ColonyComponent component = SpawnColony(colonyData);
            Colonies.Add(component);
            this.SaveColonies();
        }

        private bool CheckColonyDistance(string address, Double2 colonyPosition)
        {
            foreach (ColonyComponent colony in this.Colonies)
            {
                if (colony.Data.address != address)
                {
                    continue;
                }

                float distance = Vector2.Distance(colony.Data.position, colonyPosition);
                if (distance < 10000)// 10km
                {
                    return false;
                }
            }
            return true;
        }

        private void InitGUI()
        {
            GameObject mainUI = GameObject.Find("Main UI");
            this._createColonyButton = Builder.CreateButton(mainUI.transform, 212, 55, 0, 600, this.CreateColony, "Create Colony");
            this._createColonyButton.gameObject.SetActive(false);
            // colony menu GUI
            this._ui = ColonyGUI.Init();
        }
    }
}