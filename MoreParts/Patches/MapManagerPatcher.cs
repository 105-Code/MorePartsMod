using HarmonyLib;
using MorePartsMod.Buildings;
using MorePartsMod.Managers;
using SFS.World.Maps;
using SFS.WorldBase;
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
                if (ColonyManager.main == null)
                {
                    return;
                }

                // Draw telecommunication lines and space center point
                AntennaComponent.main.DrawInMap();

                // Draw colonies landmarks
                ColonyManager.main.DrawInMap();

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
