using HarmonyLib;
using MorePartsMod.Buildings;
using SFS;
using SFS.World;
using SFS.WorldBase;
using UnityEngine;

namespace MorePartsMod.Patches
{
    [HarmonyPatch(typeof(SpaceCenter), "Start")]
    class SpaceCenterPatcher
    {
        [HarmonyPrefix]
        public static void Prefix(SpaceCenter __instance)
        {
            SpaceCenterData spaceCenter = Base.planetLoader.spaceCenter;

            GameObject gameObject = GameObject.Instantiate(MorePartsModMain.Main.Assets.LoadAsset<GameObject>("Telecommunication Antenna"));
            gameObject.transform.parent = __instance.transform;
            gameObject.SetActive(true);

            Utils.AddBuildingToWorld(gameObject, "Model", spaceCenter.address.GetPlanet(), spaceCenter.LaunchPadLocation.position + new Double2(-150.0, -10.0));

            StaticWorldObject buildingObject = gameObject.GetComponent<StaticWorldObject>();
            WorldLoader worldLoader = gameObject.GetComponent<WorldLoader>();
            SpaceCenter.Building building = new SpaceCenter.Building();
            building.building = buildingObject;

            __instance.buildings.meshRenderers.AddItem(worldLoader.holder.GetComponent<MeshRenderer>());
            gameObject.AddComponent<AntennaComponent>();
        }
    }
}
