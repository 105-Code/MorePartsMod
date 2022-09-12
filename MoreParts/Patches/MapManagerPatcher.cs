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
                AntennaComponent.main.DrawInMap();

                if (ColonyManager.main == null)
                {
                    return;
                }

                foreach (ColonyComponent colony in ColonyManager.main.Colonies)
                {
                    Planet planet = colony.data.getPlanet();
                    double num = planet.data.basics.radius * 6;
                    float num2 = Mathf.Min(MapDrawer.GetFadeIn(Map.view.view.distance, num * 0.5, num * 0.4), MapDrawer.GetFadeOut(Map.view.view.distance, 20000.0, 15000.0));
                    if (num2 > 0f)
                    {
                        Color color = new Color(1f, 1f, 1f, num2);
                        Vector2 normal = Double2.CosSin(0.017453292f * (colony.data.LandmarkAngle + 6));
                        float x = (float)(planet.mapHolder.position.x + (colony.data.position.x / 1000));
                        float y = (float)(planet.mapHolder.position.y + (colony.data.position.y / 1000));
                        Vector2 position = new Vector2(x, y);

                        MapDrawer.DrawPointWithText(20, color, colony.data.name, 40, color, position, normal, 4, 4);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}
