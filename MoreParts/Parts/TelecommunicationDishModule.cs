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
	class TelecommunicationDishModule : MonoBehaviour, INJ_Rocket, INJ_IsPlayer
	{
		private Part _part;
		private VariableList<bool>.Variable _state;
		private Node _rocketNode;
		private Rocket _rocket;
		private bool _notifyDisconnection;
		private bool _notifyConnection;
		private float _time;
		private float _ping;

		public Node Node { get => this._rocketNode; }
		public Rocket Rocket { set => this._rocket = value; get => this._rocket; }
		public bool IsPlayer
		{
			set
			{
				if (!value) // is not the player
				{
					this._rocket.hasControl.Value = true;
					return;
				}

				// is player 
				if (this._state.Value)
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
			this._notifyDisconnection = true;
			
		}

		private void Start()
		{
			if (GameManager.main == null)
			{
				base.enabled = false;
				return;
			}

			KeySettings.AddOnKeyDown_World(KeySettings.Main.Toggle_Telecommunication_Dish, this._toggle);

			if (this._state.Value)
			{
				// conect to ARPANET
				this._rocketNode = ARPANETModule.Main.addNode(this);
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
			this._time = 3f;
			this._ping = 3f;
		}

		private void FixedUpdate()
		{
			if (GameManager.main == null)
			{
				base.enabled = false;
				return;
			}

			if (!this._rocket.isPlayer || !this._state.Value ) 
			{
				return;
			}

			if(WorldTime.main.timewarpSpeed >= 5)
			{
				this.doDisconnection();
				return;
			}

			this._time += Time.deltaTime;
			if(this._time <= this._ping)
			{
				return;
			}
			this._time = 0;

			if (ARPANETModule.Main.isConnected(this._rocketNode))
			{
		
				if (this._notifyConnection)
				{
					MsgDrawer.main.Log("Connected");
					this._notifyConnection = false;
					this._notifyDisconnection = true;
					this._rocket.hasControl.Value = true;
				}
				return;
			}
			this.doDisconnection();
		}

		private void doDisconnection()
		{
			if (this._notifyDisconnection)
			{
				MsgDrawer.main.Log("No Connection");
				this._notifyDisconnection = false;
				this._notifyConnection = true;
				this._rocket.hasControl.Value = false;
				this._time = this._ping;
			}
		}

		private void stateChange()
		{
			Debug.Log("State Change");
			this._rocket.hasControl.Value = this._state.Value;
		}

		private void _toggle()
		{
			if (!this._rocket.isPlayer)
			{
				return;
			}

			if (this._state.Value)
			{
				ARPANETModule.Main.removeNode(this);
				MsgDrawer.main.Log("Telecommunication Dish Off");
				this._rocketNode = null;
			}
			else
			{
				this._rocketNode = ARPANETModule.Main.addNode(this);
				MsgDrawer.main.Log("Telecommunication Dish On");
			}
			//this._flag = false;
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
