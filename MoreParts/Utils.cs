using SFS.World;
using SFS.World.Maps;
using SFS.WorldBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MorePartsMod
{
    public class Utils
    {
        // Adds a building to the world at a specified location
        public static void AddBuildingToWorld(GameObject building,string holder,Planet planet,Double2 position)
        {
            WorldLocation location = building.AddComponent<WorldLocation>();

            WorldLoader worldLoader = building.AddComponent<WorldLoader>();
            worldLoader.location = location; // where is the antenna placed
            worldLoader.loadDistance = 8000; // max render distance
            worldLoader.holder = building.transform.FindChild(holder).gameObject;

            StaticWorldObject buildingObject = building.AddComponent<StaticWorldObject>();
            buildingObject.location = location;

            // set the antenna in the world
            buildingObject.location.Value = new Location(planet, position, default(Double2));
        }
        // Creates a landmark on a planet's map
        public static void DrawLandmarkInPlanet(Planet planet,float degreeAngle, Double2 location,string text,Color color)
        {
            // Calculate the fade in and fade out values for the landmark
            
            //float angle = degreeAngle * Mathf.Deg2Rad;
            double num = planet.data.basics.radius * 6;
            float num2 = Mathf.Min(MapDrawer.GetFadeIn(Map.view.view.distance, num * 0.5, num * 0.4), MapDrawer.GetFadeOut(Map.view.view.distance, 20000.0, 15000.0));
            if (num2 > 0f)
            {
                color.a = num2;
                Vector2 normal = Double2.CosSin(0.017453292f * (degreeAngle + 6));
                float x = (float)(planet.mapHolder.position.x + (location.x / 1000));
                float y = (float)(planet.mapHolder.position.y + (location.y / 1000));
                Vector2 position = new Vector2(x, y);

                MapDrawer.DrawPointWithText(20, color, text, 40, color, position, normal, 4, 4);
            }
        }

        // No idea what this does
        public static bool LineIntersectCircle(double radius, Double2 circleCenter, Double2 pointA, Double2 pointB)
        {
            double m = (pointB.y - pointA.y) / (pointB.x - pointA.x);
            double aux = (-m * pointA.x + pointA.y - circleCenter.y);

            double a = 1 + m * m;
            double b = 2 * m * aux - 2 * circleCenter.x;
            double c = circleCenter.x * circleCenter.x + aux * aux - radius * radius;
            double result = b * b - 4 * a * c;
            if (result < 0)
            {
                // there is not a collision
                return false;
            }

            //detect if pointB is front of Circle Center
            float PointAToCircleCenter = Vector2.Distance(pointA, circleCenter);
            float PointAToPointB = Vector2.Distance(pointA, pointB);
            if (PointAToPointB < PointAToCircleCenter)
            {
                return false;
            }

            Vector2 origintPlanetDirection = (pointA.ToVector2 - circleCenter.ToVector2).normalized;
            Vector2 origintTargetDirection = (pointA.ToVector2 - pointB.ToVector2).normalized;
            //detect if the collision is behind pointA
            if (Vector2.Dot(origintPlanetDirection, origintTargetDirection) < 0)
            {
                return false;
            }

            return true;
        }

    }
}
