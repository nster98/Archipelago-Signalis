using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using MelonLoader;

namespace ArchipelagoSignalis
{
    class GameCompletion : MelonMod
    {

    }

    [HarmonyPatch(typeof(END_Manager), "EvaluateEnding")]
    public static class DetectEnding
    {
        private static void Prefix()
        {
            MelonLogger.Msg("Ending triggered");
        }
    }
}
