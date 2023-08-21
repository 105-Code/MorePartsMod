using ModLoader;
using UnityEngine;
using System.Collections.Generic;
using HarmonyLib;
using ModLoader.Helpers;
using MorePartsMod.Managers;
using static MorePartsMod.Buildings.ColonyComponent;
using MorePartsMod.Utils;

namespace MorePartsMod
{
    [CreateAssetMenu(fileName = "MorePartsMod", menuName = "MoreParts Mod", order = 1)]
    public class MorePartsPack : PackData
    {
        public static MorePartsPack Main;

        public const string ModFolderName = "MorePartsMod";
        public const string ModIdPatching = "morepartsmod.danielrojas.website";

        public GameObject AntennaPrefab;
        public GameObject ColonyPrefab;
        public MockMod Mod { get; private set; }

        private GameObject _manager;

        public ColonyData SpawnPoint { get; set; }
        public ColonyBuildingFactory ColonyBuildingFactory { private set; get; }
        public List<ColonyData> ColoniesInfo { set; get; }

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

            Mod = new MockMod(ModIdPatching, DisplayName, Author);
            Mod.ModFolder = FileLocations.BaseFolder.Extend("/../Saving").Extend(ModFolderName).CreateFolder();

            ColonyBuildingFactory = new ColonyBuildingFactory();

            KeySettings.Setup();

            SceneHelper.OnWorldSceneLoaded += this.LoadWorld;
            SceneHelper.OnWorldSceneUnloaded += this.UnloadWorld;
            SceneHelper.OnHubSceneLoaded += this.OnHub;
            SceneHelper.OnHomeSceneLoaded += this.OnHome;

            Harmony harmony = new Harmony(ModIdPatching);
            harmony.PatchAll();
        }

        #region Listeners
        private void OnHome()
        {
            this.ColoniesInfo = null;
        }

        private void LoadWorld()
        {
            this._manager = GameObject.Instantiate(new GameObject("MorepartsManagers"));
            this._manager.AddComponent<ColonyManager>();
            this._manager.AddComponent<ResourcesManger>();
        }

        private void UnloadWorld()
        {
            GameObject.Destroy(this._manager);
            this._manager = null;
        }

        private void OnHub()
        {
            if (this.ColoniesInfo != null)
            {
                return;
            }

            List<ColonyData> data;
            FileUtils.LoadWorldPersistent("Colonies.json", out data);
            if (data == null)
            {
                return;
            }

            this.ColoniesInfo = data;
        }

        #endregion

    }
}
