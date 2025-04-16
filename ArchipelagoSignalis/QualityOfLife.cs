using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArchipelagoSignalis
{
    class QualityOfLife : MelonMod
    {
        private static bool setMeatVersion = false;
        public static void OpenStorageBoxFromAnywhereListener()
        {
            if (Input.GetKeyDown(KeyCode.F11) && !LevelSelect.isInventoryOpen)
            {
                MelonLogger.Msg("F11 key pressed");
                InventoryBase.OpenStorageBox();
            }
        }

        public static void CheckRotfrontMeatVersion()
        {
            if (SceneManager.GetActiveScene().name.Contains("ROT")
                && SaveManagement.LocationsChecked.Contains("Lovers")
                && SaveManagement.LocationsChecked.Contains("Death")
                && SaveManagement.LocationsChecked.Contains("Moon")
                && SaveManagement.LocationsChecked.Contains("Sun")
                && SaveManagement.LocationsChecked.Contains("Tower")
                && SaveManagement.LocationsChecked.Contains("Star")
                && !setMeatVersion)
            {
                GameObject meatVersion = null;
                GameObject[] gameObjects = GameObject.FindObjectsOfType<GameObject>(true);
                if (meatVersion == null)
                {
                    foreach (var gameObject in gameObjects)
                    {
                        if (gameObject.name == "MeatVersion")
                        {
                            MelonLogger.Msg("Found MeatVersion");
                            meatVersion = gameObject;
                        }
                    }
                }

                if (meatVersion != null && !setMeatVersion)
                {
                    meatVersion.SetActive(true);
                    setMeatVersion = true;
                    MelonLogger.Msg("Setting MeatVersion to true");
                }
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
