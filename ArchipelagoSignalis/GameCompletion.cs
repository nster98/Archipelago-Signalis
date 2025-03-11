using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace ArchipelagoSignalis
{
    class GameCompletion : MelonMod
    {
        public static void DetectSecretEnding()
        {
            MelonLogger.Msg("Secret ending triggered");
        }
    }

    [HarmonyPatch(typeof(END_Manager), "EvaluateEnding")]
    public static class DetectEnding
    {
        private static void Prefix()
        {
            MelonLogger.Msg("Ending triggered");
        }
    }

    // [HarmonyPatch(typeof(CutsceneManager), "StartCutscene")]
    // public static class DetectSecretEnding
    // {
    //     private static void Prefix(CutsceneManager __instance)
    //     {
    //         GameObject[] objects = UnityEngine.Object.FindObjectsOfType<GameObject>();
    //         foreach (var gameObject in objects)
    //         {
    //             if (gameObject.activeInHierarchy && gameObject.name == "END_Grave_1 Typo")
    //             {
    //                 if (gameObject.)
    //                 {
    //                     MelonLogger.Msg("Secret ending triggered");
    //                 }
    //             }
    //         }
    //     }
    // }
}
