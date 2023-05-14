using UnityEngine;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallConnect;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.UserInterface;
using System.Collections.Generic;

namespace TrimmableArmor
{
    //Name Here
    public class ItemArmorTrimmingTool : DaggerfallUnityItem
    {
        public const int templateIndex = 4760;

        public ItemArmorTrimmingTool() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override bool IsStackable()
        {
            return false;
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemArmorTrimmingTool).ToString();
            return data;
        }

        public override bool UseItem(ItemCollection collection)
        {
            if (GameManager.Instance.AreEnemiesNearby())
            {
                DaggerfallUI.MessageBox("Can't use that with enemies around.");
                return true;
            }
            List<DaggerfallUnityItem> validTrimmableItems = new List<DaggerfallUnityItem>();
            UserInterfaceManager uiManager = DaggerfallUI.Instance.UserInterfaceManager;
            PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
            ItemCollection toolItemCollection = collection;

            DaggerfallListPickerWindow validArmorPicker = new DaggerfallListPickerWindow(uiManager, uiManager.TopWindow);
            validArmorPicker.OnItemPicked += ChooseArmorVariant_OnItemPicked;
            validTrimmableItems.Clear(); // Clears the valid item list before every trimming tool use.
            int itemCount = playerEntity.Items.Count;

            for (int i = 0; i < playerEntity.Items.Count; i++)
            {
                DaggerfallUnityItem item = playerEntity.Items.GetItem(i); // Start back from here tomorrow, after showering, eating, flossing, etc.
                if (item.ConditionPercentage < 80 && IsValidForRepair(item))
                {
                    validRepairItems.Add(item);
                    string validItemName = item.ConditionPercentage + "%" + "      " + item.LongName;
                    validItemPicker.ListBox.AddItem(validItemName);
                }
            }

            if (validItemPicker.ListBox.Count > 0)
                uiManager.PushWindow(validItemPicker);
            else
                DaggerfallUI.MessageBox("You have no valid items in need of repair.");

            return true;
        }

        // Method for calculations and work after a list item has been selected.
        public void ChooseArmorVariant_OnItemPicked(int index, string itemName)
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            DaggerfallUI.UIManager.PopWindow();
            PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
            DaggerfallUnityItem itemToRepair = validRepairItems[index]; // Gets the item object associated with what was selected in the list window.

            if (itemToRepair.currentCondition <= 0)
            {
                ShowCustomTextBox(false, itemToRepair, true, false); // Shows the specific text-box when trying to repair a completely broken item.
            }
            else
            {
                int luckMod = (int)Mathf.Round((playerEntity.Stats.LiveLuck - 50f) / 10);
                int endurMod = (int)Mathf.Round((playerEntity.Stats.LiveEndurance - 50f) / 10);
                int speedMod = (int)Mathf.Round((playerEntity.Stats.LiveSpeed - 50f) / 10);
                int agiliMod = (int)Mathf.Round((playerEntity.Stats.LiveAgility - 50f) / 10);
                int maxRepairThresh = (int)Mathf.Ceil(itemToRepair.maxCondition * (80 / 100f));
                int repairPercentage = GetRepairPercentage(luckMod, itemToRepair);
                int staminaDrainValue = GetStaminaDrain(endurMod);
                int TimeDrainValue = GetTimeDrain(speedMod, agiliMod);

                int repairAmount = (int)Mathf.Ceil(itemToRepair.maxCondition * (repairPercentage / 100f));
                if (itemToRepair.currentCondition + repairAmount > maxRepairThresh) // Checks if amount about to be repaired would go over the item's maximum allowed condition threshold.
                {   // If true, repair amount will instead set the item's current condition to the defined maximum threshold.
                    itemToRepair.currentCondition = maxRepairThresh;
                }
                else
                {   // Does the actual repair, by adding condition damage to the current item's current condition value.
                    itemToRepair.currentCondition += repairAmount;
                }
                bool toolBroke = currentCondition <= DurabilityLoss;
                bool useAltText = ShouldUseAlternateText(itemToRepair.TemplateIndex);
                LowerConditionWorkaround(DurabilityLoss, playerEntity, repairItemCollection); // Damages repair tool condition.

                // Force inventory window update
                DaggerfallUI.Instance.InventoryWindow.Refresh();

                PlayAudioTrack(); // Plays the appropriate sound effect for a specific repair tool.
                playerEntity.DecreaseFatigue(staminaDrainValue, true); // Reduce player current stamina value from the action of repairing.
                if (RepairToolsMain.TimeCostSettingCheck) { DaggerfallUnity.Instance.WorldTime.Now.RaiseTime(TimeDrainValue); } // Forwards time by an amount of minutes in-game time.
                ShowCustomTextBox(toolBroke, itemToRepair, false, useAltText); // Shows the specific text-box after repairing an item.
            }
        }

        public override bool IsValidForRepair(DaggerfallUnityItem item)
        {
            if (RepairToolsMain.RestrictedMaterialsCheck)
            {
                return !item.IsEnchanted && !item.IsArtifact && item.ItemGroup == ItemGroups.Armor && item.NativeMaterialValue >= (int)ArmorMaterialTypes.Iron &&
                    !(item.NativeMaterialValue == (int)ArmorMaterialTypes.Ebony || item.NativeMaterialValue == (int)ArmorMaterialTypes.Orcish || item.NativeMaterialValue == (int)ArmorMaterialTypes.Daedric);
            }
            else
            {
                return !item.IsEnchanted && !item.IsArtifact && item.ItemGroup == ItemGroups.Armor && item.NativeMaterialValue >= (int)ArmorMaterialTypes.Iron;
            }
        }
    }
}

