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
    public class MapUtils
    {
        public static void DrawLandmarkInPlanet(Planet planet, float degreeAngle, Double2 location, string text, Color color)
        {
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

    }
}
