using System;
using System.Collections.Generic;
using MelonLoader;
using HarmonyLib;
using UnityEngine;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;
using static ResetGame;

namespace ArchipelagoSignalis
{

    
    public class ArchipelagoStart : MelonMod
    {

        public static string SlotName = "";
        public static string Server = "archipelago.gg";
        public static string Port = "";
        public static string Password = "";
        public const string GameName = "Signalis";

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

            DeathLink.CheckDeath();
        }

        public override void OnGUI()
        {
            ArchipelagoUI.RenderArchipelagoSettingsUi();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            MelonLogger.Msg($"Scene loaded: {sceneName} build index {buildIndex}");

            LevelSelect.FillInLevelSelect(sceneName);
            LevelSelect.EnteredNewLevel(sceneName);
        }

        public static void InitializeArchipelago()
        {
            var fullUrl = Server + ":" + Port;
            var session = ArchipelagoSessionFactory.CreateSession(fullUrl);
            Task<LoginResult> loginResult = null;
            try
            {
                loginResult = session.LoginAsync(GameName, SlotName, ItemsHandlingFlags.AllItems,
                    password: Password);

            }
            catch (Exception ex)
            {
                var loginDone = loginResult.Result;
                loginDone = new LoginFailure(ex.GetBaseException().Message);
                LoginFailure failure = (LoginFailure)loginDone;
                string errorMessage = $"Failed to Connect to {fullUrl} as {SlotName}:";
                foreach (string error in failure.Errors)
                {
                    errorMessage += $"\n    {error}";
                }

                foreach (ConnectionRefusedError error in failure.ErrorCodes)
                {
                    errorMessage += $"\n    {error}";
                }

                MelonLogger.Error(errorMessage);
                return;
            }

            var loginWasSuccess = loginResult.Result;
            LoginSuccessful loginSuccess = loginWasSuccess as LoginSuccessful;
            if (loginSuccess != null)
            {
                MelonLogger.Msg($"Connected to {fullUrl} as {SlotName}");
                ArchipelagoHelper.ProcessItemsAfterLogin(session);
                
            }
        }


    }
}
