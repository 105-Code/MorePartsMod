using HarmonyLib;
using MorePartsMod.Parts;
using SFS.World;

namespace MorePartsMod.Patches
{
    [HarmonyPatch(typeof(Rocket), "Awake")]
    class RocketPatcher
    {
        [HarmonyPostfix]
        public static void Postfix(Rocket __instance)
        {
            __instance.gameObject.AddComponent<CenterOfMassDebug>();
        }
    }
}
