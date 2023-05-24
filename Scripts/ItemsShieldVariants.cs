using UnityEngine;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Serialization;

namespace TrimmableArmor
{
    //Concave Buckler
    public class ItemCustomBucklerOne : DaggerfallUnityItem
    {
        public const int templateIndex = 4761;

        public ItemCustomBucklerOne() : base(ItemGroups.Armor, templateIndex)
        {
        }

        public override EquipSlots GetEquipSlot()
        {
            return EquipSlots.LeftHand;
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemArmorTrimmingTool).ToString();
            return data;
        }
    }

    //Patterned Round Shield
    public class ItemArenaRoundShieldFemale : DaggerfallUnityItem
    {
        public const int templateIndex = 4762;

        public ItemArenaRoundShieldFemale() : base(ItemGroups.Armor, templateIndex)
        {
        }

        public override EquipSlots GetEquipSlot()
        {
            return EquipSlots.LeftHand;
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemArmorTrimmingTool).ToString();
            return data;
        }
    }

    //Striped Kite Shield
    public class ItemArenaKiteShieldMale : DaggerfallUnityItem
    {
        public const int templateIndex = 4763;

        public ItemArenaKiteShieldMale() : base(ItemGroups.Armor, templateIndex)
        {
        }

        public override EquipSlots GetEquipSlot()
        {
            return EquipSlots.LeftHand;
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemArmorTrimmingTool).ToString();
            return data;
        }
    }

    //Engraved Tower Shield
    public class ItemArenaTowerShieldMale : DaggerfallUnityItem
    {
        public const int templateIndex = 4764;

        public ItemArenaTowerShieldMale() : base(ItemGroups.Armor, templateIndex)
        {
        }

        public override EquipSlots GetEquipSlot()
        {
            return EquipSlots.LeftHand;
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemArmorTrimmingTool).ToString();
            return data;
        }
    }
}

