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
        private static List<string> doorsLockedList = new List<string>();
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
                UnlockPreviouslyUnlockedDoors(appendedSceneName);
            }

            // GameObject[] objects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            // foreach (GameObject gameObject in objects)
            // {
            //     if (gameObject.activeInHierarchy && null != gameObject.GetComponentByName("Room"))
            //     {
            //         MelonLogger.Msg($"Scene : {sceneName} : {gameObject.name}");
            //     }
            // }
        }

        public static void UnlockPreviouslyUnlockedDoors(string scene)
        {
            ConnectedDoors[] doors = UnityEngine.Object.FindObjectsOfType<ConnectedDoors>();
            doorsLockedList = new List<string>();
            foreach (ConnectedDoors door in doors)
            {
                // MelonLogger.Msg($"Door : {door.name}");
                // MelonLogger.Msg($"DoorsUnlocked String : {SaveManagement.DoorsUnlocked}");
                if (null != door.key)
                {
                    if (scene == "BIO")
                    {
                        var classroomKeyBio = door.key._item.ToString() + "_BIO";
                        doorsLockedList.Add(classroomKeyBio);
                        if (SaveManagement.DoorsUnlocked.Contains(classroomKeyBio))
                        {
                            MelonLogger.Msg($"Unlocking door: {classroomKeyBio}");
                            door.locked = false;
                        }
                    }
                    else
                    {
                        doorsLockedList.Add(door.key._item.ToString());
                        if (SaveManagement.DoorsUnlocked.Contains(door.key._item.ToString()))
                        {
                            MelonLogger.Msg($"Unlocking door: {door.key._item.ToString()}");
                            door.locked = false;
                        }
                    }
                    
                    
                }
            }
        }

        public static void TrackUnlockedDoors()
        {
            // Use array to track which doors are still locked
            // When they become unlocked, remove it from the list and add it to the save list
            ConnectedDoors[] doors = UnityEngine.Object.FindObjectsOfType<ConnectedDoors>();
            foreach (ConnectedDoors door in doors)
            {
                var sceneName = SceneManager.GetActiveScene().name.Substring(0, 3);
                if (null != door.key && !door.locked)
                {
                    if (sceneName == "BIO" && doorsLockedList.Contains(door.key._item.ToString() + "_BIO"))
                    {
                        var classroomKeyBio = door.key._item.ToString() + "_BIO";
                        doorsLockedList.Remove(classroomKeyBio);
                        SaveManagement.UpdateDoorsUnlocked(classroomKeyBio);
                    }
                    else if (doorsLockedList.Contains(door.key._item.ToString()))
                    {
                        doorsLockedList.Remove(door.key._item.ToString());
                        SaveManagement.UpdateDoorsUnlocked(door.key._item.ToString());
                    }
                        
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
}
