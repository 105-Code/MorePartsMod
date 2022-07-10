using HarmonyLib;
using MorePartsMod.Buildings;
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
	
    [HarmonyPatch(typeof(SpaceCenter), "Start")]
    class AntennaPatcher
    {
 
        [HarmonyPrefix]
        public static void Prefix(SpaceCenter __instance)
        {
            Debug.Log("Creating Antenna");
            // get space center
            SpaceCenterData spaceCenter = Base.planetLoader.spaceCenter;
            Planet planet = spaceCenter.address.GetPlanet();


            // get and instance Antenna prefab
            GameObject gameObject = GameObject.Instantiate(MorePartsMod.Main.Assets.LoadAsset<GameObject>("Telecommunication Antenna"));
            gameObject.transform.parent = __instance.transform;
            gameObject.SetActive(true);

            // add WorldLocation component
            WorldLocation location = gameObject.AddComponent<WorldLocation>();// this values is set again in Patcher

            // add WorldLoader component
            WorldLoader worldLoader = gameObject.AddComponent<WorldLoader>();
            worldLoader.location = location; // where is the antenna placed
            worldLoader.loadDistance = 8000; // max render distance
            
            //get Antenna model
            worldLoader.holder = gameObject.transform.FindChild("Model").gameObject;// get de antenna model
                 
            // add StaticWorldObject component
            StaticWorldObject buildingObject = gameObject.AddComponent<StaticWorldObject>();
            buildingObject.location = location;

            // Create Space center building
            SpaceCenter.Building building = new SpaceCenter.Building();
            building.building = buildingObject;
            ARPANETModule.Antenna = gameObject;// and Antenna gameobjecto to ARPANET module
            ARPANETModule.WorldLocation = location;

            // set the antenna in the world
            building.building.location.Value = new Location(planet, spaceCenter.LaunchPadLocation.position + new Double2(-150.0, -10.0), default(Double2));

            // add mesh to ModelSetup. This will render the Antenna mesh
            __instance.buildings.meshRenderers.AddItem(worldLoader.holder.GetComponent<MeshRenderer>());

            // add ARPANETModule
            gameObject.AddComponent<ARPANETModule>();
        }
    }

    [HarmonyPatch(typeof(GameManager), "Start")]
    class GameManagerPatcher
    {

        [HarmonyPostfix]
        public static void Postfix(GameManager __instance)
        {
            
            
            foreach(SFS.World.Environment env in __instance.environment.environments)
            {
                PlanetModule planet = env.holder.GetOrAddComponent<PlanetModule>();
                planet.gameobject = env.holder.gameObject;
                planet.transform = env.holder;
                planet.collider = env.holder.GetOrAddComponent<CircleCollider2D>();
                planet.collider.isTrigger = true;
                Debug.Log(env.planet.name + ":" + (float)env.planet.Radius+" Pos:"+env.holder.transform.position);
                planet.collider.radius = (float) env.planet.Radius;
                planet.planet = env.planet;
                ARPANETModule.planets.Add(planet);
            }

        }
            
    }

    public class PlanetModule : MonoBehaviour
    {
        public GameObject gameobject;
        public Transform transform;
        public CircleCollider2D collider;
        public Planet planet;

    }

}
