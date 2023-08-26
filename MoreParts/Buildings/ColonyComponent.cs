using MorePartsMod.ARPA;
using MorePartsMod.Managers;
using MorePartsMod.Utils;
using SFS.UI;
using SFS.Variables;
using SFS.World;
using SFS.WorldBase;
using System;
using System.Collections.Generic;
using UnityEngine;
using static MorePartsMod.Buildings.ColonyData;
using static MorePartsMod.ColonyBuildingFactory;

namespace MorePartsMod.Buildings
{
    public class ColonyComponent : MonoBehaviour
    {
        public GameObject LimitFlag;
        public WorldLocation Location;
        public ColonyData Data;

        public Node Node { private set; get; }

        private Player_Local Rocket = new Player_Local();

        void Start()
        {
            if (GameManager.main == null)
            {
                return;
            }
            this.Node = AntennaComponent.main.AddNode(Location, true);
        }

        void FixedUpdate()
        {
            if (GameManager.main == null)
            {
                return;
            }

            Player player = ColonyManager.Main.Player.Value;

            if (!IsPlayerNear(player))
            {
                Rocket.Value = null;
                return;
            }

            if (Rocket.Value != null)
            {
                return;
            }

            MsgDrawer.main.Log("Welcome To " + Data.name);
            Rocket.Value = player;
        }

        private bool IsPlayerNear(Player player)
        {
            if (player == null || player.location == null || player.location.planet.Value == null)
            {
                return false;
            }
            // is in the same planet
            if (player.location.planet.Value.codeName != Data.address)
            {
                return false;
            }


            // is near to the colony
            float distance = Vector2.Distance(Data.position, player.location.position.Value);
            if (distance > ColonyData.SIZE)
            {
                return false;
            }
            return true;
        }

        private bool CheckAndReduceMaterials(float constructionMaterial, float electronicMaterial)
        {
            double constructionQuantity = this.Data.GetResource(MorePartsTypes.CONSTRUCTION_MATERIAL);
            if (constructionQuantity - constructionMaterial < 0)
            {
                return false;
            }
            double electronicQuantity = this.Data.GetResource(MorePartsTypes.ELECTRONIC_COMPONENT);
            if (electronicQuantity - electronicMaterial < 0)
            {
                return false;
            }
            this.Data.TakeResource(MorePartsTypes.CONSTRUCTION_MATERIAL, constructionMaterial);
            this.Data.TakeResource(MorePartsTypes.ELECTRONIC_COMPONENT, electronicMaterial);
            return true;
        }

        private Building SpawnBuilding(GameObject prefab, Double2 position, float angle)
        {
            GameObject buildingObject = GameObject.Instantiate(prefab);
            BuildingUtils.AddBuildingToWorld(buildingObject, Data.GetPlanet(), position, angle);
            return new Building(position, angle, buildingObject);
        }

        private void SpawnFlag(Double2 position, float angle)
        {
            GameObject flagGameObject = GameObject.Instantiate(LimitFlag);
            BuildingUtils.AddBuildingToWorld(flagGameObject, Data.GetPlanet(), position, angle);
        }

        public bool CreateBuilding(string buildingName)
        {
            BuildingData buildingData = MorePartsPack.Main.ColonyBuildingFactory.GetBuilding(buildingName);

            if (!this.CheckAndReduceMaterials(buildingData.ConstructionMaterialCost, buildingData.ElectronicMaterialCost))
            {
                MsgDrawer.main.Log("Insufficient Materials");
                return false;
            }

            Player player = ColonyManager.Main.Player;
            float buildingAngle = (float)player.location.position.Value.AngleRadians;
            Double2 buildingPos = BuildingUtils.GetPositionOnPlanetSurface(buildingAngle, Data.GetPlanet());

            Building building = SpawnBuilding(buildingData.Prefab, buildingPos, buildingAngle);
            Data.Buildings.Add(buildingData.Name, building);
            InjectDataToBuilding(building);
            ColonyManager.Main.SaveColonies();
            return true;
        }

        public void RestoreBuildings()
        {
            if (this.Data == null)
            {
                return;
            }

            float limitAngle = BuildingUtils.FindAngleForXDistance(Data.position, ColonyData.SIZE);
            float flagRigthAngle = Data.angle + limitAngle;
            float flagLeftAngle = Data.angle - limitAngle;
            Double2 flagRigthPos = BuildingUtils.GetPositionOnPlanetSurface(flagRigthAngle, Data.GetPlanet());
            Double2 flagLeftPos = BuildingUtils.GetPositionOnPlanetSurface(flagLeftAngle, Data.GetPlanet());
            SpawnFlag(flagRigthPos, flagRigthAngle);
            SpawnFlag(flagLeftPos, flagLeftAngle);

            foreach (BuildingData building in MorePartsPack.Main.ColonyBuildingFactory.GetBuildings())
            {

                if (!Data.IsBuildingActive(building.Name))
                {
                    continue;
                }

                Building data = Data.GetBuilding(building.Name);

                Building newData = SpawnBuilding(building.Prefab, data.position, data.rotation);
                data.GameObject = newData.GameObject;
            }

            InjectDataToAllBuildings();
        }

        private void InjectDataToAllBuildings()
        {
            foreach (Building item in Data.GetBuildings())
            {
                InjectDataToBuilding(item);
            }

            foreach (Building item in Data.GetBuildings())
            {
                ExecuteInterfaces(item);
            }
        }

        private void ExecuteInterfaces(Building building)
        {
            OnInit initObject = building.GameObject.GetComponent<OnInit>();
            if (initObject != null)
            {
                initObject.OnInit();
            }
        }

        private void InjectDataToBuilding(Building building)
        {
            Inject_INJ_Building(building.GameObject, building);
            Inject_INJ_Colony(building.GameObject);
            Inject_INJ_Rocket(building.GameObject, Rocket.Value as Rocket);
            Rocket.OnChange += () => Inject_INJ_Rocket(building.GameObject, Rocket.Value as Rocket);
        }

        public void Inject_INJ_Rocket(GameObject gameObject, Rocket rocket)
        {
            INJ_Rocket injRocket = gameObject.GetComponent<INJ_Rocket>();
            if (injRocket == null)
            {
                return;
            }
            injRocket.Rocket = rocket;
        }

        public void Inject_INJ_Colony(GameObject gameObject)
        {
            INJ_Colony injColony = gameObject.GetComponent<INJ_Colony>();
            if (injColony == null)
            {
                return;
            }
            injColony.Colony = this;
        }

        public void Inject_INJ_Building(GameObject gameObject, Building building)
        {
            INJ_Building injBuilding = gameObject.GetComponent<INJ_Building>();
            if (injBuilding == null)
            {
                return;
            }
            injBuilding.Building = building;
        }

        public interface INJ_Rocket
        {
            Rocket Rocket { set; }
        }

        public interface INJ_Building
        {
            Building Building { set; }
        }

        public interface INJ_Colony
        {
            ColonyComponent Colony { set; }
        }

        public interface INJ_HasEnergy
        {
            bool HasEnergy { set; }
        }

        public interface OnInit
        {
            void OnInit();
        }

    }
}
