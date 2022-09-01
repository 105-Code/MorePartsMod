using Cysharp.Threading.Tasks;
using HarmonyLib;
using MorePartsMod;
using MorePartsMod.Buildings;
using MorePartsMod.Managers;
using MorePartsMod.Parts;
using SFS;
using SFS.Parts;
using SFS.World;
using SFS.World.Maps;
using SFS.WorldBase;
using System;
using UnityEngine;

namespace MorePartsMod
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

    [HarmonyPatch(typeof(SpaceCenter), "Start")]
    class AntennaPatcher
    {
 
        [HarmonyPrefix]
        public static void Prefix(SpaceCenter __instance)
        {
            SpaceCenterData spaceCenter = Base.planetLoader.spaceCenter;

            GameObject gameObject = GameObject.Instantiate(MorePartsMod.Main.Assets.LoadAsset<GameObject>("Telecommunication Antenna"));
            gameObject.transform.parent = __instance.transform;
            gameObject.SetActive(true);

            WorldBuildings.addBuildingToWorld(gameObject, "Model", spaceCenter.address.GetPlanet(), spaceCenter.LaunchPadLocation.position + new Double2(-150.0, -10.0));

            StaticWorldObject buildingObject = gameObject.GetComponent<StaticWorldObject>();
            WorldLoader worldLoader = gameObject.GetComponent<WorldLoader>();
            SpaceCenter.Building building = new SpaceCenter.Building();
            building.building = buildingObject;

            __instance.buildings.meshRenderers.AddItem(worldLoader.holder.GetComponent<MeshRenderer>());
            gameObject.AddComponent<AntennaComponent>();
        }
    }

    [HarmonyPatch(typeof(MapManager), "DrawLandmarks")]
    class MapManagerPatcher
    {

        [HarmonyPostfix]
        public static void Postfix()
        {
            try
            {
                AntennaComponent.main.DrawInMap();

                if(ColonyManager.main == null)
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
                        float x = (float) (planet.mapHolder.position.x + (colony.data.position.x / 1000));
                        float y = (float)(planet.mapHolder.position.y + (colony.data.position.y / 1000));
                        Vector2 position = new Vector2( x, y);

                        MapDrawer.DrawPointWithText(20, color, colony.data.name, 40, color, position, normal, 4, 4);
                    }
                }
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
        }
    
    }

}
