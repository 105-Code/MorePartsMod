using MorePartsMod.Managers;
using SFS;
using SFS.UI;
using SFS.UI.ModGUI;
using SFS.Variables;
using SFS.World;
using SFS.WorldBase;
using System.Collections.Generic;
using UnityEngine;
using static SFS.World.WorldSave;

namespace MorePartsMod.Buildings
{
    class ColonyComponent : MonoBehaviour
    {

        private GameObject holder;
        public Window windows;

        public ColonyData data;
        public bool dialogOpen;
        public bool playerNear;

        private Dictionary<string, object> _modules = new Dictionary<string, object>();
        private Bool_Local _playerInPlanet = new Bool_Local();
        private Bool_Local _playerNear = new Bool_Local();

        private void Awake()
        {
            this.holder = GameObject.Find("Main UI");
        }

        private void Start()
        {
            if(GameManager.main == null)
            {
                return;
            }
            this.dialogOpen = false;
            ColonyManager.main.player.OnChange += this.OnChangePlayer;
            KeySettings.AddOnKeyDown_World(KeySettings.Main.Open_Colony, this.OpenColony);
        }

        private void OnChangePlayer()
        {
            ColonyManager.main.player.Value.location.planet.OnChange += this.OnChangePlanet;
        }

        private void OnChangePlanet()
        {
            if (ColonyManager.main.player.Value.location.planet.Value.codeName != this.data.andress)
            {
                this._playerInPlanet.Value = false;
            }
            this._playerInPlanet.Value = true;
        }

        private void FixedUpdate()
        {
            if (GameManager.main == null || !this._playerInPlanet.Value)
            {
                return;
            }

            if (Vector2.Distance(this.data.position, ColonyManager.main.player.Value.location.position.Value) > 50)
            {
                this._playerNear.Value = false;
                return;
            }
            this._playerNear.Value = true;

        }
        
        private void InjectData()
        {
            this._playerInPlanet.OnChange += this.InjectPlayerInPlanet;
            this._playerNear.OnChange += this.InjectPlayerNear;
            ColonyManager.main.player.OnChange += this.InjectRocket;

            this.InjectPlayerInPlanet();
            this.InjectColony();
            this.InjectPlayerNear();
            this.InjectRocket();
        }


        private void InjectPlayerNear()
        {
            INJ_PlayerNear[] list = this.CollectModules<INJ_PlayerNear>();
            for (int i = 0; i < list.Length; i++)
            {
                list[i].PlayerNear = this._playerNear;
            }
        }
        
        private void InjectPlayerInPlanet()
        {

            INJ_PlayerInPlanet[] list = this.CollectModules<INJ_PlayerInPlanet>();
            for (int i = 0; i < list.Length; i++)
            {
                list[i].PlayerInPlanet = this._playerInPlanet;
            }
        }

        private void InjectColony()
        {
            INJ_Colony[] list = this.CollectModules<INJ_Colony>();
            for (int i = 0; i < list.Length; i++)
            {
                list[i].Colony = this;
            }
        }

        private void InjectRocket()
        {
            INJ_Rocket[] list = this.CollectModules<INJ_Rocket>();
            for (int i = 0; i < list.Length; i++)
            {
                list[i].Rocket = ColonyManager.main.player.Value as Rocket;
            }
        }

        private void OpenColony()
        {
            if(ColonyManager.main.player.Value.location.Value.planet.codeName != this.data.andress)
            {
                return;
            }
            
            if (Vector2.Distance(ColonyManager.main.player.Value.location.position.Value,this.data.position) > 100)
            {
                return;
            }

            this.ShowWindows();
        }

