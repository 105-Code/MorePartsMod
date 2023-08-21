using MorePartsMod.ARPA;
using MorePartsMod.Buildings;
using MorePartsMod.Parts.Types;
using SFS;
using SFS.Parts;
using SFS.UI;
using SFS.Variables;
using SFS.World;
using UnityEngine;
using static SFS.World.Rocket;


namespace MorePartsMod.Parts
{
    public class TelecommunicationDishModule : ElectricalModule, INJ_Rocket
    {

        public Bool_Reference IsOn;
        public Float_Reference FlowRate;
        public Float_Reference TargetState;

        private bool _notifyDisconnection;
        private bool _notifyConnection;
        private float _time = 3f;
        private const float _ping = 3f;
        private int _maxTimeWarp;

        public Node Node { private set; get; }

        public Rocket Rocket { set; get; }

        public bool IsActive { get => this.IsOn.Value; }

        public bool IsConnected { get => !this._notifyConnection; }

        public Part Part;

        public void Awake()
        {
            this.Part.onPartUsed.AddListener(this.Toggle);
            this._notifyConnection = true;
            this._notifyDisconnection = true;
            this._maxTimeWarp = Base.worldBase.settings.difficulty.MaxPhysicsTimewarpIndex == 3 ? 5 : 3;
        }

        public void OnDestroy()
        {
            if (this.Node != null)
            {
                AntennaComponent.main.RemoveNode(this);
                this.Node = null;
            }
        }

        private void Start()
        {
            if (GameManager.main == null)
            {
                base.enabled = false;
                return;
            }
            this.FlowModule.onStateChange += this.CheckOutOfFuel;
            this.CheckOutOfFuel();

            if (this.Rocket.isPlayer)
            {
                if (this.IsOn.Value)
                {
                    this.FlowRate.Value = 0.1f;
                    this.TargetState.Value = 1;
                    this.Node = AntennaComponent.main.AddNode(this);
                }
                else
                {
                    this.FlowRate.Value = 0;
                    this.TargetState.Value = 0;
                    this.Rocket.hasControl.Value = false;
                }
            }
            else
            {
                this.IsOn.Value = true;
                this.FlowRate.Value = 0.1f;
                this.TargetState.Value = 1;
                this.Rocket.hasControl.Value = true;
                this.Node = AntennaComponent.main.AddNode(this);
            }

        }

        private void FixedUpdate()
        {
            if (GameManager.main == null)
            {
                base.enabled = false;
                return;
            }

            if (!this.IsPlayer || !this.IsOn.Value) //if is not the player or dish is off
            {
                return;
            }

            if (WorldTime.main.timewarpSpeed > this._maxTimeWarp)
            {
                this.DoDisconnection();
                return;
            }

            this._time += Time.deltaTime;
            if (this._time <= _ping)
            {
                return;
            }
            this._time = 0;

            if (AntennaComponent.main.IsConnected(this.Node))
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

        public override void CheckOutOfFuel()
        {
            if (this.IsOn.Value && !this.HasFuel(this.Logger))
            {
                this.IsOn.Value = false;
                this.TargetState.Value = 0;
                this.FlowRate.Value = 0;
                this.Rocket.hasControl.Value = false;
            }
        }


        public void _toggle()
        {
            if (!this.Rocket.isPlayer)
            {
                return;
            }

            if (this.IsOn.Value)
            {
                AntennaComponent.main.RemoveNode(this);
                MsgDrawer.main.Log("Telecommunication Dish Off");
                this.Node = null;
                this._notifyDisconnection = true;
                this.DoDisconnection();
                this.FlowRate.Value = 0;
                this.TargetState.Value = 0;

            }
            else
            {
                this.Node = AntennaComponent.main.AddNode(this);
                MsgDrawer.main.Log("Telecommunication Dish On");
                this._notifyDisconnection = true;
                this._notifyConnection = true;
                this.FlowRate.Value = 0.1f;
                this.TargetState.Value = 1;
            }
            this.IsOn.Value = !this.IsOn.Value;
            this.CheckOutOfFuel();
        }

        public void Toggle(UsePartData data)
        {
            this._toggle();
            data.successfullyUsedPart = true;
        }

    }
}
