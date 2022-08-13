using SFS.World;
using UnityEngine;
using MorePartsMod.ARPA;
using MorePartsMod.Parts;
using System;

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

		private void Start()
		{
			KeySettings.AddOnKeyDown_World(KeySettings.Main.Toggle_Telecommunication_Dish, this.ToggleRocketAntenna);
		}

		private void ToggleRocketAntenna()
		{
			Rocket rocket = PlayerController.main.player.Value as Rocket;
			TelecommunicationDishModule[] antennas =rocket.partHolder.GetModules<TelecommunicationDishModule>();
			if(antennas.Length > 0)
			{
				antennas[0]._toggle();
			}
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
			try
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
			catch (Exception error)
			{
				Debug.Log("Errror in isConnected");
				Debug.LogError(error);
				return false;
			}
		}

	}
}
