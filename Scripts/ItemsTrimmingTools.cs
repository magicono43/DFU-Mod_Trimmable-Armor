using UnityEngine;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.UserInterface;
using System.Collections.Generic;
using DaggerfallWorkshop;
using DaggerfallConnect.Arena2;

namespace TrimmableArmor
{
    //Graving Chisel
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

        UserInterfaceManager uiManager = DaggerfallUI.Instance.UserInterfaceManager;
        PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
        ItemCollection trimmingToolCollection;
        List<DaggerfallUnityItem> ValidTrimmableItems = new List<DaggerfallUnityItem>();
        DaggerfallUnityItem trimThisItem = null;

        public override bool UseItem(ItemCollection collection)
        {
            if (GameManager.Instance.AreEnemiesNearby())
            {
                DaggerfallUI.MessageBox("Can't use that with enemies around.");
                return true;
            }
            trimmingToolCollection = collection;

            DaggerfallListPickerWindow validArmorPicker = new DaggerfallListPickerWindow(uiManager, uiManager.TopWindow);
            validArmorPicker.OnItemPicked += ShowArmorVariantPicker_OnItemPicked;
            ValidTrimmableItems.Clear(); // Clears the valid item list before every trimming tool use.
            int itemCount = playerEntity.Items.Count;

            for (int i = 0; i < playerEntity.Items.Count; i++)
            {
                DaggerfallUnityItem item = playerEntity.Items.GetItem(i);
                if (!item.IsArtifact && !item.IsQuestItem && item.TimeForItemToDisappear == 0 && item.ItemGroup == ItemGroups.Armor && item.NativeMaterialValue >= (int)ArmorMaterialTypes.Iron)
                {
                    ValidTrimmableItems.Add(item);
                    string validItemName = "";
                    if (item.IsEquipped) { validItemName = "(Equipped)" + "      " + item.LongName; }
                    else { validItemName = "      " + item.LongName; }
                    validArmorPicker.ListBox.AddItem(validItemName);
                }
            }

            if (validArmorPicker.ListBox.Count > 0)
                uiManager.PushWindow(validArmorPicker);
            else
                DaggerfallUI.MessageBox("You have no valid armor pieces to trim.");

            return true;
        }
        
        List<string> TrimVariantChoices = new List<string>();

        public void ShowArmorVariantPicker_OnItemPicked(int index, string itemName)
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            DaggerfallUI.UIManager.PopWindow();
            DaggerfallUnityItem itemToChooseVariant = ValidTrimmableItems[index]; // Gets the item object associated with what was selected in the list window.
            trimThisItem = itemToChooseVariant;

            DaggerfallListPickerWindow armorVariantPicker = new DaggerfallListPickerWindow(uiManager, uiManager.TopWindow);
            armorVariantPicker.OnItemPicked += TrimArmor_OnItemPicked;
            TrimVariantChoices.Clear(); // Clears the valid item list before every trimming tool use.
            TrimVariantChoices = GetVariantNameList(itemToChooseVariant);

