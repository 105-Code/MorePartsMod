using MorePartsMod.Parts.Types;
using SFS.Parts;
using SFS.UI;
using SFS.Variables;
using SFS.World;
using UnityEngine;

namespace MorePartsMod.Parts
{
    class ContinuousTrackModule : ElectricalModule, Rocket.INJ_TurnAxisWheels
    {

        private WheelModule _wheel;
        private VariableList<bool>.Variable _on;
        private Animator _animator;

        public float TurnAxis { get; set; }

        public override void Awake()
        {
            base.Awake();
        }

        public void Start()
        {
            this.Part.onPartUsed.AddListener(this.ToggleEnabled);
            this._animator = this.GetComponent<Animator>();

            this._on = this.Part.variablesModule.boolVariables.GetVariable("wheel_on");

            this._wheel = this.Part.GetModules<WheelModule>()[0];
        }

        public void ToggleEnabled(UsePartData data)
        {
            this._on.Value = !this._on.Value;
            data.successfullyUsedPart = true;
            MsgDrawer.main.Log(this._on.Value? "Continuous Track On": "Continuous Track off");
        }

        private void Update()
        {
            if (GameManager.main == null || !this._on.Value)
            {
                base.enabled = false;
                return;
            }

            if(this.TurnAxis > 0)
            {
                Debug.Log("Positibve"+ this.TurnAxis);
                this._animator.SetInteger("velocity", 1);
                this._animator.speed = 1;
                return;
            }

            if (this.TurnAxis < 0)
            {
                Debug.Log("Negative" + this.TurnAxis);
                this._animator.SetInteger("velocity", -1);
                this._animator.speed = -1;
                return;
            }

            Debug.Log("Equeal" + this.TurnAxis);
            this._animator.SetInteger("velocity", 0);
            this._animator.speed = 0;

        }

        public override void CheckOutOfFuel()
        {
            throw new System.NotImplementedException();
        }

    }
}
