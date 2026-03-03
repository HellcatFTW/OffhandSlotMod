using CustomSlot;
using CustomSlot.UI;
using Terraria;
using Terraria.UI;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Localization;
using System;
using System.Linq;
using System.Collections;
using Microsoft.Xna.Framework;

namespace OffHandidiotmod
{
    public class MySlotUI : UIState
    {
        public static bool QoTEnabled;
        public static Mod Calamity;
        public class SomethingSlot : CustomItemSlot
        {
            public int cooldownCount;
            private double emptyBuffAmount = 0;
            private int rowGap = 0;
            private const int rowSize = 43;
            private int buffRows = 0;
            private int goblinOffset = 0;
            private const int defaultPosYHUD = 79;
            private const int defaultPosXHUD = 25;
            private const int defaultPosYInventory = 260;
            private const int defaultPosXInventory = 20;
            private const int PosXHUDIgnoreThreshold = 450;
            private int PosXInventoryIgnoreThreshold = 0;
            private int PosYInventoryIgnoreThreshold = 0;
            private const int PosYJourneyIgnoreThreshold = 310;
            private int journeyOffsetX = 50;
            private int journeyOffsetY = 0;
            private int calamityOffsetY = 0;
            private int calamityOffsetX = 0;
            private int qotOffsetY = 0;
            private int qotOffsetX = 0;
            private int currentDynamicOffsetY;
            private int OffsetYInventory { get => OffHandConfig.Instance.SlotOffsetYInventory; set => OffHandConfig.Instance.SlotOffsetYInventory = value; }
            private int OffsetYHUD { get => OffHandConfig.Instance.SlotOffsetYHUD; set => OffHandConfig.Instance.SlotOffsetYHUD = value; }
            private int OffsetXInventory { get => OffHandConfig.Instance.SlotOffsetXInventory; set => OffHandConfig.Instance.SlotOffsetXInventory = value; }
            private int OffsetXHUD { get => OffHandConfig.Instance.SlotOffsetXHUD; set => OffHandConfig.Instance.SlotOffsetXHUD = value; }
            public bool DraggingEnabled { get => OffHandConfig.Instance.DraggingEnabled; }
            public float PosYInventory { get => OffHandConfig.Instance.PosYInventory; set => OffHandConfig.Instance.PosYInventory = value; } // 260
            public float PosYHUD { get => OffHandConfig.Instance.PosYHUD; set => OffHandConfig.Instance.PosYHUD = value; } // 79
            public float PosXInventory { get => OffHandConfig.Instance.PosXInventory; set => OffHandConfig.Instance.PosXInventory = value; } // 20
            public float PosXHUD { get => OffHandConfig.Instance.PosXHUD; set => OffHandConfig.Instance.PosXHUD = value; } // 25
            public bool dragging = false;
            public Vector2 dragoffset;

            public SomethingSlot() : base(ItemSlot.Context.InventoryItem, 0.85f)
            {
                IsValidItem = item => item.type > ItemID.None && !ItemID.Sets.Torches[item.type] && !ItemID.Sets.Glowsticks[item.type];
            }

            public int? getCalamityCooldowns()
            {
                if (Calamity == null)
                {
                    return null;
                }
                try
                {
                    int cooldownDisplaySetting = (int) Activation.cooldownDisplayProperty.GetValue(Activation.calamityConfigInstance);

                    if (cooldownDisplaySetting > 1)
                    {
                        return null;
                    }

                    object result = (IList)Activation.getCooldownsMethod.Invoke(null, [Main.LocalPlayer]);

                    if (result is IList cooldowns)
                    {
                        return cooldowns.Count;
                    }
                    else
                    {
                        Calamity = null;
                        string ContactDev = Language.GetTextValue("Mods.OffHandidiotmod.TextMessages.ContactDev");
                        Main.NewText($"Code 127: {ContactDev}");
                    }
                    return null;
                }
                catch (Exception)
                {
                    Calamity = null;
                    return null;
                }
            }

