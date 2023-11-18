using HarmonyLib;
using MorePartsMod.Buildings;
using SFS.Builds;
using SFS.Parts;
using SFS.Parts.Modules;
using SFS.World;
using SFS.World.Maps;
using SFS.WorldBase;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MorePartsMod.Patches
{
    [HarmonyPatch(typeof(RocketManager), "SpawnBlueprint")]
    class RocketManagerPatcher
    {
        private static void ClearLaunchPad(Double2 point)
        {
            List<Rocket> rockets = GameManager.main.rockets.Where(rocket => Vector2.Distance(rocket.location.position.Value, point) < 50).ToList();

            foreach (Rocket rocket in rockets)
            {
                RocketManager.DestroyRocket(rocket, DestructionReason.Intentional);
            }
        }

        public static void PlanetSurface(Planet planet, Double2 position, double angle)
        {
            Location location = new Location(0, planet, position, new Double2(0, 0));

            int indexOf = GameManager.main.rockets.IndexOf(PlayerController.main.player.Value as Rocket);
            PlayerController.main.player.Value = new Rocket();

            var nullLocation = new Location(0, location.planet, new Double2(0, 0), new Double2(0, 0));

            GameManager.main.rockets[indexOf].physics.SetLocationAndState(nullLocation, false);

            GameManager.main.rockets[indexOf].physics.SetLocationAndState(location, false);
            PlayerController.main.player.Value = GameManager.main.rockets[indexOf];

            Rocket rocket = PlayerController.main.player.Value as Rocket;
            if (rocket == null) return;
            rocket.EnableCollisionImmunity(6);
            rocket.partHolder.transform.eulerAngles = new Vector3(0, 0, (float)location.position.AngleDegrees - 90);
        }

        private static void SetMap(ColonyData targetColony)
        {
            Planet targetPlanet = targetColony.GetPlanet();
            WorldView.main.SetViewLocation(targetColony.GetLocation());
            Map.view.view.target.Value = targetPlanet.mapPlanet;
            Map.view.view.distance.Value = targetPlanet.mapPlanet.FocusDistance * 0.5;
        }

        [HarmonyPostfix]
        public static void Postfix(ref Blueprint blueprint)
        {
            ColonyData targetColony = MorePartsPack.Main.SpawnPoint;

            if (targetColony == null)
            {
                return;
            }

            SetMap(targetColony);
            // clear near rockets
            Rocket rocket = PlayerController.main.player.Value as Rocket;
            Rect bounds;
            float height = Part_Utility.GetBuildColliderBounds_WorldSpace(out bounds, useLaunchBounds: true, rocket.partHolder.GetArray()) ? bounds.height : 0f;
            Building building = targetColony.GetBuilding(MorePartsTypes.LAUNCH_PAD_BUILDING);
            Double2 spawnPoint = building.position.normalized * (building.position.magnitude + (height/2) + 5);
            ClearLaunchPad(spawnPoint);

            //move rocket to target colony
            PlanetSurface(targetColony.GetPlanet(), spawnPoint, targetColony.angle);

            //reduce colony resource
            MorePartsPack.Main.SpawnPoint = null;
        }
    }
}
