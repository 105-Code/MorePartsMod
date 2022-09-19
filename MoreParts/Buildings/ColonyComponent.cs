using MorePartsMod.ARPA;
using MorePartsMod.Managers;
using MorePartsMod.UI;
using SFS;
using SFS.Input;
using SFS.UI;
using SFS.UI.ModGUI;
using SFS.Variables;
using SFS.World;
using SFS.WorldBase;
using System.Collections.Generic;
using UnityEngine;

namespace MorePartsMod.Buildings
{
    public class ColonyComponent : MonoBehaviour
    {

        private GameObject holder;

        public ColonyData data;
        public bool dialogOpen;
        public bool playerNear;

        private Dictionary<string, object> _modules = new Dictionary<string, object>();
        private Bool_Local _playerInPlanet = new Bool_Local();
        private Bool_Local _playerNear = new Bool_Local();
        private bool _hasEnergy;
        private Node _node;

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
            this._node = AntennaComponent.main.AddNode(this.transform.parent.GetComponent<WorldLocation>(), true);
            ColonyManager.main.player.OnChange += this.OnChangePlayer;
        }

        private void OnChangePlayer()
        {
            if(ColonyManager.main.player.Value == null)
            {
                return;
            }
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
            if (GameManager.main == null || !this._playerInPlanet.Value || !ColonyManager.main.player.Value)
            {
                return;
            }

            if (Vector2.Distance(this.data.position, ColonyManager.main.player.Value.location.position.Value) > 100)
            {
                this._playerNear.Value = false;
                return;
            }
            this._playerNear.Value = true;

        }

        public bool Build(string buildingName)
        {
            ColonyBuildingData building = this.GetBuilding(buildingName);
            if(!ColonyManager.main.CheckAndReduceMaterials(building.cost.constructionCost, building.cost.electronicCost))
            {
                MsgDrawer.main.Log("Insufficient Materials");
                return false ;
            }
            building.state = true;
            this.checkSolarPanel(building);
            ColonyManager.main.SaveColonies();
            this.transform.FindChild(buildingName).gameObject.SetActive(true);
            this.InjectData();
            return true;
        }


        public ColonyBuildingData GetBuilding(string name)
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

                this.checkSolarPanel(building);
                buildingTransform.gameObject.SetActive(building.state);
            }
            this.InjectData();
        }

        private void checkSolarPanel(ColonyBuildingData building)
        {
            if (building.name == "Solar Panels")
            {
                this._hasEnergy = building.state;
            }
            this.InjectHasEnergy();
        }


        #region Injectables
        private void InjectData()
        {
            this.InjectColony();
            this.InjectRocket();
            this.InjectPlayerInPlanet();
            this.InjectPlayerNear();

            this._playerInPlanet.OnChange += this.InjectPlayerInPlanet;
            this._playerNear.OnChange += this.InjectPlayerNear;
            ColonyManager.main.player.OnChange += this.InjectRocket;
        }
        private void InjectHasEnergy()
        {
            INJ_HasEnergy[] list = this.CollectModules<INJ_HasEnergy>();
            for (int i = 0; i < list.Length; i++)
            {
                list[i].HasEnergy = this._hasEnergy;
            }
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

        public T[] CollectModules<T>()
        {
            string name = typeof(T).Name;
            if (!this._modules.ContainsKey(name))
            {
                this._modules.Add(name, base.GetComponentsInChildren<T>(true));
            }
            return (T[])this._modules[name];
        }
        #endregion

        public class ColonyData
        {
            public float angle;
            public Double2 position;
            public string andress;
            public string name;
            public double rocketParts;
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

            public Double2 getBuildingPosition(string buildingName, float height = 0)
            {
                foreach(ColonyBuildingData building in this.buildings)
                {
                    if(building.name != buildingName)
                    {
                        continue;
                    }
                    Double2 colonyPos =  Double2.CosSin((double)(0.017453292f * LandmarkAngle)) * (this.getPlanet().Radius + this.getPlanet().GetTerrainHeightAtAngle((double)(LandmarkAngle * 0.017453292f)) + height);
                    Vector2 buildingPos = Double2.CosSin((double)(0.017453292f * (this.angle))) * building.offset.x;
                    return colonyPos + buildingPos;
                }

                Debug.Log(buildingName+" Not Found");
                return Double2.CosSin((double)(0.017453292f * LandmarkAngle)) * (this.getPlanet().Radius + this.getPlanet().GetTerrainHeightAtAngle((double)(LandmarkAngle * 0.017453292f)) + height);
            }

            public bool isBuildingActive(string buildingName)
            {
                foreach (ColonyBuildingData building in this.buildings)
                {
                    if (building.name != buildingName)
                    {
                        continue;
                    }
                    return building.state;
                }
                return false;
            }

            public float LandmarkAngle { get => this.angle + 90; }
        }

        public class ColonyBuildingData
        {
            public bool state;
            public string name;
            public ColonyBuildingCost cost;
            public Double2 offset;

            public ColonyBuildingData() { }

            public ColonyBuildingData(bool state, string name, ColonyBuildingCost cost, Double2 pos)
            {
                this.name = name;
                this.state = state;
                this.cost = cost;
                this.offset = pos;
            }
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

        public interface INJ_HasEnergy
        {
            bool HasEnergy { set; }
        }

    }
}
