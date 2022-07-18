using System.Collections.Generic;
using MorePartsMod.Buildings;
using SFS;
using SFS.IO;
using SFS.Parsers.Json;
using SFS.Parts.Modules;
using SFS.UI;
using SFS.UI.ModGUI;
using SFS.World;
using SFS.WorldBase;
using UnityEngine;
using static MorePartsMod.Buildings.ColonyComponent;

namespace MorePartsMod.Managers
{
    class ColonyManager:MonoBehaviour
    {
        // public
        public static ColonyManager main;
        public Player_Local player;

        //private
        private SFS.UI.ModGUI.Button _createColonyButton;
        private List<ColonyComponent> _colonies;
        private GameObject _holder;
        private Window _windows;
        private string _colonyName;
        private ColonyData _newColony;
        private List<ColonyBuildingData> _buldingsList;

        private void Awake()
        {
            main = this;
            this._holder = GameObject.Find("Main UI");
            this._createColonyButton = Builder.CreateButton(this._holder, 212, 55, 0, 600, this.CreateColony, "Create Colony", Builder.Style.Blue);
            this._createColonyButton.gameObject.SetActive(false);
            this.player = PlayerController.main.player;
            this.player.OnChange += this.OnPlayerChange;
            this._colonies = new List<ColonyComponent>();
            this._colonyName = "Name";
        }

        private void Start()
        {
            this.LoadWorldInfo();
            this._buldingsList = new List<ColonyBuildingData>();
            this._buldingsList.Add(new ColonyBuildingData(false,"Refinery", new ColonyBuildingCost(10,12)));
        }

        private void OnPlayerChange()
        {
            this._createColonyButton.gameObject.SetActive(false);
            if(this.player.Value == null)
            {
                return;
            }
            this.player.Value.location.planet.OnChange += OnPlanetChange;
        }

        private void OnPlanetChange()
        {
            if(this.player.Value.location.planet.Value.codeName == "Earth")
            {
                return;
            }
            this.player.Value.location.velocity.OnChange += this.CheckPlayerVelocity;

        }

        private void CheckPlayerVelocity()
        {

            if(this.player.Value.location.velocity.Value.magnitude > 0.5f)
            {
                this._createColonyButton.gameObject.SetActive(false);
                return;
            }

            this._createColonyButton.gameObject.SetActive(true);
        }

        public bool CheckAndReduceMaterials(float constructionRequired, float electronicRequired)
        {
            Rocket rocket = this.player.Value as Rocket;
            ResourceModule electronic = null;
            ResourceModule construction = null;
            foreach (ResourceModule resourceGroup in rocket.resources.globalGroups)
            {
                if(resourceGroup.resourceType.name == "Electronic_Component")
                {
                    if(resourceGroup.ResourceAmount < electronicRequired)
                    {
                        return false;
                    }
                    electronic = resourceGroup;
                }

                if (resourceGroup.resourceType.name == "Construction_Material")
                {
                    if (resourceGroup.ResourceAmount < constructionRequired)
                    {
                        return false;
                    }
                    construction = resourceGroup;
                }

            }

            if(electronic == null || construction == null)
            {
                return false;
            }

            electronic.TakeResource(electronicRequired);
            construction.TakeResource(constructionRequired);
            return true;
        }

        private void CreateColony()
        {
            WorldLocation playerLocation = this.player.Value.location;
            Planet planet = this.player.Value.location.planet.Value;

            if (!this.CheckColonyDistance(planet.codeName, playerLocation.position.Value))
            {
                MsgDrawer.main.Log("Too close to another colony");
                return;
            }

            if (!CheckAndReduceMaterials(7, 7))
            {
                MsgDrawer.main.Log("Insufficient Materials");
                return;
            }

            Double2 planetCenter = new Double2(0, 0);
            Vector3 direction = (playerLocation.position.Value - planetCenter).normalized;
            double height = playerLocation.planet.Value.Radius + playerLocation.planet.Value.GetTerrainHeightAtAngle(playerLocation.position.Value.AngleRadians);
            Double2 colonyPosition = planetCenter + direction * (float) height;

            this._newColony = new ColonyData((float)playerLocation.position.Value.AngleDegrees - 90,planet.codeName, colonyPosition);
            this.OnCreateColony();
            //this.ShowWindows(); // disable for the moment
        }

        private void ShowWindows()
        {
            if (this._windows != null)
            {
                this._windows.gameObject.SetActive(true);
                return;
            }

            Window window = Builder.CreateWindow(this._holder, 350, 190, 0, 0, false, 0.95f, "New Colony");
            this._windows = window;

            Builder.CreateTextInput(this._windows.gameObject, 340, 60, 0, 20, this._colonyName, (string value) => this._colonyName = value);
            Builder.CreateButton(this._windows.gameObject, 340, 60, 0, -45, this.OnCreateColony, "Create");
        }

        private void OnCreateColony()
        {
            //this._windows.gameObject.SetActive(false); // disable for the moment
            this._newColony.name = this._colonyName;

            GameObject colony = GameObject.Instantiate(MorePartsMod.Main.Assets.LoadAsset<GameObject>("Colony"));

            WorldBuildings.addBuildingToWorld(colony, "Holder", this._newColony.getPlanet(), this._newColony.position);
            
            GameObject holder = colony.transform.FindChild("Holder").gameObject;
            holder.transform.eulerAngles = new Vector3(0, 0, this._newColony.angle);

            ColonyComponent component = holder.AddComponent<ColonyComponent>();
            this._newColony.buildings = new List<ColonyBuildingData>(this._buldingsList);
            component.data = this._newColony;
            this._colonyName = "Name";
            component.RestoreBuildings();
            this._colonies.Add(component);
            this.SaveWoldInfo();
        }

        private bool CheckColonyDistance(string address,Double2 colonyPosition)
        {
            foreach(ColonyComponent colony in this._colonies)
            {
                if(colony.data.andress != address)
                {
                    continue;
                }

                float distance = Vector2.Distance(colony.data.position, colonyPosition);
                if(distance < 10000)// 10km
                {
                    return false;
                }
            }
            return true;
        }

        public void SaveWoldInfo()
        {

            FilePath file =  Base.worldBase.paths.worldPersistentPath.ExtendToFile("Colonies.json");
            List<ColonyData> data = new List<ColonyData>();
            foreach(ColonyComponent colony in this._colonies)
            {
                data.Add(colony.data);
            }
            JsonWrapper.SaveAsJson(file, data, true);
        }

        private void LoadWorldInfo()
        {
            FilePath file = Base.worldBase.paths.worldPersistentPath.ExtendToFile("Colonies.json");
            if (!file.FileExists())
            {
                return;
            }
            List<ColonyData> data;
            JsonWrapper.TryLoadJson(file, out data);

            if (data == null)
            {
                return;
            }

            GameObject colonyPrefab = MorePartsMod.Main.Assets.LoadAsset<GameObject>("Colony");
            // setup buildings
            RefineryComponent.Setup(colonyPrefab);

            foreach (ColonyData colony in data)
            {
                GameObject colonyGameObject = GameObject.Instantiate(colonyPrefab);
                WorldBuildings.addBuildingToWorld(colonyGameObject, "Holder", colony.getPlanet(), colony.position);
                GameObject holder = colonyGameObject.transform.FindChild("Holder").gameObject;
                holder.transform.eulerAngles = new Vector3(0, 0, colony.angle);
                ColonyComponent component = holder.AddComponent<ColonyComponent>();
                component.data = colony;
                component.RestoreBuildings();
                this._colonies.Add(component);
            }
        }

    }
}