using SFS;
using SFS.World;
using SFS.WorldBase;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MorePartsMod.Buildings
{
    [Serializable]
    public class ColonyData
    {
        public float angle;
        public Double2 position;
        public string name;
        public string address;
        public const float  SIZE = 200;

        public Dictionary<string, Building> Buildings;

        public Dictionary<string, double> resources;

        public ColonyData()
        {
            this.Buildings = new Dictionary<string, Building>();
            this.resources = new Dictionary<string, double>();
        }

        public ColonyData(string name, float angle, WorldLocation worldLocation)
        {
            this.angle = angle;
            this.name = name;
            this.position = worldLocation.position.Value;
            this.address = worldLocation.planet.Value.codeName;
            this.Buildings = new Dictionary<string, Building>();
            this.resources = new Dictionary<string, double>();
        }

        public ColonyData(float angle, string planetName, Double2 position)
        {
            this.angle = angle;
            this.position = position;
            this.address = planetName;
            this.Buildings = new Dictionary<string, Building>();
            this.resources = new Dictionary<string, double>();
        }

        public float LandmarkAngle { get => this.angle + 90; }

        public Planet GetPlanet()
        {
            Planet planet;
            Base.planetLoader.planets.TryGetValue(this.address, out planet);
            return planet;
        }

        public Location GetLocation()
        {
            return new Location(GetPlanet(), position);
        }

        public Building GetBuilding(string buildingName)
        {
            this.Buildings.TryGetValue(buildingName, out Building building);

            if (building == null)
            {
                return null;
            }
            return building;
        }

        public Building[] GetBuildings()
        {
            List<Building> result = new List<Building>();
            foreach (Building item in Buildings.Values)
            {
                result.Add(item);
            }
            return result.ToArray();
        }

        public bool IsBuildingActive(string buildingName)
        {
            this.Buildings.TryGetValue(buildingName, out Building building);

            if (building == null)
            {
                return false;
            }
            return true;
        }

        private bool IsValidColonyResource(string resourceType)
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

        public bool AddResource(string resourceType, double quantity)
        {
            if (!this.IsValidColonyResource(resourceType))
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

        public double TakeResource(string resourceType, double quantity)
        {
            if (!this.IsValidColonyResource(resourceType))
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

        public double GetResource(string resourceType)
        {
            if (!this.IsValidColonyResource(resourceType))
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

    }
}
