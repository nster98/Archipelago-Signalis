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
    }

    [HarmonyPatch(typeof(SaveManager), "Save")]
    public static class SaveGame
    {
        private static void Prefix()
        {
            MelonLogger.Msg("Saving game");
            
            SProgress.SetString("ItemsCollected", SaveManagement.ItemsCollected);
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
            SaveManagement.LevelsReached = SProgress.GetString("LevelsReached", "");

            MelonLogger.Msg($"Loading game :: Items collected: {SaveManagement.ItemsCollected}");
            MelonLogger.Msg($"Loading game :: Levels reached: {SaveManagement.LevelsReached}");
        }
    }
}
