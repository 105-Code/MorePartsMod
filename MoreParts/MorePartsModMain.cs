using HarmonyLib;
using ModLoader;
using ModLoader.Helpers;
using MorePartsMod.Buildings;
using MorePartsMod.Managers;
using MorePartsMod.Parts;
using MorePartsMod.World;
using SFS;
using SFS.IO;
using SFS.Parsers.Json;
using SFS.Parts;
using SFS.WorldBase;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static MorePartsMod.Buildings.ColonyComponent;

namespace MorePartsMod
{
    public class MorePartsModMain : Mod
    {
        #region variables
        public static MorePartsModMain Main;

        public List<ColonyBuildingData> Buildings { private set; get; }
        
        public List<ColonyData> ColoniesInfo { private set; get; }

        

        public ColonyData spawnPoint;

        private GameObject _managers;

        public AssetBundle Assets { private set; get; }

        #region mod information
        public override string ModNameID => "morepartsmod";

        public override string DisplayName => "MoreParts Mod";

        public override string Author => "dani0105";

        public override string MinimumGameVersionNecessary => "0.3.7";

        public override string ModVersion => "2.1.1";

        public override string Description => "Add special features to the MoreParts Pack";

        #endregion

        #endregion

        public MorePartsModMain()
        {
            Main = this;
        }

        public override void Early_Load()
        {
            string assetFilePath = Path.Combine(this.ModFolder, "moreparts-assets");
            AssetBundle assets = AssetBundle.LoadFromFile(assetFilePath);
            if (assets == null)
            {
                throw new Exception("Assets file not found");
            }
            this.Assets = assets;

            SceneHelper.OnWorldSceneLoaded += this.LoadWorld;
            SceneHelper.OnWorldSceneUnloaded += this.UnloadWorld;
            SceneHelper.OnHubSceneLoaded += this.OnHub;
            SceneHelper.OnHomeSceneLoaded += this.OnHome;

            Harmony harmony = new Harmony("morepartsmod.danielrojas.website");
            harmony.PatchAll();
        }

        public override void Load()
        {
            KeySettings.Setup();

            this.Buildings = new List<ColonyBuildingData>();
            this.Buildings.Add(new ColonyBuildingData(false, "Refinery", new ColonyBuildingCost(10, 12) ));
            this.Buildings.Add(new ColonyBuildingData(false, "Solar Panels", new ColonyBuildingCost(13, 4) ));
            this.Buildings.Add(new ColonyBuildingData(false, "VAB", new ColonyBuildingCost(4, 10) ));
            this.Buildings.Add(new ColonyBuildingData(false, "Launch Pad", new ColonyBuildingCost(1, 20), new Double2(100, 3)));

        }


        #region Listeners
        private void OnHome()
        {
            Debug.Log("Removing Colonies info");
            this.ColoniesInfo = null;
        }

        private void LoadWorld()
        {
            this._managers = GameObject.Instantiate(new GameObject("MorepartsManagers"));
            this._managers.AddComponent<ColonyManager>();
            this._managers.AddComponent<ResourcesManger>();
        }

        private void UnloadWorld()
        {
            GameObject.Destroy(this._managers);
            this._managers = null;
        }

        private void OnHub()
        {
            Debug.Log("Loading Colonies info");
            this.LoadColonyInfo();
        }

        #endregion


        #region worlds files

        public static void SaveWorldPersistent(string filename, object data)
        {
            FilePath file = Base.worldBase.paths.worldPersistentPath.ExtendToFile(filename);
            JsonWrapper.SaveAsJson(file, data, true);
        }

        public static bool LoadWorldPersistent<T>(string filename, out T result)
        {
            result = default;
            FilePath file = Base.worldBase.paths.worldPersistentPath.ExtendToFile(filename);
            if (!file.FileExists())
            {
                return false;
            }
            JsonWrapper.TryLoadJson(file, out result);
            return true;
        }

        #region colonies information

        public void SaveColonyInfo(List<ColonyComponent> colonies)
        {
            List<ColonyData> data = new List<ColonyData>();
            foreach (ColonyComponent colony in colonies)
            {
                data.Add(colony.data);
            }
            this.ColoniesInfo = data;
            SaveWorldPersistent("Colonies.json", data);
        }

        public void LoadColonyInfo()
        {
            if(this.ColoniesInfo != null)
            {
                return;
            }
            List<ColonyData> data;
            LoadWorldPersistent<List<ColonyData>>("Colonies.json", out data);
            if (data == null)
            {
                return;
            }
            this.ColoniesInfo = data;   
        }

        #endregion

        #endregion
    
    }
}
