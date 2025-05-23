﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using MelonLoader;

namespace ArchipelagoSignalis
{
    class SaveManagement : MelonMod
    {
        public static string LevelsReached = "";
        public static string LocationsChecked = "";
        public static string ItemsReceived = "";
        public static string DoorsUnlocked = "";

        public static void UpdateLocationsChecked(string itemName)
        {
            if (LocationsChecked == "")
            {
                LocationsChecked = itemName;
            }
            else if (!LocationsChecked.Contains(itemName))
            {
                LocationsChecked += "," + itemName;
            }
            MelonLogger.Msg($"Locations checked: {LocationsChecked}");
        }

        public static void UpdateItemsReceived(string itemName)
        {
            if (ItemsReceived == "")
            {
                ItemsReceived = itemName;
            }
            else
            {
                ItemsReceived += "," + itemName;
            }
            MelonLogger.Msg($"Items received: {ItemsReceived}");
        }

        public static void UpdateLevelsReached(string levelName)
        {
            if (LevelsReached == "")
            {
                LevelsReached = levelName;
            }
            else if (!LevelsReached.Contains(levelName))
            {
                LevelsReached += "," + levelName;
            }
            MelonLogger.Msg($"Levels reached: {LevelsReached}");
        }

        public static void UpdateDoorsUnlocked(string keyName)
        {
            if (DoorsUnlocked == "")
            {
                DoorsUnlocked = keyName;
            }
            else if (!DoorsUnlocked.Contains(keyName))
            {
                DoorsUnlocked += "," + keyName;
            }
            MelonLogger.Msg($"Doors unlocked: {DoorsUnlocked}");
        }

        public static void ResetItemsReceived()
        {
            ItemsReceived = "";
            MelonLogger.Msg($"Items received reset :: {ItemsReceived}");
        }
    }

    [HarmonyPatch(typeof(SaveManager), "Save")]
    public static class SaveGame
    {
        private static void Prefix()
        {
            MelonLogger.Msg("Saving game");
            
            SProgress.SetString("LocationsChecked", SaveManagement.LocationsChecked);
            SProgress.SetString("ItemsReceived", SaveManagement.ItemsReceived);
            SProgress.SetString("LevelsReached", SaveManagement.LevelsReached);
            SProgress.SetString("DoorsUnlocked", SaveManagement.DoorsUnlocked);
        }
    }

    [HarmonyPatch(typeof(SaveManager), "Load")]
    public static class LoadSave
    {
        private static void Postfix()
        {
            MelonLogger.Msg("Loading save");

            SaveManagement.LocationsChecked = SProgress.GetString("LocationsChecked", "");
            SaveManagement.ItemsReceived = SProgress.GetString("ItemsReceived", "");
            SaveManagement.LevelsReached = SProgress.GetString("LevelsReached", "");
            SaveManagement.DoorsUnlocked = SProgress.GetString("DoorsUnlocked", "");

            MelonLogger.Msg($"Loading game :: Locations Checked : {SaveManagement.LocationsChecked}");
            MelonLogger.Msg($"Loading game :: Items Received : {SaveManagement.ItemsReceived}");
            MelonLogger.Msg($"Loading game :: Doors Unlocked : {SaveManagement.DoorsUnlocked}");
            MelonLogger.Msg($"Loading game :: Levels reached : {SaveManagement.LevelsReached}");

            Task.Run(ArchipelagoHelper.InitializeArchipelago);
        }
    }
}
