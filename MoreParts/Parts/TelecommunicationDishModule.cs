using MorePartsMod.ARPA;
using MorePartsMod.Buildings;
using SFS;
using SFS.Parts;
using SFS.Parts.Modules;
using SFS.UI;
using SFS.Variables;
using SFS.World;
using UnityEngine;
using static SFS.Parts.Modules.FlowModule;
using static SFS.World.Rocket;

namespace MorePartsMod.Parts
{
	class TelecommunicationDishModule : MonoBehaviour, INJ_Rocket
	{
		private Part _part;
		private VariableList<bool>.Variable _state;
		private VariableList<double>.Variable _flow_rate;
		private Node _rocketNode;
		private Rocket _rocket;
		private bool _notifyDisconnection;
		private bool _notifyConnection;
		private float _time = 3f;
		private const float _ping = 3f;
		private int _maxTimeWarp;
		private FlowModule _source;

		public Node Node { get => this._rocketNode; }
		public Rocket Rocket { set => this._rocket = value; get => this._rocket; }
		
		private I_MsgLogger Logger
		{
			get
			{
				if (!this._rocket.isPlayer)
				{
					return new MsgNone();
				}
				return MsgDrawer.main;
			}
		}


		private void Awake()
		{
			this._part = this.GetComponent<Part>();
			this._source = this.GetComponent<FlowModule>();
			this._state = this._part.variablesModule.boolVariables.GetVariable("isOn");
			this._flow_rate = this._part.variablesModule.doubleVariables.GetVariable("flow_rate");
			this._part.onPartUsed.AddListener(this.Toggle);

			this._notifyConnection = true;
			this._notifyDisconnection = true;
			this._maxTimeWarp = Base.worldBase.settings.difficulty.MaxPhysicsTimewarpIndex == 3 ? 5 : 3;
		}

		private void Start()
		{
			if (GameManager.main == null)
			{
				base.enabled = false;
				return;
			}
			this._source.onStateChange += this.CheckOutOfElectricity;
			this.CheckOutOfElectricity();
			
			if (this._state.Value) // telecommunication dish is on 
			{
				this._flow_rate.Value = 0.1;
				this._rocketNode = AntennaComponent.main.AddNode(this);
			}
			else
			{
				this._flow_rate.Value = 0;
				if (this._rocket.isPlayer)
				{
					this._rocket.hasControl.Value = false;
				}
			}
		}

		private void FixedUpdate()
		{
			if (GameManager.main == null)
			{
				base.enabled = false;
				return;
			}

			if (!this._rocket.isPlayer || !this._state.Value ) //if is not the player or dish is off
			{
				return;
			}

			if(WorldTime.main.timewarpSpeed > this._maxTimeWarp)
			{
				this.DoDisconnection();
				return;
			}			

			this._time += Time.deltaTime;
			if(this._time <= _ping)
			{
				return;
			}
			this._time = 0;

			if (AntennaComponent.main.IsConnected(this._rocketNode))
			{
				if (this._notifyConnection)
				{
					MsgDrawer.main.Log("Connected");
					this._notifyConnection = false;
					this._notifyDisconnection = true;
				}
				this._rocket.hasControl.Value = true;
				return;
			}
			this.DoDisconnection();
		}

		private void DoDisconnection()
		{
			this._rocket.hasControl.Value = false;
			if (this._notifyDisconnection)
			{
				MsgDrawer.main.Log("No Connection");
				this._notifyDisconnection = false;
				this._notifyConnection = true;
				this._time = _ping;
			}
		}

		private void CheckOutOfElectricity()
		{
			if (this._state.Value && !this.HasElectricity(this.Logger))
			{
				this._state.Value = false;
				this._flow_rate.Value = 0;
				this._rocket.hasControl.Value = false;
			}
		}

		private bool HasElectricity(I_MsgLogger logger)
		{
			return this._source.CanFlow(logger);
		}

		public void _toggle()
		{
			if (!this._rocket.isPlayer)
			{
				return;
			}

			if (this._state.Value)
			{
				AntennaComponent.main.RemoveNode(this);
				MsgDrawer.main.Log("Telecommunication Dish Off");
				this._rocketNode = null;
				this._notifyDisconnection = true;
				this.DoDisconnection(); 
				this._flow_rate.Value = 0;
			}
			else
			{
				this._rocketNode = AntennaComponent.main.AddNode(this);
				MsgDrawer.main.Log("Telecommunication Dish On");
				this._notifyDisconnection = true;
				this._notifyConnection = true; 
				this._flow_rate.Value = 0.1;
			}
			this._state.Value = !this._state.Value;
			this.CheckOutOfElectricity();
		}

		public void Toggle(UsePartData data)
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
