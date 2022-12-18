using MorePartsMod.Parts.Types;
using SFS.Parts;
using SFS.UI;
using SFS.Variables;
using SFS.World;
using UnityEngine;

namespace MorePartsMod.Parts
{
    class ContinuousTrackModule : ElectricalModule, Rocket.INJ_TurnAxisWheels, Rocket.INJ_Physics
    {
        private VariableList<bool>.Variable _on;
        private VariableList<double>.Variable _traction;
        private VariableList<double>.Variable _power;
        private VariableList<double>.Variable _flow_rate;
        private Animator _animator;

        private Transform[] _idlers;

        public float angularVelocity;
        private const float MAX_ANGULAR_VELOCITY = 550; 

        public float TurnAxis { get; set; }
        public Rigidbody2D Rb2d { set; get; }

        public override void Awake()
        {
            base.Awake();
            this._animator = this.GetComponent<Animator>();
        }

        public void Start()
        {
            this.Part.onPartUsed.AddListener(this.ToggleEnabled);
            this._on = this.getBoolVariable("wheel_on");
            this._traction = this.getDoubleVariable("traction");
            this._flow_rate = this.getDoubleVariable("flow_rate");
            this._power = this.getDoubleVariable("power");
            this.FlowModule.onStateChange += this.CheckOutOfFuel;
            this.GetIdlers();
            this.angularVelocity = 0;
            this.CheckOutOfFuel();
        }

        public void ToggleEnabled(UsePartData data)
        {
            this._on.Value = !this._on.Value;
            MsgDrawer.main.Log(this._on.Value? "Turn On": "Turn off");
            data.successfullyUsedPart = true;
        }

        private void Update()
        {
            if (GameManager.main == null || !this._on.Value)
            {
                return;
            }

            float direction = this._on.Value ? (-this.TurnAxis) : 0f;

            if (direction != 0)
            {
                this._flow_rate.Value = 0.1;
                this._animator.SetInteger("velocity", 1);
                this._animator.speed = 1;
                this.angularVelocity = Mathf.Clamp(this.angularVelocity + direction * (float) this._power.Value * Time.deltaTime, -MAX_ANGULAR_VELOCITY, MAX_ANGULAR_VELOCITY);
                this.UpdateIdlers(direction);
                return; 
            }

            this._flow_rate.Value = 0;
            this.angularVelocity = 0;
            this._animator.SetInteger("velocity", 0);
            this._animator.speed = 0;
        }

        public void UpdateIdlers(float direction)
        {
            foreach(Transform idler in this._idlers)
            {
                idler.eulerAngles = new Vector3(0f, 0f, idler.eulerAngles.z + 4 * direction);
            }
        }

        public override void CheckOutOfFuel()
        {
            if (this._on.Value && !this.HasFuel(this.Logger))
            {
                this._on.Value = false;
                this._flow_rate.Value = 0f;
                this._animator.SetInteger("velocity", 0);
            }
        }

        private void GetIdlers()
        {
            Transform idlers = this.transform.Find("Idlers");
            if(idlers == null)
            {
                Debug.Log("Not Found Idlers");
                return;
            }
            this._idlers = new Transform[idlers.childCount];
            for (int index = 0; index < this._idlers.Length; index++)
            {
                this._idlers[index] = idlers.GetChild(index);
            }
            Debug.Log(this._idlers.Length);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (GameManager.main == null || !this._on.Value)
            {
                return;
            }

            float d = this.angularVelocity * 0.017453292f;
            float traction = (float)this._traction.Value;
            Vector2 a = Quaternion.Euler(0f, 0f, -90f) * collision.contacts[0].normal;
            Vector2 a2 = collision.contacts[0].relativeVelocity - a * d;
            float magnitude = a2.magnitude;
            float num = 1f;
            num = num * 0.1f * (float)WorldView.main.ViewLocation.planet.data.basics.gravity;
            float num2 = traction / this.Rb2d.mass * Time.fixedDeltaTime * 10f;
            if (num2 > 1f)
            {
                num /= num2;
            }
            this.Rb2d.AddForceAtPosition(a2 * traction * num, base.transform.position);
            if (collision.rigidbody != null)
            {
                collision.rigidbody.AddForceAtPosition(-a2 * traction * num, collision.contacts[0].point);
            }
            float num3 = magnitude * traction * num;
            Vector2 vector = collision.contacts[0].relativeVelocity - a * ((this.angularVelocity + num3) * 0.017453292f);
            Vector2 vector2 = collision.contacts[0].relativeVelocity - a * ((this.angularVelocity - num3) * 0.017453292f);
            float sqrMagnitude = vector.sqrMagnitude;
            float sqrMagnitude2 = vector2.sqrMagnitude;
            if (sqrMagnitude > sqrMagnitude2)
            {
                this.angularVelocity -= magnitude * traction * num;
                return;
            }
            this.angularVelocity += magnitude * traction * num;
        }
    
    }
}
