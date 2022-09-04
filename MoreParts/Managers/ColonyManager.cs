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

namespace MorePartsMod.Managers
{
    class ColonyManager:MonoBehaviour
    {
        // public
        public static ColonyManager main;
        public Player_Local player;

        public List<ColonyBuildingData> Buildings { get => MorePartsMod.Main.Buildings; }
        public List<ColonyComponent> Colonies { get => this._colonies; }

        //private
        private SFS.UI.ModGUI.Button _createColonyButton;
        private ColonyData _newColony;
        private ColonyGUI _ui;
        private GameObject _holder;
        private List<ColonyComponent> _colonies;

        private void Awake()
        {
            main = this;
            this.player = PlayerController.main.player;
            this._colonies = new List<ColonyComponent>();
            this.InitGUI();
        }

        private void InitGUI()
        {
            GameObject mainUI = GameObject.Find("Main UI");
            this._createColonyButton = Builder.CreateButton(mainUI.transform, 212, 55, 0, 600, this.CreateColony, "Create Colony");
            this._createColonyButton.gameObject.SetActive(false);

            // colony menu GUI
            this._holder = new GameObject("Colony Menu");
            this._holder.transform.localScale = new Vector3(0.9f, 0.9f);
            Builder.AttachToCanvas(this._holder, Builder.SceneToAttach.CurrentScene);
            this._ui = this._holder.AddComponent<ColonyGUI>();
        }

        private void Start()
        {
            this.LoadColonies();
            KeySettings.AddOnKeyDown_World(KeySettings.Main.Open_Colony, this.OpenColony);
            this.player.OnChange += this.OnPlayerChange;
        }

        public void SaveColonies()
        {
            MorePartsMod.Main.SaveColonyInfo(this._colonies);
        }

        private void LoadColonies()
        {
            GameObject colonyPrefab = MorePartsMod.Main.Assets.LoadAsset<GameObject>("Colony");
            // setup buildings
            RefineryComponent.Setup(colonyPrefab);
            SolarPanelComponent.Setup(colonyPrefab);
            VABComponent.Setup(colonyPrefab);
            bool flag = false;
            foreach (ColonyData colony in MorePartsMod.Main.ColoniesInfo)
            {
                GameObject colonyGameObject = GameObject.Instantiate(colonyPrefab);
                WorldBuildings.addBuildingToWorld(colonyGameObject, "Holder", colony.getPlanet(), colony.position);
                GameObject holder = colonyGameObject.transform.FindChild("Holder").gameObject;
                holder.transform.eulerAngles = new Vector3(0, 0, colony.angle);
                ColonyComponent component = holder.AddComponent<ColonyComponent>();
                component.data = colony;
                
                // check if the building quantity is diferent.(there is a new building)
                if (component.data.buildings.Count < this.Buildings.Count)
                {
                    flag = true;
                    foreach (ColonyBuildingData newBuilding in this.Buildings)
                    {
                        // colony don't have this building
                        if (component.GetBuilding(newBuilding.name) == null)
                        {
                            component.data.buildings.Add(newBuilding);
                        }
                    }
                }
                component.RestoreBuildings();
                this._colonies.Add(component);
            }

            if (flag)
            {
                ColonyManager.main.SaveColonies();
            }
        }

        private void OnDisable()
        {
            this.SaveColonies();
        }

        private void OpenColony()
        {
            foreach (ColonyComponent colony in this.Colonies)
            {
                if (colony.data.andress != this.player.Value.location.planet.Value.codeName)
                {
                    continue;
                }

                float distance = Vector2.Distance(colony.data.position, this.player.Value.location.position.Value);
                if (distance > 100)
                {
                    continue;
                }
                this._ui.Colony = colony;
                ScreenManager.main.OpenScreen(() => this._ui);
                break;
            }           
        }

        private void OnPlayerChange()
        {
            this._createColonyButton.gameObject.SetActive(false);
            if(this.player.Value == null)
            {
                return;
            }
            ResourceModule[] resources = (this.player.Value as Rocket).partHolder.GetModules<ResourceModule>();
            bool flag = false, flag2 = false;
            foreach(ResourceModule resource in resources)
            {
                if (resource.resourceType.name == "Electronic_Component")
                {
                    flag = true;
                    continue;
                }

                if (resource.resourceType.name == "Construction_Material")
                {
                    flag2 = true;
                }
            }
            
            if(flag && flag2)
            {
                this.player.Value.location.planet.OnChange += OnPlanetChange;
            }
           
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
            if (this.player.Value.location.Value.TerrainHeight > 50)
            {
                this._createColonyButton.gameObject.SetActive(false);
                return;
            }

            if (this.player.Value.location.velocity.Value.magnitude > 0.5f)
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

            double angle = playerLocation.position.Value.AngleDegrees;

            Double2 colonyPosition = Double2.CosSin((double)(0.017453292f * angle)) * (playerLocation.planet.Value.Radius + planet.GetTerrainHeightAtAngle(angle * 0.017453292f) );

            this._newColony = new ColonyData((float)angle - 90,planet.codeName, colonyPosition);
            this.OnCreateColony();
        }

        private void OnCreateColony()
        {
            this._newColony.name = "Name";

            GameObject colony = GameObject.Instantiate(MorePartsMod.Main.Assets.LoadAsset<GameObject>("Colony"));

            WorldBuildings.addBuildingToWorld(colony, "Holder", this._newColony.getPlanet(), this._newColony.position);
            
            GameObject holder = colony.transform.FindChild("Holder").gameObject;
            holder.transform.eulerAngles = new Vector3(0, 0, this._newColony.angle);

            ColonyComponent component = holder.AddComponent<ColonyComponent>();
            this._newColony.buildings = new List<ColonyBuildingData>(this.Buildings);
            component.data = this._newColony;
            component.RestoreBuildings();
            this.Colonies.Add(component);
            MorePartsMod.Main.SaveColonyInfo(this._colonies);
        }

        private bool CheckColonyDistance(string address,Double2 colonyPosition)
        {
            foreach(ColonyComponent colony in this.Colonies)
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

    }
}