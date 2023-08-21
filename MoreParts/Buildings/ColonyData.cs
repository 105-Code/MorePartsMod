using MorePartsMod.ARPA;
using MorePartsMod.Managers;
using SFS;
using SFS.UI;
using SFS.Variables;
using SFS.World;
using SFS.WorldBase;
using System.Collections.Generic;
using UnityEngine;

namespace MorePartsMod.Buildings
{
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
    }
}
