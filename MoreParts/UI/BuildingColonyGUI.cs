﻿using MorePartsMod.Buildings;
using MorePartsMod.Managers;
using SFS.Input;
using SFS.UI.ModGUI;
using UnityEngine;
using UnityEngine.UI;
using static MorePartsMod.Buildings.ColonyComponent;

namespace MorePartsMod.UI
{
    class BuildingColonyGUI : Screen_Base
    {
        public override bool PauseWhileOpen => false;

        private Window _holder;

        public override void OnClose()
        {
            GameObject.Destroy(this._holder.gameObject);
        }

        public override void OnOpen()
        {
            this._holder = Builder.CreateWindow(this.transform,2, 500, 700,0,350,titleText:"Colonies");
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

        private void generateUI()
        {
            foreach(ColonyData colony in MorePartsMod.Main.ColoniesInfo)
            {
                if(!colony.isBuildingActive("Launch pad") || !colony.isBuildingActive("VAB"))
                {
                    continue;
                }
                Builder.CreateButton(this._holder.ChildrenHolder, 480, 60, 0, 0, () => this.SetSpawnPoint(colony), colony.name);
            }

            Builder.CreateButton(this._holder.ChildrenHolder, 480, 60, 40, 0, () => this.SetSpawnPoint(null), "Space Center");
        }

        private void SetSpawnPoint(ColonyData spawnPoint)
        {
            MorePartsMod.Main.spawnPoint = spawnPoint;
            ScreenManager.main.CloseCurrent();
        }
    }
}
//