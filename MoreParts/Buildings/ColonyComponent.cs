using MorePartsMod.ARPA;
using MorePartsMod.Managers;
using SFS;
using SFS.UI;
using SFS.Variables;
using SFS.World;
using SFS.WorldBase;
using System.Collections.Generic;
using UnityEngine;
using static MorePartsMod.ColonyBuildingFactory;

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
        public Node Node { private set; get; }

        private void Awake()
        {
            this.holder = GameObject.Find("Main UI");
        }

        private void Start()
        {
            if (GameManager.main == null)
            {
                return;
            }
            this.dialogOpen = false;
            this.Node = AntennaComponent.main.AddNode(this.transform.parent.GetComponent<WorldLocation>(), true);
            ColonyManager.main.player.OnChange += this.OnChangePlayer;
        }

        private void OnChangePlayer()
        {
            if (ColonyManager.main.player.Value == null)
            {
                return;
            }
            ColonyManager.main.player.Value.location.planet.OnChange += this.OnChangePlanet;
        }

        private void OnChangePlanet()
        {
            if (ColonyManager.main.player.Value.location.planet.Value.codeName != this.data.address)
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

        private bool CheckAndReduceMaterials(float constructionMaterial, float electronicMaterial)
        {
            double constructionQuantity = this.data.getResource(MorePartsTypes.CONSTRUCTION_MATERIAL);
            if (constructionQuantity - constructionMaterial < 0)
            {
                return false;
            }
            double electronicQuantity = this.data.getResource(MorePartsTypes.ELECTRONIC_COMPONENT);
            if (electronicQuantity - electronicMaterial < 0)
            {
                return false;
            }
            this.data.takeResource(MorePartsTypes.CONSTRUCTION_MATERIAL, constructionMaterial);
            this.data.takeResource(MorePartsTypes.ELECTRONIC_COMPONENT, electronicMaterial);
            return true;
        }

        public bool Build(string buildingName)
        {
            BuildingData data = MorePartsPack.Main.ColonyBuildingFactory.getColonyBuilding(buildingName);

            if (!this.CheckAndReduceMaterials(data.constructionCost, data.electronicCost))
            {
                MsgDrawer.main.Log("Insufficient Materials");
                return false;
            }

            if (buildingName == MorePartsTypes.SOLAR_PANELS_BUILDING)
            {
                this.checkSolarPanel(true);
            }

            this.data.structures.Add(buildingName, new Building(data.offset));
            ColonyManager.main.SaveColonies();
            this.transform.Find(buildingName).gameObject.SetActive(true);
            this.InjectData();
            return true;
        }

        public void RestoreBuildings()
        {
            if (this.data == null)
            {
                return;
            }

            foreach (string buildingName in MorePartsPack.Main.ColonyBuildingFactory.GetBuildingsName())
            {
                Transform buildingTransform = this.transform.Find(buildingName);
                if (buildingTransform == null)
                {
                    continue;
                }

                if (!this.data.isBuildingActive(buildingName))
                {
                    if (buildingName == "Solar Panels")
                    {
                        this.checkSolarPanel(false);
                    }
                    buildingTransform.gameObject.SetActive(false);
                    continue;
                }

                if (buildingName == "Solar Panels")
                {
                    this.checkSolarPanel(true);
                }
                buildingTransform.gameObject.SetActive(true);


                if (this.data.hidden)
                {
                    for (int index = 0; index < buildingTransform.childCount; index++)
                    {
                        Transform buildingGameobject = buildingTransform.GetChild(index);
                        SpriteRenderer render = buildingGameobject.GetComponent<SpriteRenderer>();
                        if (render == null)
                        {
                            for (int sub_index = 0; sub_index < buildingGameobject.childCount; sub_index++)
                            {
                                render = buildingGameobject.GetChild(sub_index).GetComponent<SpriteRenderer>();
                                if (render == null)
                                {
                                    continue;
                                }
                                render.enabled = false;
                            }
                            continue;
                        }
                        render.enabled = false;
                    }
                }
            }

            if (this.data.hidden)
            {
                SpriteRenderer render = this.transform.Find("Colony Base").Find("Building").GetComponent<SpriteRenderer>();
                if (render != null)
                {
                    render.enabled = false;
                }
            }

            this.InjectData();
        }

        private void checkSolarPanel(bool state)
        {
            this._hasEnergy = state;
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
            public string name;
            public bool hidden;
            public string address;

            public Dictionary<string, Building> structures;

            public Dictionary<string, double> resources;

            public ColonyData()
            {
                this.structures = new Dictionary<string, Building>();
                this.hidden = false;
                this.resources = new Dictionary<string, double>();
            }

            public ColonyData(string name, float angle, WorldLocation worldLocation)
            {
                this.angle = angle;
                this.name = name;
                this.position = worldLocation.position.Value;
                this.address = worldLocation.planet.Value.codeName;
                this.structures = new Dictionary<string, Building>();
                this.resources = new Dictionary<string, double>();
                this.hidden = false;
            }

            public ColonyData(float angle, string planetName, Double2 position)
            {
                this.angle = angle;
                this.position = position;
                this.address = planetName;
                this.structures = new Dictionary<string, Building>();
                this.resources = new Dictionary<string, double>();
                this.hidden = false;
            }

            public Planet getPlanet()
            {
                Planet planet;
                Base.planetLoader.planets.TryGetValue(this.address, out planet);
                return planet;
            }

            public void setWorldLocation(WorldLocation location)
            {
                this.position = location.position.Value;
                this.address = location.planet.Value.codeName;
            }

            public Double2 getBuildingPosition(string buildingName, float height = 0)
            {
                Building building;
                this.structures.TryGetValue(buildingName, out building);

                if (building == null)
                {
                    return Double2.CosSin((double)(0.017453292f * LandmarkAngle)) * (this.getPlanet().Radius + this.getPlanet().GetTerrainHeightAtAngle((double)(LandmarkAngle * 0.017453292f)) + height);
                }

                Double2 colonyPos = Double2.CosSin((double)(0.017453292f * LandmarkAngle)) * (this.getPlanet().Radius + this.getPlanet().GetTerrainHeightAtAngle((double)(LandmarkAngle * 0.017453292f)) + height);
                Vector2 buildingPos = Double2.CosSin((double)(0.017453292f * (this.angle))) * building.offset.x;
                return colonyPos + buildingPos;
            }

            public bool isBuildingActive(string buildingName)
            {
                Building building;
                this.structures.TryGetValue(buildingName, out building);

                if (building == null)
                {
                    return false;
                }
                return true;
            }

            private bool isValidColonyResource(string resourceType)
            {
                if (MorePartsTypes.CONSTRUCTION_MATERIAL == resourceType)
                {
                    return true;
                }

                if (MorePartsTypes.ELECTRONIC_COMPONENT == resourceType)
                {
                    return true;
                }

                if (MorePartsTypes.MATERIAL == resourceType)
                {
                    return true;
                }

                if (MorePartsTypes.ROCKET_MATERIAL == resourceType)
                {
                    return true;
                }
                return false;
            }

            public bool addResource(string resourceType, double quantity)
            {
                if (!this.isValidColonyResource(resourceType))
                {
                    return false;
                }

                if (!this.resources.ContainsKey(resourceType))
                {
                    this.resources.Add(resourceType, quantity);
                }
                else
                {
                    this.resources[resourceType] += quantity;
                }
                return true;
            }

            public double takeResource(string resourceType, double quantity)
            {
                if (!this.isValidColonyResource(resourceType))
                {
                    return 0;
                }

                if (!this.resources.ContainsKey(resourceType))
                {
                    this.resources.Add(resourceType, 0);
                    return 0;
                }

                if (this.resources[resourceType] - quantity < 0)
                {
                    double total = this.resources[resourceType];
                    this.resources[resourceType] -= this.resources[resourceType];
                    return total;
                }

                this.resources[resourceType] -= quantity;
                return quantity;
            }

            public double getResource(string resourceType)
            {
                if (!this.isValidColonyResource(resourceType))
                {
                    return 0;
                }

                if (this.resources.ContainsKey(resourceType))
                {
                    return this.resources[resourceType];
                }
                this.resources.Add(resourceType, 0);
                return 0;
            }

            public float LandmarkAngle { get => this.angle + 90; }

            public override string ToString()
            {
                string result = "Colony " + this.name + "\n";
                result += "address " + this.address + "\n";
                result += "Resources\n";
                foreach (string key in this.resources.Keys)
                {
                    result += key + ": " + this.resources[key] + "\n";
                }
                return result;
            }
        }

        public class Building
        {
            public Double2 offset;

            public Building(Double2 pos)
            {
                this.offset = pos;
            }

            public Building()
            {
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
