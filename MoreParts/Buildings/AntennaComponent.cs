using SFS.World;
using UnityEngine;
using MorePartsMod.ARPA;
using MorePartsMod.Parts;

namespace MorePartsMod.Buildings
{
    class AntennaComponent : MonoBehaviour
	{
		public static AntennaComponent main;

		private ARPANET _network;
		private Node _routeOrigin;
		

		private void Awake()
		{
			main = this;
			WorldLocation worldLocation = this.GetComponent<WorldLocation>();
			this._network = new ARPANET(worldLocation);
		}


		public Node AddNode(TelecommunicationDishModule dish)
		{
			WorldLocation location = dish.Rocket.GetComponent<WorldLocation>();
			return this._network.Insert(location);
		}

		public void RemoveNode(TelecommunicationDishModule dish)
		{
			this._network.Remove(dish.Node);
			this._network.ClearRoute(this._routeOrigin);
		}

		public bool IsConnected(Node origin)
		{
			
			bool result;
			
			if (origin.next != null)
			{
				result = this._network.CheckRoute(origin);
				if (result)
				{
					return true;
				}
				this._network.ClearRoute(origin);
			}
			origin.mark = true;
			result = this._network.IsConnected(origin);
			if (result)
			{
				this._routeOrigin = origin;
			}
			this._network.ClearMarks();
			return result;
		}

	}
}
