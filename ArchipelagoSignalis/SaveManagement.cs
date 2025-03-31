using System;
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

        public static void ResetItemsReceived()
        {
            ItemsReceived = "";
            MelonLogger.Msg($"Items received reset :: {ItemsReceived}");
        }

        public static void ResetIntruderCutscene()
        {
            // LOV Opening Cutscene
            SProgress.SetBool("cut LOV_Reeducation_c1006328-b9f7-40fc-aa0d-3fdd32fe3103", false);

            // DET Opening Cutscene
            SProgress.SetBool("cut DET_Detention_fbcdaacc-bae8-49da-ba35-ecf336a4ebfe", false);
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

            SaveManagement.ResetIntruderCutscene();
        }
    }

    [HarmonyPatch(typeof(SaveManager), "Load")]
    public static class LoadSave
    {
        private static void Postfix()
        {
            MelonLogger.Msg("Loading save");

            for (var i = 0; i < SProgress.progress.boolKeys.Count; i++)
            {
                MelonLogger.Msg($"Key: {SProgress.progress.boolKeys[i]} | Value: {SProgress.progress.bools[i]}");
            }

            // Reset cutscene boolean to ensure cutscene players after teleporting
            SaveManagement.ResetIntruderCutscene();

            SaveManagement.LocationsChecked = SProgress.GetString("LocationsChecked", "");
            SaveManagement.ItemsReceived = SProgress.GetString("ItemsReceived", "");
            SaveManagement.LevelsReached = SProgress.GetString("LevelsReached", "");

            MelonLogger.Msg($"Loading game :: Items collected: {SaveManagement.LocationsChecked}");
            MelonLogger.Msg($"Loading game :: Levels reached: {SaveManagement.LevelsReached}");

            Task.Run(ArchipelagoHelper.InitializeArchipelago);
        }
    }
}
