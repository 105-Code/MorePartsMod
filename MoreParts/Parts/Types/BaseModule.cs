using SFS;
using SFS.Parts;
using SFS.UI;
using SFS.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static SFS.World.Rocket;

namespace MorePartsMod.Parts.Types
{
    public abstract class BaseModule : MonoBehaviour, INJ_IsPlayer
    {
		public bool IsPlayer { set; get; }
		
		public Part Part { private set; get; }

		public VariablesModule VariableModule { private set; get; }

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

		public virtual void Awake()
		{
			this.Part = this.GetComponent<Part>();
			this.VariableModule = this.Part.variablesModule;
		}

		public VariableList<bool>.Variable getBoolVariable(string variableName)
		{
			return this.VariableModule.boolVariables.GetVariable(variableName);
		}

		public VariableList<double>.Variable getDoubleVariable(string variableName)
		{
			return this.VariableModule.doubleVariables.GetVariable(variableName);
		}


	}
}
