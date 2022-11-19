using ModLoader;
using SFS.Input;
using SFS.IO;
using SFS.Parsers.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MorePartsMod
{
	public class KeySettings : ModKeybindings
	{
		public static KeySettings Main;
		public static void Setup()
		{
			Main = SetupKeybindings<KeySettings>(MorePartsModMain.Main);
		}

		// Token: 0x0600027D RID: 637 RVA: 0x0000C74C File Offset: 0x0000A94C
		public override void CreateUI()
		{
			KeySettings keySettings = new KeySettings(); // default keybindings
			base.CreateUI_Text("MoreParts Mod");
			base.CreateUI_Keybinding(Toggle_Telecommunication_Dish, keySettings.Toggle_Telecommunication_Dish, "Toggle Telecommunication Dish");
			base.CreateUI_Keybinding(Open_Colony, keySettings.Open_Colony, "Open Colony");
			base.CreateUI_Keybinding(Toggle_Telecommunication_Lines, keySettings.Toggle_Telecommunication_Lines, "Toggle Telecommunication Lines");
			base.CreateUI_Keybinding(Toggle_Colony_Flow, keySettings.Toggle_Colony_Flow, "Toggle Colony Extraction");
		}

		public void ToggleShowTelecommunicationLines()
		{
			this.Show_Telecommunication_lines = !this.Show_Telecommunication_lines;
			this.Save();
		}

		private void Save()
		{
			string text = JsonWrapper.ToJson(this, true);
			this.GetPath(MorePartsModMain.Main).WriteText(text);
		}

		private FilePath GetPath(Mod mod)
		{
			return new FolderPath(mod.ModFolder).ExtendToFile("keybindings.json");
		}

		public KeybindingsPC.Key Toggle_Telecommunication_Dish = KeyCode.Y;
		public KeybindingsPC.Key Open_Colony = KeyCode.U;
		public KeybindingsPC.Key Toggle_Telecommunication_Lines = KeyCode.I;
		public KeybindingsPC.Key Toggle_Colony_Flow = KeyCode.O;
		public bool Show_Telecommunication_lines = true;
	}
}
