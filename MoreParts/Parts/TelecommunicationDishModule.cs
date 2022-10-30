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
	public class TelecommunicationDishModule : MonoBehaviour, INJ_Rocket
	{
		private Part _part;
		private VariableList<bool>.Variable _isOn;
		private VariableList<double>.Variable _flowRate;
		private VariableList<double>.Variable _state;
		private VariableList<double>.Variable _targetState;
		private Node RocketNode;
		private bool _notifyDisconnection;
		private bool _notifyConnection;
		private float _time = 3f;
		private const float _ping = 3f;
		private int _maxTimeWarp;
		private FlowModule _source;

		public Node Node { get => this.RocketNode; }
		public Rocket Rocket { set; get; }
		
		public bool IsActive { get => this._isOn.Value; }
		public bool IsConnected { get => !this._notifyConnection; }

		private I_MsgLogger Logger
		{
			get
			{
				if (!this.Rocket.isPlayer)
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
			this._isOn = this._part.variablesModule.boolVariables.GetVariable("isOn");
			this._flowRate = this._part.variablesModule.doubleVariables.GetVariable("flow_rate");
			this._state = this._part.variablesModule.doubleVariables.GetVariable("state");
			this._targetState = this._part.variablesModule.doubleVariables.GetVariable("target_state");
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
			
			if (this._isOn.Value) // telecommunication dish is on 
			{
				this._flowRate.Value = 0.1;
				this.RocketNode = AntennaComponent.main.AddNode(this);
			}
			else
			{
				this._flowRate.Value = 0;
				if (this.Rocket.isPlayer)
				{
					this.Rocket.hasControl.Value = false;
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

			if (!this.Rocket.isPlayer || !this._isOn.Value ) //if is not the player or dish is off
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

			if (AntennaComponent.main.IsConnected(this.RocketNode))
			{
				if (this._notifyConnection)
				{
					MsgDrawer.main.Log("Connected");
					AntennaComponent.main.ShowTelecommunicationLines = true;
					this._notifyConnection = false;
					this._notifyDisconnection = true;
				}
				this.Rocket.hasControl.Value = true;
				return;
			}
			this.DoDisconnection();
		}

		private void DoDisconnection()
		{
			this.Rocket.hasControl.Value = false;
			if (this._notifyDisconnection)
			{
				AntennaComponent.main.ShowTelecommunicationLines = false;
				MsgDrawer.main.Log("No Connection");
				this._notifyDisconnection = false;
				this._notifyConnection = true;
				this._time = _ping;
			}
		}

		private void CheckOutOfElectricity()
		{
			if (this._isOn.Value && !this.HasElectricity(this.Logger))
			{
				this._isOn.Value = false;
				this._targetState.Value = 0;
				this._flowRate.Value = 0;
				this.Rocket.hasControl.Value = false;
			}
		}

		private bool HasElectricity(I_MsgLogger logger)
		{
			return this._source.CanFlow(logger);
		}

		public void _toggle()
		{
			if (!this.Rocket.isPlayer)
			{
				return;
			}

			if (this._isOn.Value)
			{
				AntennaComponent.main.RemoveNode(this);
				MsgDrawer.main.Log("Telecommunication Dish Off");
				this.RocketNode = null;
				this._notifyDisconnection = true;
				this.DoDisconnection(); 
				this._flowRate.Value = 0;
				this._targetState.Value = 0;

			}
			else
			{
				this.RocketNode = AntennaComponent.main.AddNode(this);
				MsgDrawer.main.Log("Telecommunication Dish On");
				this._notifyDisconnection = true;
				this._notifyConnection = true; 
				this._flowRate.Value = 0.1;
				this._targetState.Value = 1;
			}
			this._isOn.Value = !this._isOn.Value;
			this.CheckOutOfElectricity();
		}

		public void Toggle(UsePartData data)
		{
			this._toggle();
			data.successfullyUsedPart = true;
		}

	}
}
