
using Cysharp.Threading.Tasks;
using HarmonyLib;
using MorePartsMod.Parts;
using SFS;
using SFS.Parts;
using System;
using UnityEngine;
using static MorePartsMod.CustomModulesManager;

namespace MorePartsMod.Patches
{
    [HarmonyPatch(typeof(CustomAssetsLoader), "LoadAllCustomAssets")]
    class CustomAssetsLoaderPatcher
    {
        [HarmonyPostfix]
        public static void Postfix(ref UniTask __result)
        {
            __result.GetAwaiter().OnCompleted(AddModules);
        }

        private static void AddModules()
        {
            // add components
            Debug.Log("Adding moreparts components");
            BalloonModule.Setup();
            TelecommunicationDishModule.Setup();
            RotorModule.Setup();

            // add Components to parts from other mods
            Debug.Log("Add Modreparts modules to external parts");
            foreach( CustomModuleData item in MorePartsModMain.Main.CustomModules.CustomModulesQueue)
            {
                Part part;
                Base.partsLoader.parts.TryGetValue(item.partName, out part);

                if (part == null)
                {
                    Debug.Log(item.partName + " not found!");
                    continue;
                }
                try
                {
                    part.gameObject.AddComponent(item.customModule);
                    Debug.Log(item.customModule.ToString() + " added to" + item.partName);
                }
                catch( Exception error)
                {
                    Debug.Log("Error attaching custom module to external part");
                    Debug.LogError(error);
                }
            }
            Debug.Log("Add MorePart modules done");
        }
    }
}
