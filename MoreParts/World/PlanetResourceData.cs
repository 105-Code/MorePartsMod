using System;
using SFS.WorldBase;
using System.Collections.Generic;
using UnityEngine;

namespace MorePartsMod.World
{
    public class PlanetResourceData
    {
        [NonSerialized]
        private Planet _planet;

        public List<ResourceDeposit> ResourceDeposits;

        public PlanetResourceData(Planet planet)
        {
            this._planet = planet;
            this.ResourceDeposits = new List<ResourceDeposit>();
        }

        public PlanetResourceData() { }

        public Planet GetPlanet()
        {
            return this._planet;
        }

        public void Initialize()
        {
            System.Random rnd = new System.Random();
            int currentAngle = rnd.Next(0,10);
            // create x deposits
            for (short index = 0; index < 9; index++)
            {
                int amount = rnd.Next(1000,3000);
                float radians = currentAngle * Mathf.Deg2Rad;
                double magnitude = this._planet.GetTerrainHeightAtAngle(radians, false);
                float x = Mathf.Cos(radians);
                float y = Mathf.Sin(radians);
                Double2 position = new Double2(x, y)*(magnitude+this._planet.Radius);

                ResourceDeposit data = new ResourceDeposit(amount, position, (int) (amount * 0.2), currentAngle);
                ResourceDeposits.Add(data);
                currentAngle += rnd.Next(20, 36);
            }

        }

        public class ResourceDeposit
        {
            public double Amount;
            public bool Active;
            public Double2 Location;
            public bool Discovered;
            public int Size;
            public float AngleDegree;

            public ResourceDeposit(float amount, Double2 location, int size, float angleDegree)
            {
                this.Amount = amount;
                this.Active = true;
                this.Location = location;
                this.Discovered = false;
                this.Size = size;
                this.AngleDegree = angleDegree;
            }

            public ResourceDeposit() {}

            /**
             * reduce resources in this deposit
             * <returns> True if there are more resources</returns>
             */
            public bool takeResources(double quantity)
            {
                this.Amount -= quantity;
                if(this.Amount < 0)
                {
                    this.Active = false;
                    return false;
                }
                return true;
            }
        }

    }
}
