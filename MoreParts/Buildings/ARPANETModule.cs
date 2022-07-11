using SFS.World;
using UnityEngine;
using MorePartsMod.ARPA;
using System.Collections.Generic;
using MorePartsMod.Parts;

namespace MorePartsMod.Buildings
{
    class ARPANETModule : MonoBehaviour
	{
		public static ARPANETModule Main;

		private ARPANET _network;
		private Node _routeOrigin;
		

		private void Awake()
		{
			Main = this;
			WorldLocation worldLocation = this.GetComponent<WorldLocation>();
			this._network = new ARPANET(worldLocation);
		}


		public Node addNode(TelecommunicationDishModule dish)
		{
			WorldLocation location = dish.Rocket.GetComponent<WorldLocation>();
			return this._network.insert(location);
		}

		public void removeNode(TelecommunicationDishModule dish)
		{
			this._network.remove(dish.Node);
			this._network.clearRoute(this._routeOrigin);
		}

		public bool isConnected(Node origin)
		{
			
			bool result;
			
			if (origin.next != null)
			{
				result = this._network.checkRoute(origin);
				if (result)
				{
					return true;
				}
				this._network.clearRoute(origin);
			}
			origin.mark = true;
			result = this._network.isConnected(origin);
			if (result)
			{
				this._routeOrigin = origin;
			}
			this._network.clearMarks();
			return result;
		}

	}
}
