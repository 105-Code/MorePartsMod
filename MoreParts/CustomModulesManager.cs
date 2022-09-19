using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorePartsMod
{
    public class CustomModulesManager
    {
        public static CustomModulesManager Main;

        public List<CustomModuleData> CustomModulesQueue { get => this._customModulesQueue; }

        private List<CustomModuleData> _customModulesQueue;

        

        public CustomModulesManager()
        {
            Main = this;
            this._customModulesQueue = new List<CustomModuleData>();
        }

        /**
        * This method helps you to add custom Moreparts Mod modules in your parts.
        * 
        * <param name="customModule">You need specify what modules you want to attach to custom part</param>
        * <param name="partName">You need say what custom part you want to add the custom module</param>
        * 
        * <example>
        *   MorePartsMod.mMain.CustomModules.SetMorePartsCustomModule(typeof(TelecommunicationDishModule),'MyAntenna');
        * </example>
        */
        public bool SetMorePartsCustomModule(Type customModule, string partName)
        {
            foreach(CustomModuleData item in _customModulesQueue)
            {
                if (item.partName == partName && item.customModule == customModule)
                {
                    return false;
                }
            }
            CustomModuleData toAdd = new CustomModuleData(customModule, partName);
            this._customModulesQueue.Add(toAdd);
            return true;
        }


        public class CustomModuleData
        {
            public Type customModule;
            public string partName;

            public CustomModuleData(Type customModule, string partName)
            {
                this.customModule = customModule;
                this.partName = partName;
            }
        }
    }
}
