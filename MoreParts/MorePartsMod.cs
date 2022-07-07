using HarmonyLib;
using ModLoader;
using ModLoader.Helpers;
using MorePartsMod.Parts;
using System;
using UnityEngine;

namespace MorePartsMod
{
    public class MorePartsMod : Mod
    {

        public static MorePartsMod Main;


        public MorePartsMod():base("morepartsmod", "MorePartsMod", "dani0105","0.3.7","v1.1.0","Add new parts!","moreparts-assets")
        {
            Main = this;
        }

        public override void Early_Load()
        {
            Harmony harmony = new Harmony("morepartsmod.danielrojas.website");
            harmony.PatchAll();
        }      

        public override void Load()
        {
            // add components
            Debug.Log("Adding components!");
            KeySettings.Setup();
            BalloonModule.Setup();
            TelecommunicationDishModule.Setup();
        }

        public override void Unload()
        {
            throw new NotImplementedException();
        }
    }
}
