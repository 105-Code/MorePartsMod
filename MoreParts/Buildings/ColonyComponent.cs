using MorePartsMod.ARPA;
using MorePartsMod.Managers;
using SFS;
using SFS.UI;
using SFS.Variables;
using SFS.World;
using System.Collections.Generic;
using UnityEngine;
using static MorePartsMod.Buildings.ColonyData;
using static MorePartsMod.ColonyBuildingFactory;

namespace MorePartsMod.Buildings
{
    public class ColonyComponent : MonoBehaviour
    {

        public ColonyData data;
        public Transform ColonyHolder;

        public WorldLocation Location;

        private Bool_Local PlayerInPlanet = new Bool_Local();
        private Bool_Local PlayerNear = new Bool_Local();

        private bool _hasEnergy;
        private Dictionary<string, object> _modules = new Dictionary<string, object>();

        public Node Node { private set; get; }

        private void Start()
        {
            if (GameManager.main == null)
            {
                return;
            }
            this.Node = AntennaComponent.main.AddNode(Location, true);
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
                this.PlayerInPlanet.Value = false;
            }
            this.PlayerInPlanet.Value = true;
        }

        private void FixedUpdate()
        {
            if (GameManager.main == null || !this.PlayerInPlanet.Value || !ColonyManager.main.player.Value)
            {
                return;
            }

            if (Vector2.Distance(this.data.position, ColonyManager.main.player.Value.location.position.Value) > 100)
            {
                this.PlayerNear.Value = false;
                return;
            }
            this.PlayerNear.Value = true;

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
            ColonyHolder.Find(buildingName).gameObject.SetActive(true);
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
                Transform buildingTransform = ColonyHolder.Find(buildingName);
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

            this.PlayerInPlanet.OnChange += this.InjectPlayerInPlanet;
            this.PlayerNear.OnChange += this.InjectPlayerNear;
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
                list[i].PlayerNear = this.PlayerNear.Value;
            }
        }

        private void InjectPlayerInPlanet()
        {

            INJ_PlayerInPlanet[] list = this.CollectModules<INJ_PlayerInPlanet>();
            for (int i = 0; i < list.Length; i++)
            {
                list[i].PlayerInPlanet = this.PlayerInPlanet.Value;
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
        #endregion
    }
}
