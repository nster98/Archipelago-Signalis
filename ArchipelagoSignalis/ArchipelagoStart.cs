using System;
using System.Collections.Generic;
using MelonLoader;
using HarmonyLib;
using UnityEngine;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;

namespace ArchipelagoSignalis
{
    public class ArchipelagoStart : MelonMod
    {

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            MelonLogger.Msg("ArchipelagoSignalis loaded");
        }

        public override void OnUpdate()
        {
            QualityOfLife.OpenStorageBoxFromAnywhereListener();
            LevelSelect.OpenIntruderLevelSelect();
            RetrieveItem.CheckForF9KeyPress();
            RetrieveItem.GiveRadio();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            MelonLogger.Msg($"Scene loaded: {sceneName} build index {buildIndex}");

            LevelSelect.FillInLevelSelect(sceneName);
        }
    }
}
