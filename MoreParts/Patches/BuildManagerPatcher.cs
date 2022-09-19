using HarmonyLib;
using MorePartsMod.UI;
using SFS;
using SFS.Builds;
using SFS.Input;
using SFS.Parts.Modules;
using SFS.UI;
using SFS.UI.ModGUI;
using UnityEngine;
namespace MorePartsMod.Patches
{
    [HarmonyPatch(typeof(BuildManager), "Start")]
    class BuildManagerStart
    {
        private static BuildingColonyGUI _ui;

        [HarmonyPostfix]
        public static void Postfix()
        {
            GameObject ui = GameObject.Find("--- UI ---");
            Transform topMenu = ui.transform.Find("Top Right");

            GameObject holder = new GameObject("Colony Menu");
            holder.transform.localScale = new Vector3(0.9f, 0.9f);
            _ui = holder.AddComponent<BuildingColonyGUI>();
            Builder.AttachToCanvas(holder, Builder.SceneToAttach.CurrentScene);

            SFS.UI.ModGUI.Button button = Builder.CreateButton(ui.transform, 120, 40, -90,-95, () => ScreenManager.main.OpenScreen(() => _ui), "Colonies");
            button.rectTransform.anchorMin = Vector2.one;
            button.rectTransform.anchorMax = Vector2.one;
        }

    }

    [HarmonyPatch(typeof(BuildManager), "Launch")]
    class BuildManagerLaunch
    {
        public static double RocketMass;

        [HarmonyPrefix]
        public static bool Prefix(BuildManager __instance)
        {
            if (MorePartsModMain.Main.spawnPoint == null)
            {
                return true;
            }

            double resourcesWeight = 0;
            ResourceModule[] resources = __instance.buildGrid.activeGrid.partsHolder.GetModules<ResourceModule>();
            foreach (ResourceModule resource in resources)
            {
                if (!validResource(resource.resourceType.name))
                {
                    ShowMenu("You can't transport " + resource.resourceType.name, "Ok");
                    return false;
                }
                
                resourcesWeight += resource.wetMass.Value - (resource.wetMass.Value * resource.dryMassPercent.Value);
            }
            RocketMass = __instance.buildMenus.statsDrawer.mass - resourcesWeight;

            if (RocketMass > MorePartsModMain.Main.spawnPoint.rocketParts)
            {
                ShowMenu("Insufficient rocket material on " + MorePartsModMain.Main.spawnPoint.name, "Ok");
                return false;
            }

            return true;
        }

        private static void ShowMenu(string text, string option)
        {
            SizeSyncerBuilder.Carrier sizeSync;
            ButtonBuilder[] array = new ButtonBuilder[1];
            new SizeSyncerBuilder(out sizeSync).HorizontalMode(SizeMode.MaxChildSize);
            array[0] = ButtonBuilder.CreateButton(sizeSync, () => option, null, CloseMode.Stack);
            MenuGenerator.ShowChoices(() => text, array);
        }

        private static bool validResource(string resourceName)
        {
            if (resourceName == "Rocket_Material")
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