        private void ShowWindows()
        {
            if (this.dialogOpen)
            {
                this.CloseWindow();
                return;
            }

            this.dialogOpen = true;

            if (this.windows != null)
            {
                this.windows.gameObject.SetActive(true);
                return;
            }

            int height = this.data.buildings.Count * 60 + 60;
            Window window = Builder.CreateWindow(holder, 350, height, 300, 300, false, 0.95f, this.data.name);
            this.windows = window;

            int start = height - 135;

            foreach (ColonyBuildingData building in this.data.buildings)
            {
                if (building.state)
                {
                    continue;
                }
              
                Builder.CreateButton(this.windows.gameObject, 340, 60, 0, start, () => this.Build(building.name), building.name);
                start -= 60;
            }

        }

        private void Build(string buildingName)
        {
            ColonyBuildingData building = this.GetBuilding(buildingName);
            if(!ColonyManager.main.CheckAndReduceMaterials(building.cost.constructionCost, building.cost.electronicCost))
            {
                MsgDrawer.main.Log("Insuficient Materials");
                return;
            }
            building.state = true;
            ColonyManager.main.SaveWoldInfo();
            this.transform.FindChild(buildingName).gameObject.SetActive(true);
            this.CloseWindow();
        }

        private void CloseWindow()
        {
            this.windows.gameObject.SetActive(false);
            this.dialogOpen = false; 
        }

        private ColonyBuildingData GetBuilding(string name)
        {
            foreach(ColonyBuildingData building in this.data.buildings)
            {
                if(building.name == name)
                {
                    return building;
                }
            }
            return null;
        }

        public T[] CollectModules<T>()
        {
            string name = typeof(T).Name;
            if (!this._modules.ContainsKey(name))
            {
                this._modules.Add(name, base.GetComponentsInChildren<T>(true));
            }
            return (T[])this._modules[name];
        }

        public void RestoreBuildings()
        {
            if(this.data == null)
            {
                return;
            }

            foreach(ColonyBuildingData building in this.data.buildings)
            {
                Transform buildingTransform = this.transform.FindChild(building.name);
                if (buildingTransform == null)
                {
                    continue;
                }
                buildingTransform.gameObject.SetActive(building.state);
            }
            this.InjectData();
        }

        public class ColonyData
        {
            public float angle;
            public Double2 position;
            public string andress;
            public string name;
            public List<ColonyBuildingData> buildings;

            public ColonyData() {
                this.buildings = new List<ColonyBuildingData>();
            }

            public ColonyData(string name,float angle, WorldLocation worldLocation)
            {
                this.angle = angle;
                this.name = name;
                this.position = worldLocation.position.Value;
                this.andress = worldLocation.planet.Value.codeName;
                this.buildings = new List<ColonyBuildingData>();
            }

            public ColonyData(float angle,string planetName, Double2 position)
            {
                this.angle = angle;
                this.position = position;
                this.andress = planetName;
                this.buildings = new List<ColonyBuildingData>();
            }

            public Planet getPlanet()
            {
                Planet planet;
                Base.planetLoader.planets.TryGetValue(this.andress, out planet);
                return planet;
            }

            public void setWorldLocation (WorldLocation location)
            {
                this.position = location.position.Value;
                this.andress = location.planet.Value.codeName;
            }

        }

        public class ColonyBuildingData
        {
            public bool state;
            public string name;
            public ColonyBuildingCost cost;

            public ColonyBuildingData() { }

            public ColonyBuildingData(bool state, string name, ColonyBuildingCost cost)
            {
                this.name = name;
                this.state = state;
                this.cost = cost;
            }

        }

        public class ColonyBuildingCost
        {
            public float constructionCost;
            public float electronicCost;
            public ColonyBuildingCost(float electronic, float construction)
            {
                this.constructionCost = construction;
                this.electronicCost = electronic;
            }
        }
        
        public interface INJ_PlayerNear
        {
            bool PlayerNear { set; }
        }

        public interface INJ_PlayerInPlanet
        {
            bool PlayerInPlanet { set; }
        }

        public interface INJ_Colony
        {
            ColonyComponent Colony { set; }
        }

        public interface INJ_Rocket
        {
            Rocket Rocket { set; }
        }

    }
}
