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

        public List<ReourceDeposit> ResourceDeposits;

        public PlanetResourceData(Planet planet)
        {
            this._planet = planet;
            this.ResourceDeposits = new List<ReourceDeposit>();
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
                double magnitude = this._planet.GetTerrainHeightAtAngle(radians);
                float x = Mathf.Cos(radians);
                float y = Mathf.Sin(radians);
                Double2 position = new Double2(x, y)*(magnitude+this._planet.Radius);
                //Debug.Log("Current:" + currentAngle + " Radians" + radians + " x:" + x + " y" + y + " position:" + position.ToString());

                ReourceDeposit data = new ReourceDeposit(amount, position, (int) (amount * 0.2), currentAngle);
                ResourceDeposits.Add(data);
                currentAngle += rnd.Next(20, 36);
            }

        }

        public class ReourceDeposit
        {
            public int Amount;
            public bool Active;
            public Double2 Location;
            public bool Discovered;
            public int Size;
            public float AngleDegree;

            public ReourceDeposit(int amount, Double2 location, int size, float angleDegree)
            {
                this.Amount = amount;
                this.Active = true;
                this.Location = location;
                this.Discovered = false;
                this.Size = size;
                this.AngleDegree = angleDegree;
            }

            public ReourceDeposit()
            {

            }
        }

    }
}
