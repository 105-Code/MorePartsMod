using MorePartsMod.Parts.Types;
using SFS;
using SFS.Parts;
using SFS.Translations;
using SFS.Variables;
using SFS.World;
using System;
using UnityEngine;
using static SFS.World.Rocket;


namespace MorePartsMod.Parts
{
	public class RotorModule : ElectricalModule, INJ_Location, INJ_Throttle, INJ_Physics
	{

		private Animator _animator;

		private VariableList<bool>.Variable _isOn;
		private VariableList<double>.Variable _throttle;
		private VariableList<double>.Variable _flow_rate;

		private Transform _base;

		private const int RPM = 2800;
		private const float Radius = 0.6f;
		private float _area;
		private float _rotorVelocity;

		public Rigidbody2D Rb2d { set; get; }
		public Location Location { set; get; }
		public float Throttle {set => this._throttle.Value = value;	}


		public override void Awake()
		{
			base.Awake();
			this._animator = this.GetComponent<Animator>();

			this._isOn = this.getBoolVariable("isOn");
			this._flow_rate = this.getDoubleVariable("flow_rate");
			this._throttle = this.getDoubleVariable("throttle");

			this._base = this.transform.FindChild("Base");

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
			this.FlowModule.onStateChange += this.CheckOutOfFuel;
			this._isOn.onValueChange += this.RecalculateRotorThrottle;
			this._throttle.onValueChange += this.RecalculateRotorThrottle;
			this._throttle.onValueChange += this.RecalculateMassFlow;
			this.Part.onPartUsed.AddListener(this.Toggle);
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

		public override void CheckOutOfFuel()
		{
			if (this._isOn.Value && !this.HasFuel(this.Logger))
			{
				this._isOn.Value = false;
				this._flow_rate.Value = 0f;
				this._animator.SetBool("isOn", false);
			}
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
			if (this.Rb2d == null || !this._isOn.Value || this.Location.planet == null)
			{
				return;
			}

			float exitVelocity = (float) ((2 * this._rotorVelocity) - this.Location.VerticalVelocity);
			float density = (float) this.Location.planet.GetAtmosphericDensity(this.Location.Height);
			double thrust = 0.5 * density * this._area * ( (exitVelocity * exitVelocity) - (this.Location.VerticalVelocity * this.Location.VerticalVelocity));

			Vector2 force = this._base.transform.TransformVector(Vector2.up * (float)(thrust/this.Rb2d.mass));
			Vector2 relativePoint = this.Rb2d.GetRelativePoint(Transform_Utility.LocalToLocalPoint(this.transform, this.Rb2d, new Vector2(0,0)));
			this.Rb2d.AddForceAtPosition(force, relativePoint, ForceMode2D.Force);
		}

	}
}
