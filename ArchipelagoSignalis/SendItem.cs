using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using MelonLoader;

namespace ArchipelagoSignalis
{
    class SendItem : MelonMod
    {
        [HarmonyPatch(typeof(ItemPickup), "release")]
        public static class DetectItemPickup
        {
            private static int numItemsInInventory = 0;

            private static void Prefix(ItemPickup __instance)
            {
                numItemsInInventory = InventoryManager.elsterItems.Count;
                var item = __instance._item;
                var playerState = PlayerState.currentRoom;
                MelonLogger.Msg($"Item picked up: {item._name} in room: {playerState.roomName}");
            }

            private static void Postfix(ItemPickup __instance)
            {
                var currentNumItemsInInventory = InventoryManager.elsterItems.Count;
                var item = __instance._item;
                if (currentNumItemsInInventory != numItemsInInventory)
                {
                    if (string.Equals(item._item.ToString(), "FalkeSpear", StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    MelonLogger.Msg($"Removed item {item._item} from inventory");
                    InventoryManager.RemoveItem(item);
                    //TODO: Call Archipelago API for check

                    SaveManagement.UpdateItemsCollected(item._item.ToString());
                }

            }
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
