using MorePartsMod.Buildings;
using MorePartsMod.Managers;
using SFS.Input;
using SFS.UI;
using SFS.UI.ModGUI;
using UnityEngine;

namespace MorePartsMod.UI
{
    class ColonyGUI : Screen_Base
    {
        public override bool PauseWhileOpen => false;
        public ColonyComponent Colony
        {
            set
            {
                this._colony = value;
            }
        }

        private Window _holder;
        private string _name;
        private ColonyComponent _colony;

        public override void OnClose()
        {
            GameObject.Destroy(this._holder.gameObject);
            if (this._name != this._colony.Data.name)
            {
                this._colony.Data.name = this._name;
                ColonyManager.Main.SaveColonies();
            }
        }

        public override void OnOpen()
        {
            this._name = this._colony.Data.name;
            this._holder = Builder.CreateWindow(this.transform, 2, 500, 700, posY: 350, titleText: "Colony Menu");
            this._holder.EnableScrolling(Type.Vertical);
            this._holder.CreateLayoutGroup(Type.Vertical).spacing = 20f;
            this._holder.CreateLayoutGroup(Type.Vertical).DisableChildControl();
            this._holder.CreateLayoutGroup(Type.Vertical).childAlignment = TextAnchor.UpperCenter;
            this.generateUI();
        }

        private void Reload()
        {
            this.OnClose();
            this.OnOpen();
        }

        public override void ProcessInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
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
            Builder.CreateTextInput(this._holder.ChildrenHolder, 480, 50, 0, 0, this._colony.Data.name, this.onChangeColonyName);
            foreach (string key in this._colony.Data.resources.Keys)
            {
                Builder.CreateLabel(this._holder.ChildrenHolder, 480, 35, 0, 0, key + ": " + this._colony.Data.resources[key]);
            }

            Builder.CreateLabel(this._holder.ChildrenHolder, 480, 35, 0, 0, "Buildings");

            foreach (string buildingName in MorePartsPack.Main.ColonyBuildingFactory.GetBuildingsName())
            {
                /*
                Cahange this to able build the same building x times
                if (this._colony.Data.IsBuildingActive(buildingName) && buildingName != MorePartsTypes.SOLAR_PANEL_BUILDING)
                {
                    continue;
                }*/
                if (this._colony.Data.IsBuildingActive(buildingName))
                {
                    continue;
                }
                Builder.CreateButton(this._holder.ChildrenHolder, 480, 60, 0, 0, () => this.OnClickButton(buildingName), "Build " + buildingName);
            }

            if (this._colony.Data.IsBuildingActive(MorePartsTypes.REFINERY_BUILDING))
            {
                Builder.CreateLabel(this._holder.ChildrenHolder, 480, 45, 0, 0, "Refine materials");
                Builder.CreateButton(this._holder.ChildrenHolder, 480, 40, 0, 0, () => this.generateResource(MorePartsTypes.CONSTRUCTION_MATERIAL), "+100 Construction material");
                Builder.CreateButton(this._holder.ChildrenHolder, 480, 40, 0, 0, () => this.generateResource(MorePartsTypes.ELECTRONIC_COMPONENT), "+100 Electronic material");
                Builder.CreateButton(this._holder.ChildrenHolder, 480, 40, 0, 0, () => this.generateResource(MorePartsTypes.ROCKET_MATERIAL), "+100 Rocket material");
            }
            Builder.CreateSpace(this._holder.ChildrenHolder, 480, 30);
            Builder.CreateButton(this._holder.ChildrenHolder, 480, 60, 40, 0, () => ScreenManager.main.CloseCurrent(), "Save");
            Builder.CreateSpace(this._holder.ChildrenHolder, 480, 15);
            Builder.CreateButton(this._holder.ChildrenHolder, 480, 60, 40, 0, () => this.OnDelete(), "Delete");
        }

        private void generateResource(string resourcesType, int toGenerate = 100)
        {
            if (!this._colony.Data.resources.ContainsKey(MorePartsTypes.MATERIAL))
            {
                MsgDrawer.main.Log("There are not material in the colony");
                return;
            }

            double materialQuantity = this._colony.Data.GetResource(MorePartsTypes.MATERIAL);
            if (materialQuantity - toGenerate < 0)
            {
                this._colony.Data.TakeResource(MorePartsTypes.MATERIAL, materialQuantity);
                this._colony.Data.AddResource(resourcesType, materialQuantity);
                this.Reload();
                return;
            }

            this._colony.Data.TakeResource(MorePartsTypes.MATERIAL, toGenerate);
            this._colony.Data.AddResource(resourcesType, toGenerate);
            this.Reload();
        }

        private void OnDelete()
        {
            if (ColonyManager.Main.DeleteColony(this._colony))
            {
                ScreenManager.main.CloseCurrent();
                return;
            }
        }

        private void OnClickButton(string buildingName)
        {
            if (this._colony.CreateBuilding(buildingName))
            {
                ScreenManager.main.CloseCurrent();
            }
        }

        public static ColonyGUI Init()
        {
            GameObject holder = new GameObject("Colony Menu");
            holder.transform.localScale = new Vector3(0.9f, 0.9f);
            Builder.AttachToCanvas(holder, Builder.SceneToAttach.CurrentScene);
            return holder.AddComponent<ColonyGUI>();
        }
    }
}