using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArchipelagoSignalis
{
    class LevelSelect : MelonMod
    {
        private static List<string> intruderLevelNames = ["END", "PEN", "EXC", "MED", "ROT", "MEM", "LAB", "DET", "BIO", "RES", "LOV"];

        //TODO: Don't allow level select press when in inventory, will crash the game
        public static void OpenIntruderLevelSelect()
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                MelonLogger.Msg("F8 key pressed");

                SceneHelper sceneHelper = new SceneHelper();
                sceneHelper.LoadSceneDirect("scenes_");
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
                        if (gameObject.name == "LOV")
                        {
                            gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
    }
}
