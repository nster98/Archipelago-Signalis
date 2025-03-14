using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
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
            SendCompletionToArchipelago();
        }

        public static void SendCompletionToArchipelago()
        {
            var statusUpdatePacket = new StatusUpdatePacket();
            statusUpdatePacket.Status = ArchipelagoClientState.ClientGoal;
            ArchipelagoHelper.Session.Socket.SendPacket(statusUpdatePacket);
        }
    }

    [HarmonyPatch(typeof(END_Manager), "EvaluateEnding")]
    public static class DetectEnding
    {
        private static void Prefix()
        {
            MelonLogger.Msg("Ending triggered");
            GameCompletion.SendCompletionToArchipelago();
        }
    }
}