            if (TrimVariantChoices.Count <= 1)
            {
                DaggerfallUI.MessageBox("This piece does not have any additional visual variants.");
            }
            else
            {
                for (int i = 0; i < TrimVariantChoices.Count; i++)
                {
                    string entryName = "";
                    if (FindCurrentVariant(itemToChooseVariant) == i) { entryName = "(Current) " + TrimVariantChoices[i]; }
                    else { entryName = TrimVariantChoices[i]; }
                    armorVariantPicker.ListBox.AddItem(entryName);
                }
                uiManager.PushWindow(armorVariantPicker);
            }
        }

        public void TrimArmor_OnItemPicked(int index, string itemName)
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            DaggerfallUI.UIManager.PopWindow();
            int selectedVariant = index; // Gets the visual variant index value based on what was selected in the list window.

            if (trimThisItem == null || FindCurrentVariant(trimThisItem) == -1)
            {
                DaggerfallUI.MessageBox("This piece is not valid.");
            }
            else if (FindCurrentVariant(trimThisItem) == index)
            {
                DaggerfallUI.MessageBox("This piece is already using that visual variant.");
            }
            else
            {
                DaggerfallUnityItem newItem = CreateNewVariantItem(trimThisItem, index);
                if (newItem != null)
                {
                    if (trimThisItem.IsEquipped)
                    {
                        trimThisItem.UnequipItem(playerEntity);
                        playerEntity.ItemEquipTable.EquipItem(newItem);
                        playerEntity.Items.RemoveItem(trimThisItem);
                        playerEntity.Items.AddItem(newItem);
                    }
                    else
                    {
                        playerEntity.Items.RemoveItem(trimThisItem);
                        playerEntity.Items.AddItem(newItem);
                    }
                    bool toolBroke = currentCondition <= 10;
                    LowerConditionWorkaround(10, playerEntity, trimmingToolCollection); // Damages trimming tool condition.
                    ShowCustomTextBox(toolBroke, newItem); // Shows the specific text-box after trimming an item.
                }
                else
                {
                    DaggerfallUI.MessageBox("That variant could not be created.");
                }
            }

            ValidTrimmableItems.Clear();
            TrimVariantChoices.Clear();
            trimThisItem = null;
            DaggerfallUI.Instance.InventoryWindow.Refresh();
        }

        public static List<string> GetVariantNameList(DaggerfallUnityItem item)
        {
            List<string> names = new List<string>();
            Genders gender = GameManager.Instance.PlayerEntity.Gender;
            Races race = GameManager.Instance.PlayerEntity.Race;

            switch ((int)item.TemplateIndex)
            {
                case (int)Armor.Cuirass:
                    if (gender == Genders.Female)
                    {
                        names.Add("1." + "      " + "Full Skirted Breastplate");
                        names.Add("2." + "      " + "Segmented Breastplate");
                        names.Add("3." + "      " + "Chain-Skirted Half-Breastplate");
                    }
                    else
                    {
                        names.Add("1." + "      " + "Shaped Breastplate");
                        names.Add("2." + "      " + "Smooth Breastplate");
                        names.Add("3." + "      " + "Full Skirted Breastplate");
                    } break;
                case (int)Armor.Greaves:
                    names.Add("1." + "      " + "Smooth Plate Greaves");
                    names.Add("2." + "      " + "Chain-Skirted Smooth Plate Greaves");
                    names.Add("3." + "      " + "Segmented Plate Greaves");
                    names.Add("4." + "      " + "Chain-Skirted Segmented Plate Greaves");
                    break;
                case (int)Armor.Left_Pauldron:
                case (int)Armor.Right_Pauldron:
                    names.Add("1." + "      " + "Plate Pauldron");
                    names.Add("2." + "      " + "Collared Plate Pauldron");
                    names.Add("3." + "      " + "Collared Gold-Trimmed Plate Pauldron");
                    break;
                case (int)Armor.Helm:
                    names.Add("1." + "      " + "Spangenhelm");
                    names.Add("2." + "      " + "Norman Nasal Helm");
                    names.Add("3." + "      " + "Barbute Helm");
                    names.Add("4." + "      " + "Red Crested Galea");
                    names.Add("5." + "      " + "Armet With Blue Tassel");
                    names.Add("6." + "      " + "Armet With Horns");
                    break;
                case (int)Armor.Boots:
                    names.Add("1." + "      " + "Plate Boots");
                    names.Add("2." + "      " + "Gold-Trimmed Plate Boots");
                    break;
                case (int)Armor.Gauntlets:
                case (int)Armor.Buckler:
                case (int)Armor.Round_Shield:
                case (int)Armor.Kite_Shield:
                case (int)Armor.Tower_Shield:
                default:
                    break;
            }
            return names;
        }

        public static int FindCurrentVariant(DaggerfallUnityItem item)
        {
            Genders gender = GameManager.Instance.PlayerEntity.Gender;

            switch ((int)item.TemplateIndex)
            {
                case (int)Armor.Cuirass:
                    if (gender == Genders.Female)
                    {
                        if (item.InventoryTextureRecord == 4) { return 0; }
                        else if (item.InventoryTextureRecord == 5) { return 1; }
                        else if (item.InventoryTextureRecord == 6) { return 2; }
                        else { return -1; }
                    }
                    else
                    {
                        if (item.InventoryTextureRecord == 4) { return 0; }
                        else if (item.InventoryTextureRecord == 5) { return 1; }
                        else if (item.InventoryTextureRecord == 6) { return 2; }
                        else { return -1; }
                    }
                case (int)Armor.Greaves:
                    if (item.InventoryTextureRecord == 12) { return 0; }
                    else if (item.InventoryTextureRecord == 13) { return 1; }
                    else if (item.InventoryTextureRecord == 14) { return 2; }
                    else if (item.InventoryTextureRecord == 15) { return 3; }
                    else { return -1; }
                case (int)Armor.Left_Pauldron:
                case (int)Armor.Right_Pauldron:
                    if (item.InventoryTextureRecord == 18 || item.InventoryTextureRecord == 23) { return 0; }
                    else if (item.InventoryTextureRecord == 19 || item.InventoryTextureRecord == 24) { return 1; }
                    else if (item.InventoryTextureRecord == 20 || item.InventoryTextureRecord == 25) { return 2; }
                    else { return -1; }
                case (int)Armor.Helm:
                    if (item.InventoryTextureRecord == 27) { return 0; }
                    else if (item.InventoryTextureRecord == 28) { return 1; }
                    else if (item.InventoryTextureRecord == 29) { return 2; }
                    else if (item.InventoryTextureRecord == 30) { return 3; }
                    else if (item.InventoryTextureRecord == 31) { return 4; }
                    else if (item.InventoryTextureRecord == 32) { return 5; }
                    else { return -1; }
                case (int)Armor.Boots:
                    if (item.InventoryTextureRecord == 1) { return 0; }
                    else if (item.InventoryTextureRecord == 2) { return 1; }
                    else { return -1; }
                case (int)Armor.Gauntlets:
                case (int)Armor.Buckler:
                case (int)Armor.Round_Shield:
                case (int)Armor.Kite_Shield:
                case (int)Armor.Tower_Shield:
                default:
                    return -1;
            }
        }

        public static DaggerfallUnityItem CreateNewVariantItem(DaggerfallUnityItem oldItem, int variant)
        {
            DaggerfallUnityItem newItem = null;
            Genders gender = GameManager.Instance.PlayerEntity.Gender;
            Races race = GameManager.Instance.PlayerEntity.Race;

            switch ((int)oldItem.TemplateIndex)
            {
                case (int)Armor.Cuirass:
                    if (gender == Genders.Female)
                    {
                        if (variant == 0) { newItem = ItemBuilder.CreateArmor(gender, race, Armor.Cuirass, (ArmorMaterialTypes)oldItem.nativeMaterialValue, 1); }
                        else if (variant == 1) { newItem = ItemBuilder.CreateArmor(gender, race, Armor.Cuirass, (ArmorMaterialTypes)oldItem.nativeMaterialValue, 2); }
                        else if (variant == 2) { newItem = ItemBuilder.CreateArmor(gender, race, Armor.Cuirass, (ArmorMaterialTypes)oldItem.nativeMaterialValue, 3); }
                        break;
                    }
                    else
                    {
                        if (variant == 0) { newItem = ItemBuilder.CreateArmor(gender, race, Armor.Cuirass, (ArmorMaterialTypes)oldItem.nativeMaterialValue, 1); }
                        else if (variant == 1) { newItem = ItemBuilder.CreateArmor(gender, race, Armor.Cuirass, (ArmorMaterialTypes)oldItem.nativeMaterialValue, 2); }
                        else if (variant == 2) { newItem = ItemBuilder.CreateArmor(gender, race, Armor.Cuirass, (ArmorMaterialTypes)oldItem.nativeMaterialValue, 3); }
                        break;
                    }
                case (int)Armor.Greaves:
                    if (variant == 0) { newItem = ItemBuilder.CreateArmor(gender, race, Armor.Greaves, (ArmorMaterialTypes)oldItem.nativeMaterialValue, 2); }
                    else if (variant == 1) { newItem = ItemBuilder.CreateArmor(gender, race, Armor.Greaves, (ArmorMaterialTypes)oldItem.nativeMaterialValue, 3); }
                    else if (variant == 2) { newItem = ItemBuilder.CreateArmor(gender, race, Armor.Greaves, (ArmorMaterialTypes)oldItem.nativeMaterialValue, 4); }
                    else if (variant == 3) { newItem = ItemBuilder.CreateArmor(gender, race, Armor.Greaves, (ArmorMaterialTypes)oldItem.nativeMaterialValue, 5); }
                    break;
                case (int)Armor.Left_Pauldron:
                    if (variant == 0) { newItem = ItemBuilder.CreateArmor(gender, race, Armor.Left_Pauldron, (ArmorMaterialTypes)oldItem.nativeMaterialValue, 1); }
                    else if (variant == 1) { newItem = ItemBuilder.CreateArmor(gender, race, Armor.Left_Pauldron, (ArmorMaterialTypes)oldItem.nativeMaterialValue, 2); }
                    else if (variant == 2) { newItem = ItemBuilder.CreateArmor(gender, race, Armor.Left_Pauldron, (ArmorMaterialTypes)oldItem.nativeMaterialValue, 3); }
                    break;
                case (int)Armor.Right_Pauldron:
                    if (variant == 0) { newItem = ItemBuilder.CreateArmor(gender, race, Armor.Right_Pauldron, (ArmorMaterialTypes)oldItem.nativeMaterialValue, 1); }
                    else if (variant == 1) { newItem = ItemBuilder.CreateArmor(gender, race, Armor.Right_Pauldron, (ArmorMaterialTypes)oldItem.nativeMaterialValue, 2); }
                    else if (variant == 2) { newItem = ItemBuilder.CreateArmor(gender, race, Armor.Right_Pauldron, (ArmorMaterialTypes)oldItem.nativeMaterialValue, 3); }
                    break;
                case (int)Armor.Helm:
                    if (variant == 0) { newItem = ItemBuilder.CreateArmor(gender, race, Armor.Helm, (ArmorMaterialTypes)oldItem.nativeMaterialValue, 0); }
                    else if (variant == 1) { newItem = ItemBuilder.CreateArmor(gender, race, Armor.Helm, (ArmorMaterialTypes)oldItem.nativeMaterialValue, 1); }
                    else if (variant == 2) { newItem = ItemBuilder.CreateArmor(gender, race, Armor.Helm, (ArmorMaterialTypes)oldItem.nativeMaterialValue, 2); }
                    else if (variant == 3) { newItem = ItemBuilder.CreateArmor(gender, race, Armor.Helm, (ArmorMaterialTypes)oldItem.nativeMaterialValue, 3); }
                    else if (variant == 4) { newItem = ItemBuilder.CreateArmor(gender, race, Armor.Helm, (ArmorMaterialTypes)oldItem.nativeMaterialValue, 4); }
                    else if (variant == 5) { newItem = ItemBuilder.CreateArmor(gender, race, Armor.Helm, (ArmorMaterialTypes)oldItem.nativeMaterialValue, 5); }
                    break;
                case (int)Armor.Boots:
                    if (variant == 0) { newItem = ItemBuilder.CreateArmor(gender, race, Armor.Boots, (ArmorMaterialTypes)oldItem.nativeMaterialValue, 1); }
                    else if (variant == 1) { newItem = ItemBuilder.CreateArmor(gender, race, Armor.Boots, (ArmorMaterialTypes)oldItem.nativeMaterialValue, 2); }
                    break;
                case (int)Armor.Gauntlets:
                case (int)Armor.Buckler:
                case (int)Armor.Round_Shield:
                case (int)Armor.Kite_Shield:
                case (int)Armor.Tower_Shield:
                default:
                    break;
            }

            if (oldItem != null && newItem != null) // copy attributes from original item to new one, if possible.
            {
                newItem.shortName = oldItem.shortName;
                newItem.dyeColor = oldItem.dyeColor;
                newItem.weightInKg = oldItem.weightInKg;
                newItem.value = oldItem.value;
                newItem.unknown = oldItem.unknown;
                newItem.flags = oldItem.flags;
                newItem.currentCondition = oldItem.currentCondition;
                newItem.maxCondition = oldItem.maxCondition;
                newItem.unknown2 = oldItem.unknown2;
                newItem.enchantmentPoints = oldItem.enchantmentPoints;
                newItem.message = oldItem.message;
                newItem.legacyMagic = oldItem.legacyMagic;
                newItem.customMagic = oldItem.customMagic;
                newItem.TrappedSoulType = oldItem.TrappedSoulType;
                newItem.poisonType = oldItem.poisonType;
                newItem.PotionRecipeKey = oldItem.PotionRecipeKey;
                newItem.timeHealthLeechLastUsed = oldItem.timeHealthLeechLastUsed;
            }
            return newItem;
        }

        // Creates the custom text-box after trimming an item.
        public void ShowCustomTextBox(bool toolBroke, DaggerfallUnityItem itemToTrim)
        {
            TextFile.Token[] tokens = null;
            if (toolBroke)
            {
                tokens = DaggerfallUnity.Instance.TextProvider.CreateTokens(
                            TextFile.Formatting.JustifyCenter,
                            "You change the appearance of the,",
                            itemToTrim.LongName + ".",
                            "",
                            "The Graving Chisel's blade is too blunted to dig into metal with,",
                            "you discard it.");
            }
            else
            {
                tokens = DaggerfallUnity.Instance.TextProvider.CreateTokens(
                            TextFile.Formatting.JustifyCenter,
                            "You change the appearance of the,",
                            itemToTrim.LongName + ".");
            }
            DaggerfallMessageBox itemTrimmedText = new DaggerfallMessageBox(DaggerfallUI.UIManager, DaggerfallUI.UIManager.TopWindow);
            itemTrimmedText.SetTextTokens(tokens);
            itemTrimmedText.ClickAnywhereToClose = true;
            uiManager.PushWindow(itemTrimmedText);
        }

        // Like DaggerfallUnityItem's LowerCondition, but without taking DaggerfallUnity.Settings.AllowMagicRepairs into account
        public void LowerConditionWorkaround(int amount, DaggerfallEntity unequipFromOwner = null, ItemCollection removeFromCollectionWhenBreaks = null)
        {
            currentCondition -= amount;
            if (currentCondition <= 0)
            {
                currentCondition = 0;
                ItemBreaks(unequipFromOwner);
                removeFromCollectionWhenBreaks.RemoveItem(this);
            }
        }
    }
}

