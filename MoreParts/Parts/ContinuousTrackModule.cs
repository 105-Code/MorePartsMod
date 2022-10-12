using SFS.Parts;
using SFS.Translations;
using SFS.UI;
using SFS.Variables;
using SFS.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MorePartsMod.Parts
{
    class ContinuousTrackModule : MonoBehaviour, Rocket.INJ_TurnAxisWheels
    {

        private WheelModule _wheel;
        private VariableList<bool>.Variable _on;
        private Animator _animator;

        public float TurnAxis { get; set; }

        public void Start()
        {
            Part part = this.GetComponent<Part>();
            part.onPartUsed.AddListener(this.ToggleEnabled);
            this._animator = this.GetComponent<Animator>();

            this._on = part.variablesModule.boolVariables.GetVariable("wheel_on");

            this._wheel = part.GetModules<WheelModule>()[0];
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
    }
}
