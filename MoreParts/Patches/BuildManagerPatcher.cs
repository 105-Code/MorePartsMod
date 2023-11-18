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

        [HarmonyPrefix]
        public static bool Prefix(BuildManager __instance)
        {
            if (MorePartsPack.Main.SpawnPoint == null)
            {
                return true;
            }

            double resourcesWeight = 0;
            ResourceModule[] resources = __instance.buildGrid.activeGrid.partsHolder.GetModules<ResourceModule>();
            foreach (ResourceModule resource in resources)
            {
                if (IsSpeacialResource(resource.resourceType.name))
                {
                    resource.resourcePercent.Value = 0;
                }

                if(resource.resourceType.resourceMass > 0)
                {
                    resourcesWeight += (resource.wetMass.Value - (resource.wetMass.Value * resource.dryMassPercent.Value)) * resource.resourcePercent.Value;
                }
            }

            double rocketMass = __instance.buildMenus.statsDrawer.mass - resourcesWeight;

            if (rocketMass > MorePartsPack.Main.SpawnPoint.GetResource(MorePartsTypes.ROCKET_MATERIAL))
            {
                ShowMenu("Insufficient rocket material on " + MorePartsPack.Main.SpawnPoint.name, "Ok");
                return false;
            }

            MorePartsPack.Main.SpawnPoint.TakeResource(MorePartsTypes.ROCKET_MATERIAL, rocketMass);

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

        private static bool IsSpeacialResource(string resourceName)
        {
            if (resourceName == MorePartsTypes.ROCKET_MATERIAL)
            {
                return true;
            }

            if (resourceName == MorePartsTypes.ELECTRONIC_COMPONENT)
            {
                return true;
            }

            if (resourceName == MorePartsTypes.CONSTRUCTION_MATERIAL)
            {
                return true;
            }

            if (resourceName == MorePartsTypes.MATERIAL)
            {
                return true;
            }

            return false;
        }
    }


}
