using SFS;
using SFS.Parts.Modules;
using SFS.UI;
using UnityEngine;
using static SFS.World.Rocket;

namespace MorePartsMod.Parts.Types
{
    public abstract class ElectricalModule : MonoBehaviour, INJ_IsPlayer
    {
        public FlowModule FlowModule;
        public bool IsPlayer { set; get; }
        public I_MsgLogger Logger
        {
            get
            {
                if (!this.IsPlayer)
                {
                    return new MsgNone();
                }
                return MsgDrawer.main;
            }
        }

        public abstract void CheckOutOfFuel();

        public bool HasFuel(I_MsgLogger logger)
        {
            return this.FlowModule.CanFlow(logger);
        }

    }
}
