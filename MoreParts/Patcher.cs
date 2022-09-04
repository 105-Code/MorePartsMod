using Cysharp.Threading.Tasks;
using HarmonyLib;
using MorePartsMod.Buildings;
using MorePartsMod.Managers;
using MorePartsMod.Parts;
using SFS;
using SFS.Parts;
using SFS.Builds;
using SFS.Parts.Modules;
using SFS.UI;
using SFS.UI.ModGUI;
using SFS.World;
using SFS.World.Maps;
using SFS.WorldBase;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MorePartsMod.Buildings.ColonyComponent;
using MorePartsMod.UI;
using SFS.Input;
using SFS.Translations;

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

    [HarmonyPatch(typeof(RocketManager), "SpawnBlueprint")]
    class RocketManagerPatcher
    {
        private static Planet colonyPlanet;

        private static Location GetSpawnLocation(JointGroup group)
        {
            Vector2 vector = Vector2.zero;
            float num = 0f;
            foreach (Part part in group.parts)
            {

                vector += (Vector2) part.transform.TransformPoint(part.centerOfMass.Value) * part.mass.Value;
                num += part.mass.Value;
            }
            vector /= num;
            return new Location(colonyPlanet, WorldView.ToGlobalPosition(vector), default(Double2));
        }

        private static Rocket CreateRocket(JointGroup jointGroup, string rocketName, bool throttleOn, float throttlePercent, bool RCS, float rotation, float angularVelocity, Func<Rocket, Location> location, bool physicsMode)
        {
            Rocket prefab = Traverse.Create(typeof(RocketManager)).Field("prefab").GetValue() as Rocket;

            Rocket rocket = UnityEngine.Object.Instantiate<Rocket>(prefab);
            rocket.rocketName = rocketName;
            rocket.throttle.throttleOn.Value = throttleOn;
            rocket.throttle.throttlePercent.Value = throttlePercent;
            rocket.arrowkeys.rcs.Value = RCS;
            rocket.SetJointGroup(jointGroup);
            rocket.rb2d.transform.eulerAngles = new Vector3(0f, 0f, rotation);
            rocket.rb2d.angularVelocity = angularVelocity;
            rocket.physics.SetLocationAndState(location(rocket), physicsMode);
            return rocket;
        }

        private static Rocket[] SpawnRockets(List<JointGroup> groups)
        {
            List<Rocket> list = new List<Rocket>();
            foreach (JointGroup jointGroup in groups)
            {
                Location location = GetSpawnLocation(jointGroup);
                Rocket rocket = CreateRocket(jointGroup, "", false, 0.5f, false, 0f, 0f, (Rocket a) => location, false);
                list.Add(rocket);
                rocket.stats.Load(-1);
            }
            return list.ToArray();

        }

        [HarmonyPrefix]
        public static bool Prefix( ref Blueprint blueprint)
        {
            ColonyData target = MorePartsMod.Main.spawnPoint;

            if(target == null)
            {
                return true;
            }

            colonyPlanet = target.getPlanet();
            Location colonyLocation = new Location(colonyPlanet, target.position);
            WorldView.main.SetViewLocation(colonyLocation);
            blueprint.rotation = target.angle;
            if (blueprint.rotation != 0f)
            {
                foreach (PartSave partSave in blueprint.parts)
                {
                    partSave.orientation += new Orientation(1f, 1f, blueprint.rotation);
                    partSave.position *= new Orientation(1f, 1f, blueprint.rotation);
                }
            }
            
            OwnershipState[] array2;
            Part[] array = PartsLoader.CreateParts(blueprint.parts, null, null, OnPartNotOwned.Delete, out array2);
            Part[] array3 = (from a in array
                             where a != null
                             select a).ToArray<Part>();
            if (blueprint.rotation != 0f)
            {
                foreach (PartSave partSave2 in blueprint.parts)
                {
                    partSave2.orientation += new Orientation(1f, 1f, -blueprint.rotation);
                    partSave2.position *= new Orientation(1f, 1f, -blueprint.rotation);
                }
            }
            blueprint.rotation = 0;
            Part_Utility.PositionParts(WorldView.ToLocalPosition(target.getBuildingPosition("Launch pad", 3)), new Vector2(0.5f, 0f), true, true, array3);
            List<JointGroup> groups;
            new JointGroup(RocketManager.GenerateJoints(array3), array3.ToList<Part>()).RecreateGroups(out groups);
            Rocket[] array4 = SpawnRockets(groups);
            Staging.CreateStages(blueprint, array);
            Rocket rocket = array4.FirstOrDefault((Rocket a) => a.hasControl.Value);
            PlayerController.main.player.Value = ((rocket != null) ? rocket : ((array4.Length != 0) ? array4[0] : null));
            MorePartsMod.Main.spawnPoint = null;

            return false; 
        }

    }

    [HarmonyPatch(typeof(BuildManager), "Start")]
    class BuildManagerPatcherStart
    {

        private static BuildingColonyGUI _ui;

        [HarmonyPostfix]
        public static void Postfix(BuildManager __instance)
        {
            GameObject ui = GameObject.Find("--- UI ---");
            Transform topMenu = ui.transform.Find("Top Right");

            GameObject holder = new GameObject("Colony Menu");
            holder.transform.localScale = new Vector3(0.9f, 0.9f);
            Builder.AttachToCanvas(holder, Builder.SceneToAttach.CurrentScene);
            _ui = holder.AddComponent<BuildingColonyGUI>();

            SFS.UI.ModGUI.Button button =  Builder.CreateButton(ui.transform, 120, 50, (int) topMenu.localPosition.x + 400, (int) topMenu.localPosition.y + 180, () => ScreenManager.main.OpenScreen(() => _ui), "Colonies");
        }

    }

    [HarmonyPatch(typeof(BuildManager), "Launch")]
    class BuildManagerPatcherLaunch
    {

        [HarmonyPrefix]
        public static bool Prefix(BuildManager __instance)
        {
            if (MorePartsMod.Main.spawnPoint == null)
            {
                return true;
            }

            // spawn point selected
            if (__instance.buildMenus.statsDrawer.mass > MorePartsMod.Main.spawnPoint.rocketParts)
            {
                ShowMenu("Insufficient Resource on " + MorePartsMod.Main.spawnPoint.name,"Ok");
                return false;
            }

            ResourceModule[] resources = __instance.buildGrid.activeGrid.partsHolder.GetModules<ResourceModule>();
            foreach (ResourceModule resource in resources)
            {
                if (!validResource(resource.resourceType.name))
                {
                    ShowMenu("You can't transport " + resource.resourceType.name, "Ok");
                    return false;
                }
            }

            return true;
        }

        private static void ShowMenu(string text,string option)
        {
            SizeSyncerBuilder.Carrier sizeSync;
            ButtonBuilder[] array = new ButtonBuilder[1];
            new SizeSyncerBuilder(out sizeSync).HorizontalMode(SizeMode.MaxChildSize);
            array[0] = ButtonBuilder.CreateButton(sizeSync, () => option, null, CloseMode.Stack);
            MenuGenerator.ShowChoices(() => text, array);
        }

        private static bool validResource(string resourceName)
        {
            if(resourceName == "Rocket_Material")
            {
                return false;
            }

            if (resourceName == "Electronic_Component")
            {
                return false;
            }

            if (resourceName == "Construction_Material")
            {
                return false;
            }

            return true;
        }
    }
}
