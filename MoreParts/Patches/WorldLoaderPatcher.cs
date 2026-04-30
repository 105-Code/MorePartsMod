using HarmonyLib;
using MorePartsMod.Parts;
using SFS.World;

namespace MorePartsMod.Patches
{
    // Prevents the WorldLoader from disabling physics on a rocket that has an active auto-lander,
    // so the stage keeps simulating even when the player is controlling a different stage far away.
    [HarmonyPatch(typeof(WorldLoader), "UpdateLoading", new System.Type[] { typeof(Location) })]
    class WorldLoader_AutoLander_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(WorldLoader __instance)
        {
            foreach (Rocket rocket in AutoLanderModule.ActiveAutoLanders)
            {
                if (rocket != null && rocket.physics != null && rocket.physics.loader == __instance)
                    return false;
            }
            return true;
        }
    }
}