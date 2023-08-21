using SFS.World;
using SFS.World.Maps;
using SFS.WorldBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MorePartsMod.Utils
{
    public class MathUtils
    {
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
