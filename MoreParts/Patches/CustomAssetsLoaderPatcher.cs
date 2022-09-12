
using Cysharp.Threading.Tasks;
using HarmonyLib;
using MorePartsMod.Parts;
using SFS.Parts;
using UnityEngine;

namespace MorePartsMod.Patches
{
    [HarmonyPatch(typeof(CustomAssetsLoader), "LoadAllCustomAssets")]
    class CustomAssetsLoaderPatcher
    {
        [HarmonyPostfix]
        public static void Postfix(ref UniTask __result)
        {
            __result.GetAwaiter().OnCompleted(test);
        }

        private static void test()
        {
            // add components
            Debug.Log("Adding moreparts components");
            BalloonModule.Setup();
            TelecommunicationDishModule.Setup();
            RotorModule.Setup();
        }
    }
}
