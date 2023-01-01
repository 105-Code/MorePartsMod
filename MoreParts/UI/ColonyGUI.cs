using MorePartsMod.Buildings;
using MorePartsMod.Managers;
using SFS.Input;
using SFS.UI;
using SFS.UI.ModGUI;
using UnityEngine;
using UnityEngine.UI;
using static MorePartsMod.Buildings.ColonyComponent;

namespace MorePartsMod.UI
{  
    // GUI class for interacting with a colony
    class ColonyGUI : Screen_Base
    {
         // Whether the game should be paused while the GUI is open
        public override bool PauseWhileOpen => false;
        
        // Property for setting the colony to display in the GUI
        public ColonyComponent Colony {set{
                this._colony = value;
            } 
        }

        private Window _holder; // Window for displaying the GUI
        private string _name; // Name of the colony displayed in the GUI
        private ColonyComponent _colony; // Colony displayed in the GUI
        
        // Called when the GUI is closed
        public override void OnClose()
        {
            GameObject.Destroy(this._holder.gameObject); // Destroy the window object
            if (this._name != this._colony.data.name) // Save the colony if the name has been changed
            {
                this._colony.data.name = this._name;
                ColonyManager.main.SaveColonies();
            }
        }

        public override void OnOpen() // Called when the GUI is opened
        {
            this._name = this._colony.data.name; // Save the colony's name
            this._holder = Builder.CreateWindow(this.transform,2, 500, 700,posY:350,titleText:"Colony Menu"); // Create the window for displaying the GUI
            this._holder.EnableScrolling(Type.Vertical);
            
            // Create a vertical layout group for the window
            this._holder.CreateLayoutGroup(Type.Vertical).spacing = 20f;
            this._holder.CreateLayoutGroup(Type.Vertical).DisableChildControl();
            this._holder.CreateLayoutGroup(Type.Vertical).childAlignment = TextAnchor.UpperCenter;
            
            this.generateUI(); // Generate the UI elements for the window
        }

        private void Reload() // Reloads the GUI
        {
            this.OnClose();
            this.OnOpen();
        }
        
        // Processes input for the GUI
        public override void ProcessInput()
        {
            // Close the GUI if the escape key is pressed
            if (Input.GetKeyDown(KeyCode.Escape)){
                ScreenManager.main.CloseCurrent();
            }
        }

        private void onChangeColonyName(string value)
        {
            this._name = value; // Update the name of the colony
        }

        private void generateUI()
        {
            Builder.CreateLabel(this._holder.ChildrenHolder, 480, 35, 0, 0, "Information"); // Display label "Information"
            Builder.CreateTextInput(this._holder.ChildrenHolder, 480, 50, 0, 0, this._colony.data.name,this.onChangeColonyName); // Create a text input for the user to enter the colony name
            
            // Display the current quantity of each resource in the colony
            foreach(string key in this._colony.data.resources.Keys)
            {
                Builder.CreateLabel(this._holder.ChildrenHolder, 480, 35, 0, 0, key + ": " +this._colony.data.resources[key]);
            }
            
            Builder.CreateLabel(this._holder.ChildrenHolder, 480, 35, 0, 0, "Buildings"); // Display label "Buildings"

            // Create buttons for each building that can be built in the colony
            foreach (string buildingName in MorePartsModMain.Main.ColonyBuildingFactory.GetBuildingsName())
            {
                if (this._colony.data.isBuildingActive(buildingName))
                {
                    continue;
                }
                Builder.CreateButton(this._holder.ChildrenHolder, 480, 60, 0, 0, () => this.OnClickButton(buildingName), "Build " + buildingName);
            }
            
            // If the colony has a refinery building, create buttons for generating refined materials
            if (this._colony.data.isBuildingActive(MorePartsTypes.REFINERY_BUILDING))
            {
                Builder.CreateLabel(this._holder.ChildrenHolder, 480, 45, 0, 0, "Refine materials");
                Builder.CreateButton(this._holder.ChildrenHolder, 480, 40, 0, 0, () => this.generateResource(MorePartsTypes.CONSTRUCTION_MATERIAL), "+100 Construction material");
                Builder.CreateButton(this._holder.ChildrenHolder, 480, 40, 0, 0, () => this.generateResource(MorePartsTypes.ELECTRONIC_COMPONENT), "+100 Electronic material");
                Builder.CreateButton(this._holder.ChildrenHolder, 480, 40, 0, 0, () => this.generateResource(MorePartsTypes.ROCKET_MATERIAL), "+100 Rocket material");
            }

            Builder.CreateButton(this._holder.ChildrenHolder, 480, 60, 40, 0, () => ScreenManager.main.CloseCurrent(), "Save"); // Create a button for saving the changes made to the colony
        }

        private void generateResource(string resourcesType, int toGenerate = 100)
        {
            if (!this._colony.data.resources.ContainsKey(MorePartsTypes.MATERIAL)) // Check if the colony has any raw material to use for refining
            {   
                MsgDrawer.main.Log("There are not material in the colony");
                return;
            }
            
            double materialQuantity = this._colony.data.getResource(MorePartsTypes.MATERIAL); // Get the current quantity of raw material in the colony
            // If the colony doesn't have enough raw material to generate the requested amount of refined material,
            // use all of the available raw material instead
            if (materialQuantity - toGenerate < 0)
            {
                
                this._colony.data.takeResource(MorePartsTypes.MATERIAL, materialQuantity); // Remove all of the available raw material from the colony
                this._colony.data.addResource(resourcesType, materialQuantity); // Add the same amount of refined material to the colony
                this.Reload(); // Update the user interface
                return;
            }

            this._colony.data.takeResource(MorePartsTypes.MATERIAL, toGenerate); // If the colony has enough raw material, remove the specified amount and add the corresponding amount of refined material
            this._colony.data.addResource(resourcesType, toGenerate);
            this.Reload(); // Update the user interface
        }

        private void OnClickButton(string buildingName)
        {
            if (this._colony.Build(buildingName)) // Attempt to build the specified building in the colony
            {
                ScreenManager.main.CloseCurrent(); // If the building was successfully built, close the current screen
            }
        }

    }
}
