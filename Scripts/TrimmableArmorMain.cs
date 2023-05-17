// Project:         TrimmableArmor mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2023 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Created On: 	    5/13/2021, 3:15 PM
// Last Edit:		5/16/2023, 8:30 PM
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

        // Global Variables
        public static AudioClip LastSoundPlayed { get { return lastSoundPlayed; } set { lastSoundPlayed = value; } }

        // Mod Sounds
        private static AudioClip lastSoundPlayed = null;
        public static AudioClip[] armorTrimmingClips = { null, null, null, null };
        private static AudioClip trimmingToolBrokeClip = null;

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
            success &= modManager.TryGetAsset("Chiseling_Metal_1", false, out armorTrimmingClips[0]);
            success &= modManager.TryGetAsset("Chiseling_Metal_2", false, out armorTrimmingClips[1]);
            success &= modManager.TryGetAsset("Chiseling_Stone_1", false, out armorTrimmingClips[2]);
            success &= modManager.TryGetAsset("Chiseling_Stone_2", false, out armorTrimmingClips[3]);
            success &= modManager.TryGetAsset("Chiseling_Stone_3_Breaking_Sound", false, out trimmingToolBrokeClip);

            if (!success)
                throw new Exception("TrimmableArmor: Missing sound asset");
        }

        public static void TrimmableArmorStockShelves_OnLootSpawned(object sender, ContainerLootSpawnedEventArgs e) // Populates shop shelves when opened, depending on the shop type.
        {
            DaggerfallInterior interior = GameManager.Instance.PlayerEnterExit.Interior;
            DaggerfallUnityItem item = null;

            if (interior != null && e.ContainerType == LootContainerTypes.ShopShelves)
            {
                if (interior.BuildingData.BuildingType == DaggerfallConnect.DFLocation.BuildingTypes.Armorer)
                {
                    if (DaggerfallWorkshop.Game.Utility.Dice100.SuccessRoll(5 * interior.BuildingData.Quality))
                    {
                        item = ItemBuilder.CreateItem(ItemGroups.UselessItems2, ItemArmorTrimmingTool.templateIndex);
                    }
                }
            }

            if (item != null) { e.Loot.AddItem(item); }
        }

        public static AudioClip RollRandomAudioClip(AudioClip[] clips)
        {
            int randChoice = UnityEngine.Random.Range(0, clips.Length);
            AudioClip clip = clips[randChoice];

            if (clip == LastSoundPlayed)
            {
                if (randChoice == 0)
                    randChoice++;
                else if (randChoice == clips.Length - 1)
                    randChoice--;
                else
                    randChoice = CoinFlip() ? randChoice + 1 : randChoice - 1;

                clip = clips[randChoice];
            }
            LastSoundPlayed = clip;
            return clip;
        }

        public static AudioClip GetTrimmingToolUseClip(bool toolBroke)
        {
            AudioClip clip = null;

            if (toolBroke)
                clip = trimmingToolBrokeClip;
            else
                clip = RollRandomAudioClip(armorTrimmingClips);

            if (clip == null)
                clip = armorTrimmingClips[0];

            return clip;
        }

        public static void TrimmableArmorCommands()
        {
            Debug.Log("[TrimmableArmor] Trying to register console commands.");
            try
            {
                ConsoleCommandsDatabase.RegisterCommand(GiveTools.name, GiveTools.description, GiveTools.usage, GiveTools.Execute);
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

        public static bool CoinFlip()
        {
            if (UnityEngine.Random.Range(0, 1 + 1) == 0)
                return false;
            else
                return true;
        }
    }
}
