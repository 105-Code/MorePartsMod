using MorePartsMod.ARPA;
using MorePartsMod.Buildings;
using SFS;
using SFS.Parts;
using SFS.UI;
using SFS.Variables;
using SFS.World;
using SFS.World.Maps;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SFS.World.Rocket;

namespace MorePartsMod.Parts
{
    class TelecommunicationDishModule: MonoBehaviour, INJ_Rocket, INJ_IsPlayer
	{
		private Part _part;
		private VariableList<bool>.Variable _state;
		private Node _rocketNode;
		private Node _nearNode;
		private Rocket _rocket;
		private bool _active;
		private bool _notifyDisconection;
		private bool _notifyConnection;
		private const float _pingTime = 5f;
		public bool State { get => this._state.Value; }

		public Rocket Rocket { set => this._rocket = value; }
		public bool IsPlayer { 
			set {
				if (!value) // is not player
				{
					this._rocket.hasControl.Value = true;
					if (this._active)
					{
						this.CancelInvoke("checkConnection");
					}
					return;
				}

				// is player 
				if (this._active)
				{
					this._rocket.hasControl.Value = false;
					this.stateChange();
				}

			}
		}

		private void Awake()
		{
			this._part = this.GetComponent<Part>();
			this._state = this._part.variablesModule.boolVariables.GetVariable("isOn");
			this._part.onPartUsed.AddListener(this.Toggle);
			this._notifyConnection = true;
			this._notifyDisconection = true;

		}

		private void checkConnection()
		{
			if (this._rocketNode == null)
			{
				this._rocket.hasControl.Value = false;
			}


			if (this._nearNode == null)
			{
				this._nearNode = ARPANETModule.Main.network.getClosestNode(this._rocketNode);
			}
			else if (!this._nearNode.isNear(this._rocketNode.Position))
			{
				this._nearNode = ARPANETModule.Main.network.getClosestNode(this._rocketNode);
			}

			if (_nearNode == null || !ARPANETModule.Main.network.isConnected(this._nearNode))
			{
				if (this._notifyDisconection)
				{
					MsgDrawer.main.Log("No Connection");
					this._notifyDisconection = false;
					this._notifyConnection = true;
				}
				
				this._rocket.hasControl.Value = false;
				
				return;
			}

			if (this._notifyConnection)
			{
				MsgDrawer.main.Log("Connected");
				this._notifyConnection = false;
				this._notifyDisconection = true;

			}
			this._rocket.hasControl.Value = true;
		}

		private void Start()
		{
			if (GameManager.main == null)
			{
				base.enabled = false;
				return;
			}
			KeySettings.AddOnKeyDown_World(KeySettings.Main.Toggle_Telecommunication_Dish,this._toggle);

			if (this._state.Value)
			{
				// conect to ARPANET
				this._rocketNode = ARPANETModule.Main.network.insertNode(this._rocket.GetComponent<WorldLocation>());
				this.stateChange();

			}
			else
			{
				if (this._rocket.isPlayer)
				{
					this._rocket.hasControl.Value = false;
				}
				
			}
			
			this._state.onValueChange += this.stateChange;
		
		}


		private void stateChange()
		{
			Debug.Log("State Change");
			//this._active = this._state.Value;
			if (this._state.Value)
			{
				this.InvokeRepeating("checkConnection",1f,_pingTime);
				this._active = true;
			}
			else
			{
				this.CancelInvoke("checkConnection");
				this._active = false;
			}
		}

		private void _toggle()
		{
			if (!this._rocket.isPlayer)
			{
				return;
			}

			if (this._state.Value)
			{
				ARPANETModule.Main.network.destroyNode(this._rocketNode);
				MsgDrawer.main.Log("Telecommunication Dish Off");
				this._rocketNode = null;
			}
			else
			{
				this._rocketNode = ARPANETModule.Main.network.insertNode(this._rocket.GetComponent<WorldLocation>());
				MsgDrawer.main.Log("Telecommunication Dish On");
			}

			this._state.Value = !this._state.Value;
		}

		private void Toggle(UsePartData data)
		{
			this._toggle();
			data.successfullyUsedPart = true;
		}

		public static void Setup()
        {
			Part part;
			Base.partsLoader.parts.TryGetValue("Telecommunication Dish", out part);

			if (part == null)
			{
				Debug.Log("Telecommunication Dish not found!");
				return;
			}

			part.gameObject.AddComponent<TelecommunicationDishModule>();

			Debug.Log("Telecommunication Dish component added!");
		}
    }
}
