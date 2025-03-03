using MelonLoader;
using HarmonyLib;
using UnityEngine;

namespace ArchipelagoSignalis
{
    public class ArchipelagoStart : MelonMod {

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            MelonLogger.Msg("ArchipelagoSignalis loaded");
        }

        public override void OnUpdate()
        {
            OpenStorageBoxFromAnywhereListener();
            GiveRadio();
        }

        private static void OpenStorageBoxFromAnywhereListener()
        {
            if (Input.GetKeyDown(KeyCode.F11))
            {
                MelonLogger.Msg("F11 key pressed");
                InventoryBase.OpenStorageBox();
            }
        }

        private static void GiveRadio()
        {
            if (Input.GetKeyDown(KeyCode.F10))
            {
                MelonLogger.Msg("F10 key pressed");
                RadioManager.moduleInstalled = !RadioManager.moduleInstalled;
            }
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

    [HarmonyPatch(typeof(RadioModuleAcquirer), "getRadioModule")]
    public static class DetectRadioPickup
    {
        private static void Prefix()
        {
            MelonLogger.Msg("Picked up Radio");
        }
    }
}
