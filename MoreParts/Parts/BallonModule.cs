using SFS;
using SFS.Parts;
using SFS.Parts.Modules;
using SFS.UI;
using SFS.Variables;
using SFS.World;
using System;
using UnityEngine;
using static SFS.World.Rocket;

namespace MorePartsMod.Parts
{
    class BalloonModule : MonoBehaviour, I_PartMenu, INJ_Location, INJ_Physics
	{

		private VariablesModule _variables;
		private VariableList<double>.Variable _state;
		private VariableList<double>.Variable _targetState;

		
		private Location _location;
		private Rigidbody2D _rb2d;

		private double maxDeployVelocity;
		private bool _isOpen;
		private float _thrust;

		public Location Location { get => this._location; set => this._location = value; }

		public Rigidbody2D Rb2d{ get =>	this._rb2d;	set =>	this._rb2d = value;	}
	

		private void Awake()
		{
			this._thrust = 15;//15T
			this.maxDeployVelocity = 250;

			Part balloonPart = this.GetComponent<Part>();
			this._variables = this.GetComponent<VariablesModule>();

			this._state = this._variables.doubleVariables.GetVariable("state");
			this._targetState = this._variables.doubleVariables.GetVariable("state_target");

			balloonPart.onPartUsed.AddListener(this.Deploy);
		}

		private void Start()
		{
			if (GameManager.main == null)
			{
				base.enabled = false;
				return;
			}
			this._isOpen = this._state.Value == 1;
			this._targetState.onValueChange += this.UpdateEnabled;
		}

		private void FixedUpdate()
		{
			if (!this._isOpen)
			{
				return;
			}
			Vector2 force;
			
			if (this._location.Height > this._location.planet.AtmosphereHeightPhysics * 0.88)
			{
				force = base.transform.TransformVector(new Vector2(0, 1) * (this._rb2d.mass * this._location.planet.GetGravity(this._location.position) ));
			}
			else
			{
				if(this._location.VerticalVelocity > 60) {
					force = Vector2.up;
				}
				else
				{
					force = base.transform.TransformVector(new Vector2(0, 1) * (this._thrust * 9.8f));
				}
				
			}

			Vector2 relativePoint = this._rb2d.GetRelativePoint(Transform_Utility.LocalToLocalPoint(base.transform, this._rb2d, new Vector2(-0.5f, -0.047f)));
			this._rb2d.AddForceAtPosition(force, relativePoint, ForceMode2D.Force);
		}

		public void Draw(StatsMenu drawer, PartDrawSettings settings)
		{
			drawer.DrawStat(40, "Drawer", null);
		}

		public void Deploy(UsePartData data)
		{
			bool flag = false;
			double parachuteMultiplier = this.Location.planet.data.atmospherePhysics.parachuteMultiplier; 
			if (this._targetState.Value == 0f && this._state.Value == 0f)
			{
				if (!this.Location.planet.HasAtmospherePhysics || this.Location.Height > this.Location.planet.AtmosphereHeightPhysics * 0.9)
				{
					MsgDrawer.main.Log("no atmosphere");
				}
				else if (this.Location.velocity.magnitude > this.maxDeployVelocity * parachuteMultiplier)
				{
					MsgDrawer.main.Log("Max velocity 250m/s");
				}
				else
				{
					MsgDrawer.main.Log("Deployed");
					this._targetState.Value = 1f;
					flag = true;
				}
			}
			else if(this._targetState.Value == 1f && this._state.Value == 1f)// is deployed
			{
				MsgDrawer.main.Log("Cut");
				this._targetState.Value = 2f;
				this._state.Value = 2f;
				flag = true;
			}
			else if (this._targetState.Value == 2f)
			{
				flag = true;
			}

			data.successfullyUsedPart = flag;
		}
		private void UpdateEnabled()
		{
			base.enabled = this._isOpen = (this._targetState.Value == 1f);
		}

		public static void Setup()
		{
			Part balloon;
			Base.partsLoader.parts.TryGetValue("Balloon", out balloon);

			if(balloon == null)
			{
				Debug.Log("Balloon not found!");
				return;
			}

			balloon.gameObject.AddComponent<BalloonModule>();
			
			Debug.Log("Balloon component added!");
		}

	}
}
