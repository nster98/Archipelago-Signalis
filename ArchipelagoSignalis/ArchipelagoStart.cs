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
using System.IO;
using System.Linq;
using System.Reflection;
using static ResetGame;

namespace ArchipelagoSignalis
{
    public class ArchipelagoStart : MelonMod
    {
        private static List<ItemMapping> itemMappings;

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            MelonLogger.Msg("ArchipelagoSignalis loaded");
            LoadItemMappings();
        }

        private void LoadItemMappings()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "ArchipelagoSignalis.resource_files.signalis_location_mapping.csv";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                itemMappings = new List<ItemMapping>();
                var lines = reader.ReadToEnd().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines.Skip(1)) // Skip header line
                {
                    var values = line.Split(',');
                    var itemMapping = new ItemMapping
                    {
                        ArchipelagoItemName = values[0],
                        InGameName = values[1],
                        Scene = values[2],
                        Room = values[3]
                    };
                    itemMappings.Add(itemMapping);
                }
                MelonLogger.Msg("Item mappings loaded successfully.");
            }
        }

        public static string GetArchipelagoItemName(string inGameName, string scene, string room)
        {
            MelonLogger.Msg($"Looking for item mapping for {inGameName} in {scene} {room}");
            MelonLogger.Msg($"Item mappings count: {itemMappings.Count}");
            MelonLogger.Msg($"Item Mappings index 1: <{itemMappings[1].Room}>");
            var itemMapping = itemMappings.FirstOrDefault(im => im.InGameName == inGameName && im.Scene == scene && (string.IsNullOrEmpty(im.Room) || im.Room == room));
            MelonLogger.Msg($"Found mapping :: {itemMapping?.ArchipelagoItemName}");
            return itemMapping?.ArchipelagoItemName;
        }

        public static string GetSignalisItemName(string archipelagoName)
        {
            var itemMapping = itemMappings.FirstOrDefault(im => im.ArchipelagoItemName == archipelagoName);
            return itemMapping?.InGameName;
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
            ArchipelagoHelper.ConnectToArchipelagoOnBeginAnew(sceneName);
        }
    }
}
