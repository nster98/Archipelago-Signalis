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
        public static string ItemsCollected = "";
        public static string ItemsReceived = "";

        public static void UpdateItemsCollected(string itemName)
        {
            if (ItemsCollected == "")
            {
                ItemsCollected = itemName;
            }
            else
            {
                ItemsCollected += "," + itemName;
            }
            MelonLogger.Msg($"Items collected: {ItemsCollected}");
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
    }

    [HarmonyPatch(typeof(SaveManager), "Save")]
    public static class SaveGame
    {
        private static void Prefix()
        {
            MelonLogger.Msg("Saving game");
            
            SProgress.SetString("ItemsCollected", SaveManagement.ItemsCollected);
            SProgress.SetString("ItemsReceived", SaveManagement.ItemsReceived);
            SProgress.SetString("LevelsReached", SaveManagement.LevelsReached);
        }
    }

    [HarmonyPatch(typeof(SaveManager), "Load")]
    public static class LoadSave
    {
        private static void Postfix()
        {
            MelonLogger.Msg("Loading save");

            SaveManagement.ItemsCollected = SProgress.GetString("ItemsCollected", "");
            SaveManagement.ItemsReceived = SProgress.GetString("ItemsReceived", "");
            SaveManagement.LevelsReached = SProgress.GetString("LevelsReached", "");

            MelonLogger.Msg($"Loading game :: Items collected: {SaveManagement.ItemsCollected}");
            MelonLogger.Msg($"Loading game :: Levels reached: {SaveManagement.LevelsReached}");

            Task.Run(ArchipelagoHelper.InitializeArchipelago);

            // RetrieveItem.AddItemToInventory("KeyOfLove");
            // RetrieveItem.AddItemToInventory("KeyOfSacrifice");
            // RetrieveItem.AddItemToInventory("KeyOfEternity");
        }
    }
}
