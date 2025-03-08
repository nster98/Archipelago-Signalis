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
        private static bool isDebug = false;
        public static bool isInventoryOpen = false;

        //TODO: Don't allow level select press when in inventory, will crash the game
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
            if (intruderLevelNames.Contains(appendedSceneName))
            {
                SaveManagement.UpdateLevelsReached(appendedSceneName);
            }
        }

        public static void FillInLevelSelect(string sceneName)
        {
            if (sceneName == "scenes_")
            {
                Scene intruderScene = SceneManager.GetActiveScene();
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
        }
    }
}
