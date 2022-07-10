using SFS.World;
using UnityEngine;
using MorePartsMod.ARPA;
using System.Collections.Generic;

namespace MorePartsMod.Buildings
{
    class ARPANETModule : MonoBehaviour

	{
		public static GameObject Antenna;
		public static WorldLocation WorldLocation;
		public static ARPANETModule Main;
		public static List<PlanetModule> planets = new List<PlanetModule>();

		public ARPANET network;

		private void Awake()
		{
			Main = this;
			Debug.Log("Creating network");
			this.network = new ARPANET(WorldLocation);
			Debug.Log("ARPANET ready!");
		}

		public static PlanetModule getPlanet(string name)
		{
			foreach(PlanetModule planet in planets)
			{
				if(planet.planet.name == name)
				{
					return planet;
				}
			}
			return null;
		}
	}
}