            public void ApplyConfigs()
            {
                if (OffsetXHUD != 0 || OffsetYHUD != 0 || OffsetXInventory != 0 || OffsetYInventory != 0)
                {
                    PosXHUD += OffsetXHUD;
                    PosYHUD += OffsetYHUD;
                    PosXInventory += OffsetXInventory;
                    PosYInventory += OffsetYInventory;
                    OffsetXHUD = 0;
                    OffsetYHUD = 0;
                    OffsetXInventory = 0;
                    OffsetYInventory = 0;
                    OffHandidiotmod.SaveConfig(OffHandConfig.Instance);
                }
            }

            protected override void DrawSelf(SpriteBatch spriteBatch)
            {
                string SlotHoverText = Language.GetTextValue("Mods.OffHandidiotmod.SlotHoverText");
                HoverText = SlotHoverText;
                ItemRarity = ItemRarityID.White;

                // when player is hovering over item in hotbar state, and hotbar isnt locked, and there's an item in the slot
                if (!Main.LocalPlayer.ItemAnimationActive && !Main.LocalPlayer.hbLocked && RMBSlot.item.type > ItemID.None && !Main.playerInventory)
                {
                    HoverText = RMBSlot.Item.AffixName();
                    ItemRarity = RMBSlot.item.rare;
                    if (RMBSlot.Item.stack > 1)
                        HoverText = HoverText + " (" + RMBSlot.Item.stack + ")";
                }
                else
                {
                    HoverText = "";
                    ItemRarity = null;
                }

                if (DraggingEnabled)
                {
                    Drag();
                }

                RMBSlot.SetPos(CalcDisplayedPos().X,
                               CalcDisplayedPos().Y);

                base.DrawSelf(spriteBatch);
            }// end of drawself override


