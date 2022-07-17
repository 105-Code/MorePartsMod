using MorePartsMod.Buildings;
using SFS.World;
using SFS.WorldBase;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MorePartsMod.ARPA
{
    class Node
    {
        private WorldLocation _worldLocation;
        private bool _isOrigin;
        private int _id;

        public Node next;
        public bool mark;

        public int Id { get => this._id; }
        public bool IsOrigin { get => this._isOrigin; }
        public WorldLocation WorlLocation {  get => this._worldLocation; }
      


        public Node(int id, WorldLocation worldLocation, bool isOrigin =false)
        {
            this._id = id;
            this.mark = false;
            this.next = null;
            this._worldLocation = worldLocation;
            this._isOrigin = isOrigin;
        }

        public bool IsAvailableTo(Node target)
        {
            Planet planet1 = this.WorlLocation.planet.Value;
            Planet planet2 = target.WorlLocation.planet.Value;
            if (this.HitPlanet(planet1.Radius, planet1.GetSolarSystemPosition(), this.GetAbsolutePosition(), target.GetAbsolutePosition()))
            {
                return false;
            }

            if(planet1.codeName == planet2.codeName)
            {
                return true;
            }

            if (this.HitPlanet(planet2.Radius, planet2.GetSolarSystemPosition(), this.GetAbsolutePosition(), target.GetAbsolutePosition()))
            {
                return false;
            }
            return true;
        }

        private bool HitPlanet(double planetRadius, Double2 planetCenter, Double2 origin, Double2 target)
        {
            double m = (target.y - origin.y) / (target.x - origin.x);
            double aux = (-m * origin.x + origin.y - planetCenter.y);

            double a = 1 + m * m;
            double b = 2 * m * aux - 2 * planetCenter.x;
            double c = planetCenter.x * planetCenter.x + aux * aux - planetRadius * planetRadius;
            double result = b * b - 4 * a * c;
            if(result < 0)
            {
                return false;
            }

            // el error esta aquí
            float originToPlanet = Vector2.Distance(origin, planetCenter);
            float originToTarget = Vector2.Distance(origin, target);
            if (originToTarget < originToPlanet)
            {
                return false;
            }

            Vector2 origintPlanetDirection = (origin.ToVector2 - planetCenter.ToVector2).normalized;
            Vector2 origintTargetDirection = (origin.ToVector2 - target.ToVector2).normalized;
            
            if(Vector2.Dot(origintPlanetDirection, origintTargetDirection) < 0)
            {
                return false;
            }

            return true;
        }

        public Double2 GetAbsolutePosition()
        {       
            return this._worldLocation.planet.Value.GetSolarSystemPosition() + this._worldLocation.Value.position;
        }

    }
}
