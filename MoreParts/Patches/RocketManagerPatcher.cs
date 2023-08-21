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
        private static Planet colonyPlanet;

        private static Location GetSpawnLocation(JointGroup group)
        {
            Vector2 vector = Vector2.zero;
            float num = 0f;
            foreach (Part part in group.parts)
            {

                vector += (Vector2)part.transform.TransformPoint(part.centerOfMass.Value) * part.mass.Value;
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

        private static void ClearLaunchPad(Double2 point)
        {
            List<Rocket> rockets = GameManager.main.rockets.Where(rocket => Vector2.Distance(rocket.location.position.Value, point) < 50).ToList();

            foreach (Rocket rocket in rockets)
            {
                RocketManager.DestroyRocket(rocket, DestructionReason.Intentional);
            }
        }

        private static double GetHeight(Part[] parts)
        {
            Rect rect;
            return (double)(Part_Utility.GetBuildColliderBounds_WorldSpace(out rect, true, parts) ? rect.height + 3 : 3f);
        }

        [HarmonyPrefix]
        public static bool Prefix(ref Blueprint blueprint)
        {
            ColonyData target = MorePartsPack.Main.SpawnPoint;

            if (target == null)
            {
                return true;
            }
            colonyPlanet = target.getPlanet();
            Location colonyLocation = new Location(colonyPlanet, target.position);
            WorldView.main.SetViewLocation(colonyLocation);

            Map.view.view.target.Value = colonyPlanet.mapPlanet;
            Map.view.view.distance.Value = colonyPlanet.mapPlanet.FocusDistance * 0.5;

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

            Double2 spawnPoint = target.getBuildingPosition("Launch Pad", (int)GetHeight(array3));
            ClearLaunchPad(spawnPoint);
            Part_Utility.PositionParts(WorldView.ToLocalPosition(spawnPoint), new Vector2(0.5f, 0f), true, true, array3);
            List<JointGroup> groups;
            new JointGroup(RocketManager.GenerateJoints(array3), array3.ToList<Part>()).RecreateGroups(out groups);
            Rocket[] array4 = SpawnRockets(groups);
            Staging.CreateStages(blueprint.stages, array);
            Rocket rocket = array4.FirstOrDefault((Rocket a) => a.hasControl.Value);
            PlayerController.main.player.Value = ((rocket != null) ? rocket : ((array4.Length != 0) ? array4[0] : null));
            MorePartsPack.Main.SpawnPoint.takeResource(MorePartsTypes.ROCKET_MATERIAL, BuildManagerLaunch.RocketMass);
            MorePartsPack.Main.SpawnPoint = null;
            return false;
        }
    }
}
