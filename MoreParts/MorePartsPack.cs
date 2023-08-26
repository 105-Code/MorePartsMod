using ModLoader;
using UnityEngine;
using System.Collections.Generic;
using HarmonyLib;
using ModLoader.Helpers;
using MorePartsMod.Managers;
using MorePartsMod.Buildings;
using MorePartsMod.Utils;

namespace MorePartsMod
{
    [CreateAssetMenu(fileName = "MorePartsData", menuName = "MoreParts Mod", order = 1)]
    public class MorePartsPack: ScriptableObject
    {
        public static MorePartsPack Main;

        public const string ModFolderName = "MorePartsMod";
        public const string ModIdPatching = "morepartsmod.danielrojas.website";

        public GameObject AntennaPrefab;
        public MockMod Mod { get; private set; }

        private GameObject _manager;

        public ColonyData SpawnPoint { get; set; }

        [SerializeReference]
        public ColonyBuildingFactory ColonyBuildingFactory;
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
            Debug.Log("Loading MoreParts Mod!");
            Mod = new MockMod(ModIdPatching, "MoreParts Mod","dani0105");
            Mod.ModFolder = FileLocations.BaseFolder.Extend("/../Saving").Extend(ModFolderName).CreateFolder();

            KeySettings.Setup();

            SceneHelper.OnWorldSceneLoaded += this.LoadWorld;
            SceneHelper.OnWorldSceneUnloaded += this.UnloadWorld;
            SceneHelper.OnHubSceneLoaded += this.OnHub;
            SceneHelper.OnHomeSceneLoaded += this.OnHome;

            Harmony harmony = new Harmony(ModIdPatching);
            harmony.PatchAll();
            Debug.Log("Loaded!");
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
