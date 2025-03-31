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
    class LevelSelect : MelonMod
    {
        private static List<string> intruderLevelNames = ["PEN", "LOV", "DET", "MED", "RES", "EXC", "LAB", "MEM", "BIO", "ROT", "END", "TEST"];
        private static bool isDebug = true;
        public static bool isInventoryOpen = false;
        public static string currentScene = "";

        public static void OpenIntruderLevelSelect()
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                MelonLogger.Msg("F8 key pressed");
                if (!isInventoryOpen)
                {
                    MelonLogger.Msg("Inventory not open");
                    SceneHelper sceneHelper = new SceneHelper();
                    sceneHelper.LoadSceneDirect("scenes_");
                }
            }
        }

        public static void EnteredNewLevel(string sceneName)
        {
            var appendedSceneName = sceneName.Substring(0, 3);
            MelonLogger.Msg($"Appended scene name: {appendedSceneName}");
            currentScene = appendedSceneName;
            PlayerState playerState = new PlayerState();
            
            // Set gameState to play in order to not get stuck when cutscene does not load
            // TODO: Check all possible cutscenes and states
            if (SaveManagement.LevelsReached.Contains(appendedSceneName))
            {
                PlayerState.gameState = PlayerState.gameStates.play;
            }

            if (intruderLevelNames.Contains(appendedSceneName))
            {
                SaveManagement.UpdateLevelsReached(appendedSceneName);
                UnlockPreviouslyUnlockedDoors();
            }

            GameObject[] objects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            foreach (GameObject gameObject in objects)
            {
                if (gameObject.activeInHierarchy && null != gameObject.GetComponentByName("Room"))
                {
                    MelonLogger.Msg($"Scene : {sceneName} : {gameObject.name}");
                }
            }
        }

        public static void UnlockPreviouslyUnlockedDoors()
        {
            ConnectedDoors[] doors = UnityEngine.Object.FindObjectsOfType<ConnectedDoors>();
            foreach (ConnectedDoors door in doors)
            {
                MelonLogger.Msg($"Door : {door.name}");
                MelonLogger.Msg($"DoorsUnlocked String : {SaveManagement.DoorsUnlocked}");
                if (null != door.key && SaveManagement.DoorsUnlocked.Contains(door.key._item.ToString()))
                {
                    MelonLogger.Msg($"Unlocking door: {door.key._item.ToString()}");
                    door.locked = false;
                }
            }
        }

        public static void FillInLevelSelect(string sceneName)
        {
            if (sceneName == "EndCredits")
            {
                GameCompletion.DetectSecretEnding();
            }
            if (sceneName == "scenes_")
            {
                GameObject[] objects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                foreach (GameObject gameObject in objects)
                {
                    if (gameObject.activeInHierarchy && intruderLevelNames.Contains(gameObject.name))
                    {
                        if (!isDebug)
                        {
                            string levelsReached = SaveManagement.LevelsReached;
                            if (levelsReached.Contains(gameObject.name))
                            {
                                gameObject.SetActive(true);
                            }
                            else
                            {
                                gameObject.SetActive(false);
                            }

                            if (gameObject.name.Contains("TEST"))
                            {
                                gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(InventoryBase), "ToggleInventory")]
    public static class DetectInventoryOpen
    {
        private static void Postfix()
        {
            MelonLogger.Msg("Inventory toggled");
            LevelSelect.isInventoryOpen = !LevelSelect.isInventoryOpen;

            if (!LevelSelect.isInventoryOpen)
            {
                while (RetrieveItem.RetrieveItemQueue.Any())
                {
                    RetrieveItem.AddItemToInventory(RetrieveItem.RetrieveItemQueue.Dequeue());
                }
            }
        }
    }

    [HarmonyPatch(typeof(ConnectedDoors), "Unlock")]
    public static class DetectDoorUnlock
    {
        private static void Postfix(ConnectedDoors __instance)
        {
            MelonLogger.Msg($"Door unlocked: {__instance.key._item.ToString()}");
            SaveManagement.UpdateDoorsUnlocked(__instance.key._item.ToString());
        }
    }
}
