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

    }

    [HarmonyPatch(typeof(SaveManager), "Save")]
    public static class SaveGame
    {
        private static void Prefix()
        {
            MelonLogger.Msg("Saving game");
            SProgress.SetString("archipelagoData1", "Item1");
        }
    }

    [HarmonyPatch(typeof(SaveManager), "Load")]
    public static class LoadSave
    {
        private static void Postfix()
        {
            MelonLogger.Msg("Loading save");
            MelonLogger.Msg(SProgress.GetString("archipelagoData1", ""));
        }
    }
}
