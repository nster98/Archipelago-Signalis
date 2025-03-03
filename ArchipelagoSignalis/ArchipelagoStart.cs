using MelonLoader;
using System;
using System.Collections.Generic;
using MelonLoader;
using HarmonyLib;
using UnityEngine;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace ArchipelagoSignalis
{
    public class ArchipelagoStart : MelonMod {

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            MelonLogger.Msg("ArchipelagoSignalis loaded");
        }

        public override void OnUpdate()
        {
            OpenStorageBoxFromAnywhereListener();
            GiveRadio();
            CheckForF9KeyPress();
        }

        private static void OpenStorageBoxFromAnywhereListener()
        {
            if (Input.GetKeyDown(KeyCode.F11))
            {
                MelonLogger.Msg("F11 key pressed");
                InventoryBase.OpenStorageBox();
            }
        }

        private static void GiveRadio()
        {
            if (Input.GetKeyDown(KeyCode.F10))
            {
                MelonLogger.Msg("F10 key pressed");
                RadioManager.moduleInstalled = !RadioManager.moduleInstalled;
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            MelonLogger.Msg($"Scene loaded: {sceneName}");
        }

        // Add item to inventory or box, depending on current inventory count
        private static void AddItemToInventory(string itemName)
        {
            List<string> elsterItems = new List<string>();
            foreach (var item in InventoryManager.elsterItems)
            {
                elsterItems.Add(item.key._item.ToString());
            }
            foreach (AnItem item in InventoryManager.allItems.Values)
            {
                if (string.Equals(itemName, item.name, StringComparison.OrdinalIgnoreCase))
                {
                    var itemCount = InventoryManager.elsterItems.Count;
                    if (elsterItems.Contains("PhotoModule")) itemCount--;
                    if (elsterItems.Contains("Flashlight")) itemCount--;

                    if (itemCount > 6)
                    {
                        MelonLogger.Msg($"Adding item to box: {itemName}");
                        InventoryManager.boxItem(item, 1);
                    }
                    else
                    {
                    MelonLogger.Msg($"Adding item to inventory: {itemName}");
                    InventoryManager.AddItem(item, 1);
                }

                }
            }
        }

        private static async void CheckForF9KeyPress()
        {
            if (Input.GetKeyDown(KeyCode.F9))
            {
                MelonLogger.Msg("F9 key pressed");
                await FetchDataFromApi();
            }
        }

        private static async Task FetchDataFromApi()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync("http://localhost:3000/api/data");
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<List<ApiResponse>>(responseBody);
                    if (data != null && data.Count > 0)
                    {
                        var itemField = data[0].Item;
                        MelonLogger.Msg($"Item field extracted: {itemField}");
                        AddItemToInventory(itemField);
                    }
                }
                catch (HttpRequestException e)
                {
                    MelonLogger.Error($"Request error: {e.Message}");
                }
            }
        }
    }

    public class ApiResponse
    {
        [JsonProperty("item")]
        public string Item { get; set; }

        [JsonProperty("location")]
        public int Location { get; set; }

        [JsonProperty("player")]
        public string Player { get; set; }

        [JsonProperty("flags")]
        public int Flags { get; set; }
    }

    [HarmonyPatch(typeof(ItemPickup), "release")]
    public static class DetectItemPickup
    {
        private static void Prefix(ItemPickup __instance)
        {
            var item = __instance._item;
            var playerState = PlayerState.currentRoom;
            MelonLogger.Msg($"Item picked up: {item._name} in room: {playerState.roomName}");
            // Here you can add your logic to send the item pickup to Archipelago
            // For example, using an HTTP request to the Archipelago server
        }
    }

    [HarmonyPatch(typeof(RadioModuleAcquirer), "getRadioModule")]
    public static class DetectRadioPickup
    {
        private static void Prefix()
        {
            MelonLogger.Msg("Picked up Radio");
        }
    }
}
