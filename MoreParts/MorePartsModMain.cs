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
        
        public List<ColonyData> ColoniesInfo { set; get; }
        
        public ColonyBuildingFactory ColonyBuildingFactory { private set; get; }

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
            this.ColonyBuildingFactory = new ColonyBuildingFactory();
        }

        public override void Early_Load()
        {
            string assetFilePath = Path.Combine(this.ModFolder, "moreparts-assets");
            AssetBundle assets = AssetBundle.LoadFromFile(assetFilePath);
            if (assets == null)
            {

                return;
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
            if (this.ColoniesInfo != null)
            {
                return;
            }
            Debug.Log("Loading Colonies info");
            List<ColonyData> data;
            LoadWorldPersistent("Colonies.json", out data);
            if (data == null)
            {
                return;
            }

            // Remove this for next version
            foreach (ColonyData colony in data)
            {
                if (colony.structures.Keys.Count == 0)
                {
                    foreach (ColonyBuildingData building in colony.buildings)
                    {
                        if (!building.state)
                        {
                            continue;
                        }
                        colony.structures.Add(building.name, new Building(building.offset));
                    }
                }

                if(colony.address == "" || colony.address == null)
                {
                    colony.address = colony.andress;
                }
            }

            this.ColoniesInfo = data;
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

        #endregion
    
    }
}
