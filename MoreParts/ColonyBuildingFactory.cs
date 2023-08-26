using System;
using System.Collections.Generic;
using UnityEngine;

namespace MorePartsMod
{
    [Serializable]
    public class ColonyBuildingFactory
    {
        [SerializeField]
        public GameObject ColonyBuilding;
        [SerializeField]
        public GameObject LaunchPadBuilding;
        [SerializeField]
        public GameObject VabBuilding;
        [SerializeField]
        public GameObject SolarPanelBuilding;
        [SerializeField]
        public GameObject RefineryBuilding;
        private List<BuildingData> _buildings = new List<BuildingData>();
        private string[] _buildingNames;



        public BuildingData GetBuilding(string name)
        {
            foreach (BuildingData item in GetBuildings())
            {
                if (item.Name == name)
                {
                    return item;
                }
            }
            return null;
        }

        public BuildingData[] GetBuildings()
        {
            // i need to change this to better solution
            if (_buildings.Count == 0)
            {
                _buildings.Add(new BuildingData(20, 1, LaunchPadBuilding));
                _buildings.Add(new BuildingData(10, 4, VabBuilding));
                _buildings.Add(new BuildingData(4, 13, SolarPanelBuilding));
                _buildings.Add(new BuildingData(12, 10, RefineryBuilding));
            }
            return _buildings.ToArray();
        }

        public string[] GetBuildingsName()
        {
            if (this._buildingNames == null)
            {
                this._buildingNames = new string[this._buildings.Count];
                short i = 0;
                foreach (BuildingData building in GetBuildings())
                {
                    this._buildingNames[i] = building.Name;
                    i += 1;
                }
            }
            return this._buildingNames;
        }

        [Serializable]
        public class BuildingData
        {
            public string Name { get => Prefab.name; }
            public float ConstructionMaterialCost;
            public float ElectronicMaterialCost;
            public GameObject Prefab;

            public BuildingData(float constructionMaterialCost, float electronicMaterialCost, GameObject prefab)
            {
                ConstructionMaterialCost = constructionMaterialCost;
                ElectronicMaterialCost = electronicMaterialCost;
                Prefab = prefab;
            }
        }

    }
}
