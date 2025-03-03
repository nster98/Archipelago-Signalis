using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net.Http;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ArchipelagoSignalis
{
    public class ArchipelagoStart : MelonMod {

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            MelonLogger.Msg("ArchipelagoSignalis loaded");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            MelonLogger.Msg($"Scene loaded: {sceneName}");
        }
    }

    [HarmonyPatch(typeof(ItemPickup), "release")]
    public static class DetectItemPickup
    {
        private static void Prefix(ItemPickup __instance)
        {
            var item = __instance._item;
            var playerState = PlayerState.currentRoom;
            MelonLogger.Msg($"Item picked up: {item._name} in room: {playerState.roomName}");
            // Here you can add your logic to send the item pickup to Archipelago
            // For example, using an HTTP request to the Archipelago server
        }
    }
}