            public Vector2 CalcDisplayedPos()
            {
                float resultX;
                float resultY;
                ApplyConfigs();

                emptyBuffAmount = Main.LocalPlayer.buffTime.Count(0); // includes modded buffs too :D

                buffRows = (int)Math.Ceiling((Main.LocalPlayer.buffTime.Length - emptyBuffAmount) / 11); // divide buffs by 11 which is amount of buffs in a row

                if (buffRows > 1)
                {
                    rowGap = 4;
                }
                else
                {
                    rowGap = 0;
                }

                if (getCalamityCooldowns() != null && getCalamityCooldowns() != 0) // Calamitymod cooldown timers offset
                {
                    calamityOffsetY = 57;
                    calamityOffsetX = 7;
                }
                else
                {
                    calamityOffsetY = 0;
                    calamityOffsetX = 0;
                }

                if (QoTEnabled)
                {
                    qotOffsetY = 50;
                    qotOffsetX = 48;
                    PosXInventoryIgnoreThreshold = 335;
                    PosYInventoryIgnoreThreshold = 310;
                }
                else
                {
                    qotOffsetY = 0;
                    qotOffsetX = 0;
                    PosXInventoryIgnoreThreshold = 0;
                    PosYInventoryIgnoreThreshold = 0;
                }

                if (Main.InReforgeMenu) // goblin tinkerer reforge slot offset
                {
                    goblinOffset = 60;
                }
                else
                {
                    goblinOffset = 0;
                }

                if (Main.LocalPlayer.difficulty == 3)
                {
                    journeyOffsetX = 50;
                    journeyOffsetY = 50;
                }
                else
                {
                    journeyOffsetX = 0;
                    journeyOffsetY = 0;
                }

                currentDynamicOffsetY = (buffRows * rowSize) + (rowGap * buffRows) + calamityOffsetY; // buffs + calamity offset for HUD

                // Sets position for inventory
                if (Main.playerInventory)
                {
                    // Sets position for inventory
                    if (Main.LocalPlayer.difficulty != 3) // Regular modes
                    {
                        if (Main.npcShop == 0) // not in a shop
                        {
                            if (Main.npcShop == 0 && !Main.InReforgeMenu) // regular inventory
                            {
                                if (PosXInventory == defaultPosXInventory && PosYInventory == defaultPosYInventory) // slot is at default pos
                                {
                                    resultX = defaultPosXInventory + qotOffsetX;
                                    resultY = defaultPosYInventory + qotOffsetY;
                                }
                                else // slot has been moved by user
                                {
                                    if (PosXInventory < PosXInventoryIgnoreThreshold && PosYInventory < PosYInventoryIgnoreThreshold)
                                    {
                                        resultX = PosXInventory;
                                        resultY = defaultPosYInventory + qotOffsetY;
                                    }
                                    else
                                    {
                                        resultX = PosXInventory;
                                        resultY = PosYInventory;
                                    }
                                }
                            }
                            else // shop / reforging, 
                            {
                                if (Main.InReforgeMenu)
                                {
                                    resultX = defaultPosXInventory;
                                    resultY = defaultPosYInventory + goblinOffset;
                                }
                                else
                                {
                                    resultX = defaultPosXInventory;
                                    resultY = defaultPosYInventory;
                                }
                            }
                        }
                        else // is in a shop, force default
                        {
                            resultX = defaultPosXInventory;
                            resultY = defaultPosYInventory;
                        }
                    }
                    else // In journey mode
                    {
                        if (Main.npcShop == 0 && !Main.InReforgeMenu) // regular inventory, use user position and offset for journey menu toggle
                        {
                            if (PosXInventory == defaultPosXInventory && PosYInventory == defaultPosYInventory) // slot is at default pos
                            {
                                resultX = defaultPosXInventory + journeyOffsetX;
                                resultY = defaultPosYInventory + qotOffsetY;
                            }
                            else // slot has been moved by user
                            {
                                if (QoTEnabled) // quality of terraria enabled, 50 pixel addition makes it perfectly touch the cog
                                {
                                    if (PosXInventory < PosXInventoryIgnoreThreshold + 50 && PosYInventory < PosYJourneyIgnoreThreshold)
                                    {
                                        resultX = PosXInventory;
                                        resultY = defaultPosYInventory + journeyOffsetY;
                                    }
                                    else
                                    {
                                        resultX = PosXInventory;
                                        resultY = PosYInventory;
                                    }
                                }
                                else // vanilla journey UI, 60 pixels clears the menu toggle nicely
                                {
                                    if (PosXInventory < 60 && PosYInventory < PosYJourneyIgnoreThreshold)
                                    {
                                        resultX = PosXInventory;
                                        resultY = defaultPosYInventory + journeyOffsetY;
                                    }
                                    else
                                    {
                                        resultX = PosXInventory;
                                        resultY = PosYInventory;
                                    }
                                }
                            }
                        }
                        else // shop / reforging,
                        {
                            if (Main.InReforgeMenu) // reforging, force default
                            {
                                resultX = defaultPosXInventory;
                                resultY = defaultPosYInventory + goblinOffset;
                            }
                            else // shop is open, force default
                            {
                                resultX = defaultPosXInventory;
                                resultY = defaultPosYInventory;
                            }
                        }
                    }
                }




                // Sets position for HUD
                else
                {
                    // Sets position for HUD
                    if (PosYHUD == defaultPosYHUD && PosXHUD == defaultPosXHUD) // HUD is default position, add dynamic offset
                    {
                        resultX = defaultPosXHUD + calamityOffsetX;
                        resultY = defaultPosYHUD + currentDynamicOffsetY;
                    }
                    else // slot position is not default
                    {
                        if (PosXHUD >= PosXHUDIgnoreThreshold) // Special case that slot is set to right of hotbar, no need to lower it with buffs.
                        {
                            resultX = PosXHUD + calamityOffsetX;  // leave calamityoffset there for consistency
                            resultY = PosYHUD;
                        }
                        else // X is not at ignore threshold and positions are not default
                        {
                            if (PosYHUD > currentDynamicOffsetY + defaultPosYHUD) // Config position is outside current active buffs area
                            {
                                resultX = PosXHUD + calamityOffsetX;
                                resultY = PosYHUD;
                            }
                            else // config position is IN current active buffs area, 
                            {
                                resultX = PosXHUD + calamityOffsetX;
                                resultY = defaultPosYHUD + currentDynamicOffsetY;
                            }
                        }
                    }
                }


                Vector2 vector = new Vector2(resultX, resultY);
                return vector;
            }//end of calcpos

