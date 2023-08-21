using ModLoader;
using UnityEngine;
using SFS.Parts;
using System;
using SFS.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using HarmonyLib;
using SFS;
using SFS.Parsers.Json;
using ModLoader.Helpers;
using MorePartsMod.Managers;
using static MorePartsMod.Buildings.ColonyComponent;

namespace MorePartsMod
{
    [CreateAssetMenu(fileName = "MorePartsMod", menuName = "MoreParts Mod", order = 1)]
    public class MorePartsPack : PackData
    {
        public static MorePartsPack Main;
        public MockMod Mod;
        public const string ModIdPatching = "www.danielrojas.website";
        public GameObject AntennaPrefab;
        public GameObject ColonyPrefab;

        public ColonyBuildingFactory ColonyBuildingFactory { private set; get; }
        public List<ColonyData> ColoniesInfo { set; get; }

        public ColonyData spawnPoint;
        private GameObject _managers;


        public MorePartsPack()
        {
            Main = this;
        }

        public void OnEnable()
        {
            if (Application.isEditor)
            {
                return;
            }
            Mod = new MockMod();
            ColonyBuildingFactory = new ColonyBuildingFactory();

            Mod.ModFolder = Loader.ModsFolder.Extend(Mod.ModFolderName).CreateFolder();

            KeySettings.Setup();

            SceneHelper.OnWorldSceneLoaded += this.LoadWorld;
            SceneHelper.OnWorldSceneUnloaded += this.UnloadWorld;
            SceneHelper.OnHubSceneLoaded += this.OnHub;
            SceneHelper.OnHomeSceneLoaded += this.OnHome;


            Harmony harmony = new Harmony(ModIdPatching);
            harmony.PatchAll();
        }

        public void Start()
        {
            Debug.Log("Start MoreParts");
        }

        public void Update()
        {
            Debug.Log("Update MoreParts");
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

            this.ColoniesInfo = data;
        }

        #endregion

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

        public class MockMod : Mod
        {
            public override string ModNameID => "morepartsmod.danielrojas.website";

            public override string DisplayName => "MoreParts Mod";

            public override string Author => "dani0105";

            public override string MinimumGameVersionNecessary => "1.5.9.8";

            public override string ModVersion => "3.0.2";

            public override string Description => "Add special features to the MoreParts Pack";
            public string ModFolderName => "MorePartsMod";
        }
    }
}
