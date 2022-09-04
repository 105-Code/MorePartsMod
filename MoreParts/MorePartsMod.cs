using HarmonyLib;
using ModLoader;
using ModLoader.Helpers;
using MorePartsMod.Buildings;
using MorePartsMod.Managers;
using MorePartsMod.Parts;
using SFS;
using SFS.IO;
using SFS.Parsers.Json;
using SFS.Parts;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static MorePartsMod.Buildings.ColonyComponent;

namespace MorePartsMod
{
    class MorePartsMod : Mod
    {

        public static MorePartsMod Main;

        public List<ColonyBuildingData> Buildings { get => this._buldingsList; }
        public List<ColonyData> ColoniesInfo { get => this._coloniesInfo; }

        public ColonyData spawnPoint;

        private AssetBundle _assets;
        private List<ColonyBuildingData> _buldingsList;
        private List<ColonyData> _coloniesInfo;


        public AssetBundle Assets
        {
            get { return this._assets; }
        }


        public override string ModNameID => "morepartsmod";

        public override string DisplayName => "MoreParts Mod";

        public override string Author => "dani0105";

        public override string MinimumGameVersionNecessary => "0.3.7";

        public override string ModVersion => "v2.1.0";

        public override string Description => "Add special features to the MoreParts Pack";

        public override void Early_Load()
        {
            Main = this;

            string assetFilePath = Path.Combine(this.ModFolder, "moreparts-assets");
            AssetBundle assets = AssetBundle.LoadFromFile(assetFilePath);
            if (assets == null)
            {
                throw new Exception("Assets file not found");
            }
            this._assets = assets;

            SceneHelper.OnWorldSceneLoaded += this.OnWorld;
            SceneHelper.OnBuildSceneLoaded += this.OnBuild;

            Harmony harmony = new Harmony("morepartsmod.danielrojas.website");
            harmony.PatchAll();
        }

        public override void Load()
        {
            KeySettings.Setup();

            this._buldingsList = new List<ColonyBuildingData>();
            this._buldingsList.Add(new ColonyBuildingData(false, "Refinery", new ColonyBuildingCost(10, 12) ));
            this._buldingsList.Add(new ColonyBuildingData(false, "Solar Panels", new ColonyBuildingCost(13, 4) ));
            this._buldingsList.Add(new ColonyBuildingData(false, "VAB", new ColonyBuildingCost(4, 10) ));
            this._buldingsList.Add(new ColonyBuildingData(false, "Launch pad", new ColonyBuildingCost(1, 20), new Double2(100, 3)));

        }
            

        private void OnWorld()
        {
            this.LoadColonyInfo();
            GameObject colonyManager = GameObject.Instantiate(new GameObject("Colony Manager"));
            colonyManager.AddComponent<ColonyManager>();
        }

        private void OnBuild()
        {
            this.LoadColonyInfo();
        }


        public void SaveColonyInfo(List<ColonyComponent> colonies)
        {
            FilePath file = Base.worldBase.paths.worldPersistentPath.ExtendToFile("Colonies.json");
            List<ColonyData> data = new List<ColonyData>();
            foreach (ColonyComponent colony in colonies)
            {
                data.Add(colony.data);
            }
            JsonWrapper.SaveAsJson(file, data, true);
            this._coloniesInfo = data;
        }

        public void LoadColonyInfo()
        {
            if(this._coloniesInfo != null)
            {
                return;
            }

            FilePath file = Base.worldBase.paths.worldPersistentPath.ExtendToFile("Colonies.json");
            if (!file.FileExists())
            {
                return;
            }
            List<ColonyData> data;
            JsonWrapper.TryLoadJson(file, out data);
            if (data == null)
            {
                return;
            }
            this._coloniesInfo = data;
            
        }


    }
}
