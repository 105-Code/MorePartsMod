using HarmonyLib;
using ModLoader;
using ModLoader.Helpers;
using MorePartsMod.Buildings;
using MorePartsMod.Managers;
using MorePartsMod.Parts;
using SFS.UI.ModGUI;
using System;
using UnityEngine;

namespace MorePartsMod
{
    public class MorePartsMod : Mod
    {

        public static MorePartsMod Main;


        public MorePartsMod():base("morepartsmod", "MorePartsMod", "dani0105","0.3.7","v1.2.0","Add new parts and things!","moreparts-assets")
        {
            Main = this;
        }

        public override void Early_Load()
        {
            SceneHelper.OnWorldSceneLoaded += this.OnWorld;
            Harmony harmony = new Harmony("morepartsmod.danielrojas.website");
            harmony.PatchAll();
        }      

        public override void Load()
        {
            KeySettings.Setup();
            // add components
            Debug.Log("Adding components");
            BalloonModule.Setup();
            TelecommunicationDishModule.Setup();
            RotorModule.Setup();
        }


        public override void Unload()
        {
            throw new NotImplementedException();
        }

        private void OnWorld()
        {
            GameObject colonyManager = GameObject.Instantiate(new GameObject("Colony Manager"));
            colonyManager.AddComponent<ColonyManager>();
        }

    }
}
