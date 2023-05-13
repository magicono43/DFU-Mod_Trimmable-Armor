// Project:         TrimmableArmor mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2023 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Created On: 	    5/13/2021, 3:15 PM
// Last Edit:		5/13/2023, 3:15 PM
// Version:			1.00
// Special Thanks:  
// Modifier:

using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;

namespace TrimmableArmor
{
    public class TrimmableArmorMain : MonoBehaviour
    {
        public static TrimmableArmorMain Instance;

        static Mod mod;

        public static int RequiredRecoveryHours { get; set; }
        public static int NonMemberCostMultiplier { get; set; }
        public static float FinalTrainingCostMultiplier { get; set; }
        public static int MaxTrainAwful { get; set; }
        public static int MaxTrainPoor { get; set; }
        public static int MaxTrainDecent { get; set; }
        public static int MaxTrainGood { get; set; }
        public static int MaxTrainGreat { get; set; }
        public static float FinalTrainedAmountMultiplier { get; set; }
        public static int HoursPassedDuringTraining { get; set; }
        public static bool AllowHealthMagicDamage { get; set; }
        public static int MaximumPossibleTraining { get; set; }

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject(mod.Title);
            go.AddComponent<TrimmableArmorMain>(); // Add script to the scene.

            mod.LoadSettingsCallback = LoadSettings; // To enable use of the "live settings changes" feature in-game.

            mod.IsReady = true;
        }

        private void Start()
        {
            Debug.Log("Begin mod init: Trimmable Armor");

            Instance = this;

            mod.LoadSettings();

            UIWindowFactory.RegisterCustomUIWindow(UIWindowType.GuildServiceTraining, typeof(TrainingServiceOverhaulWindow));

            Debug.Log("Finished mod init: Trimmable Armor");
        }

        static void LoadSettings(ModSettings modSettings, ModSettingsChange change)
        {
            RequiredRecoveryHours = mod.GetSettings().GetValue<int>("TimeRelated", "HoursNeededBetweenSessions");
            NonMemberCostMultiplier = mod.GetSettings().GetValue<int>("GoldCost", "Non-MemberCostMultiplier");
            FinalTrainingCostMultiplier = mod.GetSettings().GetValue<float>("GoldCost", "FinalCostMultiplier");
            MaxTrainAwful = mod.GetSettings().GetValue<int>("MaxSkillsBasedOnQuality", "MaxAwfulHallsCanTrain");
            MaxTrainPoor = mod.GetSettings().GetValue<int>("MaxSkillsBasedOnQuality", "MaxPoorHallsCanTrain");
            MaxTrainDecent = mod.GetSettings().GetValue<int>("MaxSkillsBasedOnQuality", "MaxDecentHallsCanTrain");
            MaxTrainGood = mod.GetSettings().GetValue<int>("MaxSkillsBasedOnQuality", "MaxGoodHallsCanTrain");
            MaxTrainGreat = mod.GetSettings().GetValue<int>("MaxSkillsBasedOnQuality", "MaxGreatHallsCanTrain");
            FinalTrainedAmountMultiplier = mod.GetSettings().GetValue<float>("TrainingExperience", "TrainedXPMultiplier");
            HoursPassedDuringTraining = mod.GetSettings().GetValue<int>("TimeRelated", "HoursPassedDuringSessions");
            AllowHealthMagicDamage = mod.GetSettings().GetValue<bool>("VitalsRelated", "AllowHealthMagicDamage");
            MaximumPossibleTraining = mod.GetSettings().GetValue<int>("MaxSkillsCanBeTrain", "MaxPossibleTraining");
        }

        public static int GetReqRecovHours()
        {
            return RequiredRecoveryHours;
        }

        public static int GetNonMembMulti()
        {
            return NonMemberCostMultiplier;
        }

        public static float GetFinalTrainCostMulti()
        {
            return FinalTrainingCostMultiplier;
        }

        public static int GetMaxTrainAwful()
        {
            return MaxTrainAwful;
        }

        public static int GetMaxTrainPoor()
        {
            return MaxTrainPoor;
        }

        public static int GetMaxTrainDecent()
        {
            return MaxTrainDecent;
        }

        public static int GetMaxTrainGood()
        {
            return MaxTrainGood;
        }

        public static int GetMaxTrainGreat()
        {
            return MaxTrainGreat;
        }

        public static float GetFinalTrainedAmountMulti()
        {
            return FinalTrainedAmountMultiplier;
        }

        public static int GetHoursPassedTraining()
        {
            return HoursPassedDuringTraining;
        }

        public static bool GetAllowHPMPDamage()
        {
            return AllowHealthMagicDamage;
        }

        public static int GetMaxPossibleTrain()
        {
            return MaximumPossibleTraining;
        }
    }
}