            public void Drag()
            {
                if (ContainsPoint(Main.MouseScreen))
                {
                    Main.LocalPlayer.mouseInterface = true;
                }

                if (dragging && Main.playerInventory && Main.npcShop == 0 && !Main.InReforgeMenu)
                {
                    PosXInventory = Main.mouseX - dragoffset.X;
                    PosYInventory = Main.mouseY - dragoffset.Y;
                }
                else if (dragging && !Main.playerInventory && Main.npcShop == 0 && !Main.InReforgeMenu)
                {
                    PosXHUD = Main.mouseX - dragoffset.X;
                    PosYHUD = Main.mouseY - dragoffset.Y;
                }
            }
        }// end of class

        public override void LeftMouseDown(UIMouseEvent evt)
        {
            if (Main.npcShop == 0 && !Main.InReforgeMenu)
                base.LeftMouseDown(evt);

            if (!RMBSlot.DraggingEnabled) return;

            if (RMBSlot.ContainsPoint(evt.MousePosition))
            {
                DragBegin(evt);
            }
        }

        public override void LeftMouseUp(UIMouseEvent evt)
        {

            base.LeftMouseUp(evt);

            if (!RMBSlot.DraggingEnabled) return;

            if (RMBSlot.dragging)
            {
                DragEnd(evt);
                OffHandidiotmod.SaveConfig(OffHandConfig.Instance);
            }
        }
        private void DragBegin(UIMouseEvent e)
        {
            RMBSlot.dragoffset = new Vector2(e.MousePosition.X - RMBSlot.Left.Pixels, e.MousePosition.Y - RMBSlot.Top.Pixels);
            RMBSlot.dragging = true;
        }

        private void DragEnd(UIMouseEvent e)
        {
            Vector2 end = e.MousePosition;
            RMBSlot.dragging = false;
            if (Main.playerInventory && Main.npcShop == 0 && !Main.InReforgeMenu)
            {
                RMBSlot.PosXInventory = end.X - RMBSlot.dragoffset.X;
                RMBSlot.PosYInventory = end.Y - RMBSlot.dragoffset.Y;
            }
            else if (!Main.playerInventory && Main.npcShop == 0 && !Main.InReforgeMenu)
            {
                RMBSlot.PosXHUD = end.X - RMBSlot.dragoffset.X;
                RMBSlot.PosYHUD = end.Y - RMBSlot.dragoffset.Y;
            }
        }



        public static SomethingSlot RMBSlot;

        public bool Visible = true;

        public override void OnInitialize()
        {
            RMBSlot = new SomethingSlot();
            Append(RMBSlot);


            // If you're going to hook into CustomItemSlot events, put them here, then unload them during MyMod.Unload()
            RMBSlot.ItemChanged += ItemChanged;
        }

        private void ItemChanged(CustomItemSlot slot, ItemChangedEventArgs e)
        {
            // It's usually best to "encapsulate" data: that is, let the class that owns it handle it, while calling only
            // public functions
            Main.LocalPlayer.GetModPlayer<MyCustomSlotPlayer>().ItemChanged(slot, e);
        }

        // Unload the class by removing its event handlers
        internal void Unload()
        {
            RMBSlot.ItemChanged -= ItemChanged;
        }
    }
}