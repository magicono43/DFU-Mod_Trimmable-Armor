// Project:         TrimmableArmor mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2023 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Created On: 	    5/13/2021, 3:15 PM
// Last Edit:		5/16/2023, 12:40 AM
// Version:			1.00
// Special Thanks:  
// Modifier:

using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop;
using System;
using Wenzil.Console;
using DaggerfallWorkshop.Game.Items;

namespace TrimmableArmor
{
    [RequireComponent(typeof(DaggerfallAudioSource))]
    public class TrimmableArmorMain : MonoBehaviour
    {
        public static TrimmableArmorMain Instance;

        static Mod mod;

        // Mod Sounds
        public static AudioClip[] repairToolClips = { null, null, null };

        public DaggerfallAudioSource dfAudioSource;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject(mod.Title);
            go.AddComponent<TrimmableArmorMain>(); // Add script to the scene.

            mod.IsReady = true;
        }

        void Awake()
        {
            dfAudioSource = GetComponent<DaggerfallAudioSource>();
        }

        private void Start()
        {
            Debug.Log("Begin mod init: Trimmable Armor");

            Instance = this;

            DaggerfallUnity.Instance.ItemHelper.RegisterCustomItem(ItemArmorTrimmingTool.templateIndex, ItemGroups.UselessItems1, typeof(ItemArmorTrimmingTool)); // Register Graving Chisel item.

            PlayerActivate.OnLootSpawned += TrimmableArmorStockShelves_OnLootSpawned;

            // Load Resources
            LoadAudio();

            TrimmableArmorCommands();

            Debug.Log("Finished mod init: Trimmable Armor");
        }

        private void LoadAudio()
        {
            ModManager modManager = ModManager.Instance;
            bool success = true;

            // Trimmable Armor Clips
            success &= modManager.TryGetAsset("Blade_Sharpen_WhetStone_1", false, out repairToolClips[0]);
            success &= modManager.TryGetAsset("Sewing_Kit_Repair_1", false, out repairToolClips[1]);
            success &= modManager.TryGetAsset("Armorers_Hammer_Repair_1", false, out repairToolClips[2]);

            if (!success)
                throw new Exception("RepairTools: Missing sound asset");
        }

        public static void TrimmableArmorStockShelves_OnLootSpawned(object sender, ContainerLootSpawnedEventArgs e) // Populates shop shelves when opened, depending on the shop type.
        {
            DaggerfallInterior interior = GameManager.Instance.PlayerEnterExit.Interior;
            DaggerfallUnityItem item = null;

            if (interior != null && e.ContainerType == LootContainerTypes.ShopShelves)
            {
                if (interior.BuildingData.BuildingType == DaggerfallConnect.DFLocation.BuildingTypes.Armorer)
                {
                    if (DaggerfallWorkshop.Game.Utility.Dice100.SuccessRoll(4 * interior.BuildingData.Quality))
                    {
                        item = ItemBuilder.CreateItem(ItemGroups.UselessItems2, ItemArmorTrimmingTool.templateIndex);
                    }
                }
            }

            if (item != null) { e.Loot.AddItem(item); }
        }

        public static void TrimmableArmorCommands()
        {
            Debug.Log("[TrimmableArmor] Trying to register console commands.");
            try
            {
                ConsoleCommandsDatabase.RegisterCommand(GiveTools.name, GiveTools.description, GiveTools.usage, GiveTools.Execute);
                ConsoleCommandsDatabase.RegisterCommand(MakeMagicItems.name, MakeMagicItems.description, MakeMagicItems.usage, MakeMagicItems.Execute);
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("Error Registering TrimmableArmor Console commands: {0}", e.Message));
            }
        }

        private static class GiveTools
        {
            public static readonly string name = "addtrimmingtools";
            public static readonly string description = "Adds Armor Trimming Tools To Inventory.";
            public static readonly string usage = "addtrimmingtools";

            public static string Execute(params string[] args)
            {
                DaggerfallWorkshop.Game.Entity.PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;

                DaggerfallUnityItem item = ItemBuilder.CreateItem(ItemGroups.UselessItems2, 4760);
                playerEntity.Items.AddItem(item);

                return "Gave you ALL the armor trimming tool items.";
            }
        }

        private static class MakeMagicItems
        {
            public static readonly string name = "addmagicarmor";
            public static readonly string description = "Adds A Bunch Of Random Magic Armor Pieces For Testing.";
            public static readonly string usage = "addmagicarmor";

            public static string Execute(params string[] args)
            {
                DaggerfallWorkshop.Game.Entity.PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;

                for (int i = 0; i < 16; i++)
                {
                    DaggerfallUnityItem item = ItemBuilder.CreateRegularMagicItem(-1, 20, GameManager.Instance.PlayerEntity.Gender, GameManager.Instance.PlayerEntity.Race);
                    playerEntity.Items.AddItem(item);
                }

                return "Gave you a bunch of random magic armor.";
            }
        }
    }
}
