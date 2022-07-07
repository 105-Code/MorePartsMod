using SFS.World;
using UnityEngine;
using MorePartsMod.ARPA;

namespace MorePartsMod.Buildings
{
    class ARPANETModule : MonoBehaviour

	{
		public static GameObject Antenna;
		public static WorldLocation WorldLocation;
		public static ARPANETModule Main;

		public ARPANET network;

		private void Awake()
		{
			Main = this;
			Debug.Log("Creating network");
			this.network = new ARPANET(WorldLocation);
			Debug.Log("ARPANET ready!");
		}


	}
}
