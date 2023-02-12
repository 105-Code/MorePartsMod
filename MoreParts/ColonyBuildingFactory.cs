using System.Collections.Generic;

namespace MorePartsMod
{
    public class ColonyBuildingFactory
    {

        private Dictionary<string, BuildingData> _buildings;
        private string[] _buildingNames;

        public ColonyBuildingFactory()
        {
            this._buildings = new Dictionary<string, BuildingData>();
            this._buildings.Add("Refinery", new BuildingData(12,10));
            this._buildings.Add("Solar Panels", new BuildingData(  4, 13));
            this._buildings.Add("VAB", new BuildingData( 10, 4));
            this._buildings.Add("Launch Pad", new BuildingData(  20, 1, new Double2(100, 3) ) );
        }

        public BuildingData getColonyBuilding(string name)
        {
            BuildingData result;
            this._buildings.TryGetValue(name, out result);
            return result;
        }

        public string[] GetBuildingsName()
        {
            if(this._buildingNames == null)
            {
                this._buildingNames = new string[this._buildings.Count];
                short i = 0;
                foreach(string building in this._buildings.Keys)
                {
                    this._buildingNames[i] = building;
                    i += 1;
                }
            }
            return this._buildingNames;
        }

        public class BuildingData
        {
            public float constructionCost;
            public float electronicCost;
            public Double2 offset;

            public BuildingData() { }

            public BuildingData(float constructionCost, float electronicCost, Double2 pos)
            {
                this.constructionCost = constructionCost;
                this.electronicCost = electronicCost;
                this.offset = pos;
            }
           
            public BuildingData(float constructionCost, float electronicCost)
            {
                this.constructionCost = constructionCost;
                this.electronicCost = electronicCost;
            }

        }

    }
}
