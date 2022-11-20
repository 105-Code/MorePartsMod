﻿using MorePartsMod.Buildings;
using MorePartsMod.Managers;
using SFS.Input;
using SFS.UI.ModGUI;
using UnityEngine;
using UnityEngine.UI;
using static MorePartsMod.Buildings.ColonyComponent;

namespace MorePartsMod.UI
{
    class ColonyGUI : Screen_Base
    {
        public override bool PauseWhileOpen => false;
        public ColonyComponent Colony {set{
                this._colony = value;
            } 
        }

        private Window _holder;
        private string _name;
        private ColonyComponent _colony;

        public override void OnClose()
        {
            GameObject.Destroy(this._holder.gameObject);
            if (this._name != this._colony.data.name)
            {
                this._colony.data.name = this._name;
                ColonyManager.main.SaveColonies();
            }
        }

        public override void OnOpen()
        {
            this._name = this._colony.data.name;
            this._holder = Builder.CreateWindow(this.transform,2, 500, 700,posY:350,titleText:"Colony Menu");
            this._holder.CreateLayoutGroup(Type.Vertical).spacing = 20f;
            this._holder.CreateLayoutGroup(Type.Vertical).DisableChildControl();
            this._holder.CreateLayoutGroup(Type.Vertical).childAlignment = TextAnchor.UpperCenter;
            this.generateUI();
        }

        public override void ProcessInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape)){
                ScreenManager.main.CloseCurrent();
            }
        }

        private void onChangeColonyName(string value)
        {
            this._name = value;
        }

        private void generateUI()
        {
            Builder.CreateLabel(this._holder.ChildrenHolder, 480, 35, 0, 0, "Information");
            Builder.CreateTextInput(this._holder.ChildrenHolder, 480, 50, 0, 0, this._colony.data.name,this.onChangeColonyName);
            foreach(string key in this._colony.data.resources.Keys)
            {
                Builder.CreateLabel(this._holder.ChildrenHolder, 480, 35, 0, 0, key + ": " +this._colony.data.resources[key]);
            }

            Builder.CreateLabel(this._holder.ChildrenHolder, 480, 35, 0, 0, "Buildings");

            foreach(string buildingName in MorePartsModMain.Main.ColonyBuildingFactory.GetBuildingsName())
            {
                if (this._colony.data.isBuildingActive(buildingName))
                {
                    continue;
                }
                Builder.CreateButton(this._holder.ChildrenHolder, 480, 60, 0, 0, () => this.OnClickButton(buildingName), "Build " + buildingName);
            }

            Builder.CreateButton(this._holder.ChildrenHolder, 480, 60, 40, 0, () => ScreenManager.main.CloseCurrent(), "Close");
        }

        private void OnClickButton(string buildingName)
        {
            if (this._colony.Build(buildingName))
            {
                ScreenManager.main.CloseCurrent();
            }
        }

    }
}
//