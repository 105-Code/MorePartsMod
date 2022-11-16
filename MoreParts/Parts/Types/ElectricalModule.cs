using SFS;
using SFS.Parts.Modules;
using UnityEngine;

namespace MorePartsMod.Parts.Types
{
    public abstract class ElectricalModule : BaseModule
    {
        public FlowModule FlowModule { private set; get; }

        public override void Awake()
        {
            base.Awake();
            this.FlowModule = this.GetComponent<FlowModule>();
        }

        public abstract void CheckOutOfFuel();

        public bool HasFuel(I_MsgLogger logger)
        {
            return this.FlowModule.CanFlow(logger);
        }

    }
}
