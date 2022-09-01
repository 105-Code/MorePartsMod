using HarmonyLib;
using ModLoader;
using ModLoader.Helpers;
using MorePartsMod.Managers;
using MorePartsMod.Parts;
using SFS;
using SFS.Parts;
using System;
using System.IO;
using UnityEngine;

namespace MorePartsMod
{
    public class MorePartsMod : Mod
    {

        public static MorePartsMod Main;

        private AssetBundle _assets;
        public AssetBundle Assets
        {
            get { return this._assets; }
        }


        public override string ModNameID => "morepartsmod";

        public override string DisplayName => "MoreParts Mod";

        public override string Author => "dani0105";

        public override string MinimumGameVersionNecessary => "0.3.7";

        public override string ModVersion => "v2.0.0";

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
            Harmony harmony = new Harmony("morepartsmod.danielrojas.website");
            harmony.PatchAll();
        }

        public override void Load()
        {
            KeySettings.Setup();
        }
            

        private void OnWorld()
        {
            GameObject colonyManager = GameObject.Instantiate(new GameObject("Colony Manager"));
            colonyManager.AddComponent<ColonyManager>();
        }

    }
}
