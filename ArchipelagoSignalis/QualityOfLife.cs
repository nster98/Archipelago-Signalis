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
    class QualityOfLife : MelonMod
    {
        public static void OpenStorageBoxFromAnywhereListener()
        {
            if (Input.GetKeyDown(KeyCode.F11))
            {
                MelonLogger.Msg("F11 key pressed");
                InventoryBase.OpenStorageBox();
            }
        }
    }

    [HarmonyPatch(typeof(ROT_MeatBlocker), "Block")]
    public static class TurnOffMeatBlockers
    {
        public static void Postfix(ROT_MeatBlocker __instance)
        {
            MelonLogger.Msg("Attempting to turn off meat blockers");
            MelonLogger.Msg(__instance.blocked);
            __instance.blocked = false;
            __instance.enabled = false;
            SProgress.SetBool("Meat_" + __instance.ID, __instance.blocked);


            foreach (var blocker in __instance.Blockers)
            {
                blocker.SetActive(false);
            }
            foreach (var unblocker in __instance.UnBlockers)
            {
                unblocker.SetActive(true);
            }

            foreach (var connectedDoor in __instance.Locks)
            {
                connectedDoor.locked = false;
                connectedDoor.BacktrackDoor = true;
                // could be this need to test
                connectedDoor.externalUnlocker = true;
                connectedDoor.UpdateProperties();
            }
        }

    }
}
