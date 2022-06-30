using HarmonyLib;
using ModLoader;
using ModLoader.Helpers;
using MorePartsMod.Parts;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MorePartsMod
{
    public class MorePartsMod : Mod
    {

        public static MorePartsMod main;

        public MorePartsMod():base("morepartsmod", "MorePartsMod", "dani0105","0.3.7","v1.0.0")
        {
            main = this;
        }

        public override void Early_Load()
        {
            Harmony harmony = new Harmony("website.danielrojas.morepartsmod");
            harmony.PatchAll();
        }      

        public override void Load()
        {
            // add components
            Debug.Log("Adding components!");
            BalloonModule.Setup();
        }

        public override void Unload()
        {
            throw new NotImplementedException();
        }
    }
}
