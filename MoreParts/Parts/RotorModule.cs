using SFS;
using SFS.Parts;
using SFS.Parts.Modules;
using SFS.Translations;
using SFS.UI;
using SFS.Variables;
using SFS.World;
using System;
using UnityEngine;
using static SFS.World.Rocket;

namespace MorePartsMod.Parts
{
	class RotorModule : MonoBehaviour, INJ_IsPlayer, INJ_Location, INJ_Throttle, INJ_Physics
	{
		private VariablesModule _variables;
		private Part _part;
		private Animator _animator;
		private VariableList<bool>.Variable _isOn;
		private VariableList<double>.Variable _throttle;
		private VariableList<double>.Variable _flow_rate;
		private FlowModule _source;
		private Rigidbody2D _rb2d;
		private Location _location;
		private float _turn;
		private Transform _base;
		private const int RPM = 2800;
		private const float Radius = 0.6f;
		private float _area;
		private float _rotorVelocity;

		private bool _isPlayer;

		private I_MsgLogger Logger
		{
			get
			{
				if (!this._isPlayer)
				{
					return new MsgNone();
				}
				return MsgDrawer.main;
			}
		}

		public Rigidbody2D Rb2d { set => this._rb2d = value; }
		public bool IsPlayer { set => this._isPlayer = value; }
		public Location Location { set => this._location = value; }
		public float Throttle {set => this._throttle.Value = value;	}


		private void Awake()
		{
			this._part = this.GetComponent<Part>();
			this._animator = this.GetComponent<Animator>();
			this._source = this.GetComponent<FlowModule>();
			this._variables = this._part.variablesModule;

			this._isOn = this._variables.boolVariables.GetVariable("isOn");
			this._base = this.transform.FindChild("Base");
			this._throttle = this._part.variablesModule.doubleVariables.GetVariable("throttle");
			this._flow_rate = this._part.variablesModule.doubleVariables.GetVariable("flow_rate");
			this._part.onPartUsed.AddListener(this.Toggle);
			this._area = (float)(Math.PI * Radius * Radius);
		}

		private void Start()
		{
			if (GameManager.main == null)
			{
				base.enabled = false;
				return;
			}
			this.CheckOutOfFuel();
			this._source.onStateChange += this.CheckOutOfFuel;
			this._isOn.onValueChange += this.RecalculateRotorThrottle;
			this._throttle.onValueChange += this.RecalculateRotorThrottle;
			this._throttle.onValueChange += this.RecalculateMassFlow;
		}

		private void RecalculateMassFlow()
		{
			this._flow_rate.Value = 0.1 * this._throttle.Value;
		}

		private void RecalculateRotorThrottle()
		{
			this._animator.speed = 100 * (float) this._throttle.Value;
			this._rotorVelocity = (float)(((RPM * this._throttle.Value) / 60) * 2 * Math.PI);
			if (!this._isOn.Value)
			{
				this._throttle.Value =  0f;
				this._flow_rate.Value = 0f;
				this._animator.SetBool("isOn", false);
				return;
			}

			if(this._throttle.Value > 0)
			{
				this._animator.SetBool("isOn", true);
				return;
			}

			this._animator.SetBool("isOn", false);
			this._flow_rate.Value = 0f;
		}

		private void CheckOutOfFuel()
		{
			if (this._isOn.Value && !this.HasFuel(this.Logger))
			{
				this._isOn.Value = false;
				this._flow_rate.Value = 0f;
				this._animator.SetBool("isOn", false);
			}
		}

		private bool HasFuel(I_MsgLogger logger)
		{
			return this._source.CanFlow(logger);
		}

		private void Toggle(UsePartData data)
		{
			if (this._isOn.Value)
			{
				this.DisableRotor(this.Logger);
				return;
			}
			this.EnableRotor(this.Logger);

			data.successfullyUsedPart = true;
		}

		private void EnableRotor(I_MsgLogger logger)
		{
			if (!this.HasFuel(logger))
			{
				return;
			}
			this._isOn.Value = true;

			if (this._throttle.Value == 0f)
			{
				logger.Log(Loc.main.Engine_Module_State.InjectField(this._isOn.Value.State_ToOnOff(), "state"));
			}
		}

		private void DisableRotor(I_MsgLogger logger)
		{
			bool flag = this._throttle.Value == 0f;
			this._isOn.Value = false;
			if (flag)
			{
				logger.Log(Loc.main.Engine_Module_State.InjectField(this._isOn.Value.State_ToOnOff(), "state"));
			}
		}

		private void FixedUpdate()
		{
			if (this._rb2d == null || !this._isOn.Value || this._location.planet == null)
			{
				return;
			}
			float exitVelocity = (float) ((2 * this._rotorVelocity) - this._location.VerticalVelocity);
			float density = (float) this._location.planet.GetAtmosphericDensity(this._location.Height);
			double thrust = 0.5 * density * this._area * ( (exitVelocity * exitVelocity) - (this._location.VerticalVelocity * this._location.VerticalVelocity));

			Vector2 force = this._base.transform.TransformVector(Vector2.up * (float)(thrust/this._rb2d.mass));
			Vector2 relativePoint = this._rb2d.GetRelativePoint(Transform_Utility.LocalToLocalPoint(this.transform, this._rb2d, new Vector2(0,0)));
			this._rb2d.AddForceAtPosition(force, relativePoint, ForceMode2D.Force);
		}

		public static void Setup()
		{
			Part part;
			Base.partsLoader.parts.TryGetValue("Rotor", out part);

			if (part == null)
			{
				Debug.Log("Rotor not found!");
				return;
			}

			part.gameObject.AddComponent<RotorModule>();

			Debug.Log("Rotor component added!");
		}
	
	}
}
