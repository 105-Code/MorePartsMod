using HarmonyLib;
using MorePartsMod.Buildings;
using MorePartsMod.Managers;
using SFS.World.Maps;
using System;
using UnityEngine;

namespace MorePartsMod.Patches
{
    [HarmonyPatch(typeof(MapManager), "DrawLandmarks")]
    class MapManagerPatcher
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            try
            {
                if (ColonyManager.Main == null)
                {
                    return;
                }

                // Draw telecommunication lines and space center point
                AntennaComponent.main.DrawInMap();

                // Draw colonies landmarks
                ColonyManager.Main.DrawInMap();

                // Draw resources landmarks
                ResourcesManger.Main.DrawInMap();

            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

    }
}
