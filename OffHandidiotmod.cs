using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.UI;


namespace OffHandidiotmod
{
	public class OffHandidiotmod : Mod
	{
		public static UserInterface _myUserInterface;
		public static MySlotUI SlotUI;

		public override void Load()
		{
			// You can only display the UI to the local player -- prevent an error message!
			if (!Main.dedServ)
			{
				_myUserInterface = new UserInterface();
				SlotUI = new MySlotUI();

				SlotUI.Activate();
				_myUserInterface.SetState(SlotUI);
			}
		}

		public override void Unload()
		{
			// Ensure that you unload the UI's event handlers here
			SlotUI?.Unload();
		}
		internal static void SaveConfig(OffHandConfig configInstance) // yoinked from calamitymod
		{
			// There is no current way to manually save a mod configuration file in tModLoader.
			// The method which saves mod config files is private in ConfigManager, so reflection is used to invoke it.
			try
			{
				MethodInfo saveMethodInfo = typeof(ConfigManager).GetMethod("Save", BindingFlags.Static | BindingFlags.NonPublic);
				if (saveMethodInfo is not null)
				{
					saveMethodInfo.Invoke(null, [configInstance]);
				}
				else
				{
					string ContactDev = Language.GetTextValue("Mods.OffHandidiotmod.TextMessages.ContactDev");
					Main.NewText($"Modconfig save reflection failed. {ContactDev}");
				}
			}
			catch
			{
			}
		}

	}
	public class Activation : ModSystem
	{
		public static MethodInfo getCooldownsMethod;
		public static object calamityConfigInstance;
		public static PropertyInfo cooldownDisplayProperty;
		public override void PostSetupContent()
		{

			if (ModLoader.TryGetMod("CalamityMod", out Mod Calamity)) // Calamity mod
			{
				try
				{
					MySlotUI.Calamity = Calamity;

					//calamity cooldown method
					Type calamityUtilsType = Calamity.Code.GetType("CalamityMod.CalamityUtils");
					getCooldownsMethod = calamityUtilsType.GetMethod("GetDisplayedCooldowns", BindingFlags.Public | BindingFlags.Static);

					//calamity config's cooldownrack setting
					calamityConfigInstance = Calamity.GetConfig("CalamityClientConfig");
					cooldownDisplayProperty = calamityConfigInstance.GetType().GetProperty("CooldownDisplay");
				}
				catch (Exception exception)
				{
					MySlotUI.Calamity = null;
					Mod.Logger.Warn($"Error setting up calamity compatibility with offhand mod. {exception}");
				}
			}
			else
			{
				MySlotUI.Calamity = null;
			}

			if (ModLoader.TryGetMod("ImproveGame", out _)) // Quality of terraria mod that adds stupid ass 20 trash slots
			{
				MySlotUI.QoTEnabled = true;
			}
			else
			{
				MySlotUI.QoTEnabled = false;
			}


		}
		public override void UpdateUI(GameTime gameTime)
		{
			OffHandidiotmod._myUserInterface?.Update(gameTime);
		}

		// Make sure the UI can draw
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			// This will draw on the same layer as the inventory
			int inventoryLayer = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));

			if (inventoryLayer != -1)
			{
				layers.Insert(
					inventoryLayer,
					new LegacyGameInterfaceLayer("My Mod: My Slot UI", () =>
					{
						if (OffHandidiotmod.SlotUI.Visible)
						{
							OffHandidiotmod._myUserInterface.Draw(Main.spriteBatch, new GameTime());
						}

						return true;
					},
					InterfaceScaleType.UI));
			}
		}
		public static ModKeybind SwapKeybind { get; private set; }
		public static ModKeybind UseOffhandKeybind { get; private set; }

		public override void Load()
		{
			// Register new keybind
			SwapKeybind = KeybindLoader.RegisterKeybind(Mod, "Swap Offhand", "Q");
			UseOffhandKeybind = KeybindLoader.RegisterKeybind(Mod, "Use Offhand Item", "Mouse2");
		}

		public override void Unload()
		{
			SwapKeybind = null;
			UseOffhandKeybind = null;
		}
	}
	public class OffHandConfig : ModConfig
	{
		public static OffHandConfig Instance => ModContent.GetInstance<OffHandConfig>();
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[JsonProperty("PosYInventory")]
		[DefaultValue(260f)]
		private float _PosYInventory; // 260
		[JsonIgnore]
		public float PosYInventory { get => _PosYInventory; set => _PosYInventory = value; }

		[JsonProperty("PosYHUD")]
		[DefaultValue(79f)]
		private float _PosYHUD;// 79
		[JsonIgnore]
		public float PosYHUD { get => _PosYHUD; set => _PosYHUD = value; }

		[JsonProperty("PosXInventory")]
		[DefaultValue(20f)]
		private float _PosXInventory;// 20
		[JsonIgnore]
		public float PosXInventory { get => _PosXInventory; set => _PosXInventory = value; }

		[JsonProperty("PosXHUD")]
		[DefaultValue(25f)]
		private float _PosXHUD; // 25
		[JsonIgnore]
		public float PosXHUD { get => _PosXHUD; set => _PosXHUD = value; }

		[LabelKey("$Mods.OffHandidiotmod.Configs.OffHandConfig.Label")]
		[DefaultValue(true)]
		public bool ChatMessageToggle;

		[Header("$Mods.OffHandidiotmod.Configs.OffHandConfig.HUDHeader")]
		[TooltipKey("$Mods.OffHandidiotmod.Configs.OffHandConfig.SlotOffsetXHUD.Tooltip")]
		[DefaultValue(0)]
		[Range(-100, 100)]
		public int SlotOffsetXHUD;

		[TooltipKey("$Mods.OffHandidiotmod.Configs.OffHandConfig.SlotOffsetYHUD.Tooltip")]
		[DefaultValue(0)]
		[Range(-100, 100)]
		public int SlotOffsetYHUD;

		[Header("$Mods.OffHandidiotmod.Configs.OffHandConfig.InventoryHeader")]
		[TooltipKey("$Mods.OffHandidiotmod.Configs.OffHandConfig.SlotOffsetXInventory.Tooltip")]
		[DefaultValue(0)]
		[Range(-100, 100)]
		public int SlotOffsetXInventory;

		[TooltipKey("$Mods.OffHandidiotmod.Configs.OffHandConfig.SlotOffsetYInventory.Tooltip")]
		[DefaultValue(0)]
		[Range(-100, 100)]
		public int SlotOffsetYInventory;

		[Header("$Mods.OffHandidiotmod.Configs.OffHandConfig.DraggingHeader")]
		[LabelKey("$Mods.OffHandidiotmod.Configs.OffHandConfig.DraggingEnabled.Label")]
		[TooltipKey("$Mods.OffHandidiotmod.Configs.OffHandConfig.DraggingEnabled.Tooltip")]
		[DefaultValue(false)]
		public bool DraggingEnabled;

		[Header("$Mods.OffHandidiotmod.Configs.OffHandConfig.PriorityListHeader")]
		[LabelKey("$Mods.OffHandidiotmod.Configs.OffHandConfig.PriorityList.Label")]
		[TooltipKey("$Mods.OffHandidiotmod.Configs.OffHandConfig.PriorityList.Tooltip")]
		public List<ItemDefinition> ItemPriorityList { get; set; } = [];

		public override void OnChanged()
		{
			MyPlayer.priorityListChanged = true;
		}
	}
}