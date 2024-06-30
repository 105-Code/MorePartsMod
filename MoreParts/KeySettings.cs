using ModLoader;
using SFS.Input;
using SFS.IO;
using SFS.Parsers.Json;
using UnityEngine;

namespace MorePartsMod
{
    public class KeySettings : ModKeybindings
    {
        public static KeySettings Main;

        public KeybindingsPC.Key Toggle_Telecommunication_Dish = KeyCode.Y;
        public KeybindingsPC.Key Open_Colony = KeyCode.U;
        public KeybindingsPC.Key Toggle_Telecommunication_Lines = KeyCode.I;
        public KeybindingsPC.Key Insert_Colony_Resources = KeyCode.J;
        public KeybindingsPC.Key Extract_Colony_Resources = KeyCode.K;
        public bool Show_Telecommunication_lines = true;

        public static void Setup()
        {
            Main = SetupKeybindings<KeySettings>(MorePartsPack.Main.Mod);
        }

        public override void CreateUI()
        {
            KeySettings keySettings = new KeySettings(); // default keybindings
            base.CreateUI_Text("MoreParts Mod");
            base.CreateUI_Keybinding(Toggle_Telecommunication_Dish, keySettings.Toggle_Telecommunication_Dish, "Toggle Telecommunication Dish");
            base.CreateUI_Keybinding(Open_Colony, keySettings.Open_Colony, "Open Colony");
            base.CreateUI_Keybinding(Toggle_Telecommunication_Lines, keySettings.Toggle_Telecommunication_Lines, "Toggle Telecommunication Lines");
            base.CreateUI_Keybinding(Insert_Colony_Resources, keySettings.Insert_Colony_Resources, "Insert resources to the colony");
            base.CreateUI_Keybinding(Extract_Colony_Resources, keySettings.Extract_Colony_Resources, "Extract resources from the colony");
        }

        public void ToggleShowTelecommunicationLines()
        {
            this.Show_Telecommunication_lines = !this.Show_Telecommunication_lines;
            this.Save();
        }

        private void Save()
        {
            string text = JsonWrapper.ToJson(this, true);
            this.GetPath(MorePartsPack.Main.Mod).WriteText(text);
        }

        private FilePath GetPath(Mod Mod)
        {
            return new FolderPath(Mod.ModFolder).ExtendToFile("keybindings.json");
        }
    }
}
