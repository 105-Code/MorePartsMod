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
    public class BalloonModule : MonoBehaviour, INJ_Location, INJ_Physics
	{
		private OrientationModule _orientation;
		private VariablesModule _variables;
		private Part _part;

		private VariableList<double>.Variable _state;
		private VariableList<double>.Variable _targetState;

		private Transform _balloon;
		private Location _location;
		private Rigidbody2D _rb2d;

		private double _volumn = 1.33f * Math.PI * Math.Pow(300, 3);
		private bool _isOpen;

		public Location Location { get => this._location; set => this._location = value; }
		public Rigidbody2D Rb2d{ get =>	this._rb2d;	set =>	this._rb2d = value;	}

		private void Awake()
		{
			this._part = this.GetComponent<Part>();

			this._variables = this._part.variablesModule;
			this._orientation = this._part.orientation;
			this._isOpen = false;

			this._part.onPartUsed.AddListener(this.Deploy);
			this._balloon = this.transform.FindChild("Deployed Ballon");
			this._state = this._variables.doubleVariables.GetVariable("state");
			this._targetState = this._variables.doubleVariables.GetVariable("state_target");
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
			if (!this._isOpen || this.Location.planet == null || this._location == null)
			{
				return;
			}

			Vector2 force;
			
			float airDensity = (float) this._location.planet.GetAtmosphericDensity(this._location.Height);
			float gravity = (float) this._location.planet.GetGravity(this._location.position).y * -1;
			float ascensionForce = ((airDensity * gravity  * (float)this._volumn) - this._rb2d.mass* gravity * 1000)/1000;
			float aceleration = (float)Math.Sqrt(ascensionForce / 0.5f * this._rb2d.mass);

			if (this._location.VerticalVelocity < 30)
				force = this._balloon.transform.TransformVector(Vector2.up * (aceleration-gravity));
			else
				force = this._balloon.transform.TransformVector(Vector2.up);

			Vector2 relativePoint = this._rb2d.GetRelativePoint(Transform_Utility.LocalToLocalPoint(base.transform, this._rb2d, new Vector2(-0.5f, -0.047f)));
			this._rb2d.AddForceAtPosition(force, relativePoint, ForceMode2D.Force);
		}

		private void LateUpdate()
		{
			if (GameManager.main == null || this.Location.planet == null || this._location == null)
			{
				return;
			}

			float newRotation = (this._orientation.orientation.Value.z * -1 * this._orientation.orientation.Value.x) - (this._rb2d.transform.localEulerAngles.z * this._orientation.orientation.Value.x) ;
			this._balloon.localEulerAngles = new Vector3(1, 1, newRotation + Mathf.Sin(Time.time) * 3f * this._balloon.parent.lossyScale.x * this._balloon.parent.lossyScale.y);
		}
	
		public void Deploy(UsePartData data)
		{
			bool flag = false;

			if (this._targetState.Value == 0f && this._state.Value == 0f)
			{
				if (!this.Location.planet.HasAtmospherePhysics || this.Location.Height > this.Location.planet.AtmosphereHeightPhysics * 0.9)
				{
					MsgDrawer.main.Log("Not atmosphere");
					flag = false;
				}
				else
				{
					MsgDrawer.main.Log("Deployed");
					this._targetState.Value = 1f;
					flag = true;
				}

				
			}
			else if(this._targetState.Value == 1f && this._state.Value == 1f)
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
