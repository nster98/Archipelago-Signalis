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
using System.Text;
using static ResetGame;
using Harmony;
using TMPro;
using static UnityEngine.GUI;

namespace ArchipelagoSignalis
{
    public class ArchipelagoStart : MelonMod
    {
        private static List<LocationMapping> locationMappings;
        private static List<ItemMapping> itemMappings;

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            MelonLogger.Msg("ArchipelagoSignalis loaded");
            LoadLocationMappings();
            LoadItemMappings();
        }

        private void LoadItemMappings()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "ArchipelagoSignalis.resource_files.signalis_item_mapping.csv";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                itemMappings = new List<ItemMapping>();
                var lines = reader.ReadToEnd().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines.Skip(1)) // Skip header line
                {
                    var values = ParseCsvLine(line);
                    var itemMapping = new ItemMapping
                    {
                        ArchipelagoItemName = values[0],
                        InGameName = values[1]
                    };
                    itemMappings.Add(itemMapping);
                }
                MelonLogger.Msg("Item mappings loaded successfully.");
            }
        }

        private void LoadLocationMappings()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "ArchipelagoSignalis.resource_files.signalis_location_mapping.csv";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                locationMappings = new List<LocationMapping>();
                var lines = reader.ReadToEnd().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines.Skip(1)) // Skip header line
                {
                    var values = ParseCsvLine(line);
                    var locationMapping = new LocationMapping
                    {
                        ArchipelagoItemName = values[0],
                        InGameName = values[1],
                        Scene = values[2],
                        Room = values[3]
                    };
                    locationMappings.Add(locationMapping);
                }
                MelonLogger.Msg("Location mappings loaded successfully.");
            }
        }

        private string[] ParseCsvLine(string line)
        {
            var values = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            foreach (var c in line)
            {
                if (c == '"' && !inQuotes)
                {
                    inQuotes = true;
                }
                else if (c == '"' && inQuotes)
                {
                    inQuotes = false;
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            values.Add(current.ToString());
            return values.ToArray();
        }

        public static string GetArchipelagoItemNameFromLocation(string inGameName, string scene, string room)
        {
            try
            {
                MelonLogger.Msg($"Looking for item mapping for {inGameName} in {scene} {room}");
                var locationMapping = locationMappings.FirstOrDefault(im =>
                    im.InGameName == inGameName && im.Scene == scene &&
                    (string.IsNullOrEmpty(im.Room) || im.Room == room));
                MelonLogger.Msg($"Found location mapping :: {locationMapping?.ArchipelagoItemName}");
                return locationMapping?.ArchipelagoItemName;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error in GetArchipelagoItemNameFromLocation: {ex}");
                return "null";

            }
        }

        public static string GetSignalisItemNameFromArchipelagoLocation(string archipelagoName)
        {
            var locationMapping = locationMappings.FirstOrDefault(im => im.ArchipelagoItemName == archipelagoName);
            return locationMapping?.InGameName;

        }

        public static string GetSignalisItemName(string archipelagoName)
        {
            MelonLogger.Msg($"Looking for item mapping for {archipelagoName}");
            var itemMapping = itemMappings.FirstOrDefault(im => im.ArchipelagoItemName == archipelagoName);
            MelonLogger.Msg($"Found item mapping :: {itemMapping?.ArchipelagoItemName}");
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
            ArchipelagoHelper.CheckDisconnectFromArchipelago(sceneName);

            SendLocation.SendPhotoOfAlinaLocation(sceneName);
            UpdateVersionName(sceneName);
        }

        private void UpdateVersionName(string sceneName)
        {
            if (sceneName == "MainMenu")
            {
                GameObject[] objects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                foreach (GameObject gameObject in objects)
                {
                    if (gameObject.activeInHierarchy && gameObject.name == "Version")
                    {
                        TextMeshPro text = gameObject.GetComponent<TextMeshPro>();
                        text.m_text += " | Archipelago v0.2.1";
                    }
                }
            }
        }
    }
}
