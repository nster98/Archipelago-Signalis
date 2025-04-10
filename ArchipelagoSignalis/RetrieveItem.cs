using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Models;
using MelonLoader;
using Newtonsoft.Json;
using UnityEngine;

namespace ArchipelagoSignalis
{
    public class RetrieveItem : MelonMod
    {
        public static Queue<string> RetrieveItemQueue = new();

        public static async void CheckForF9KeyPress()
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

        // Add item to inventory or box, depending on current inventory count
        public static void AddItemToInventory(string itemName)
        {
            // Adding item while inventory is open will crash the game
            // Queue the item instead, and add it when inventory is closed
            if (LevelSelect.isInventoryOpen)
            {
                MelonLogger.Msg($"Adding item to queue: {itemName}");
                RetrieveItemQueue.Enqueue(itemName);
            }
            else
            {
                MelonLogger.Msg($"Attempting to add item to inventory: {itemName}");
                if (itemName.Contains("Radio"))
                {
                    GiveRadio();
                    SaveManagement.UpdateItemsReceived(itemName);
                    return;
                }

                List<string> elsterItems = new List<string>();
                foreach (var item in InventoryManager.elsterItems)
                {
                    elsterItems.Add(item.key._item.ToString());
                }

                var countOfItems = GetCountOfItemsToAddToInventory(itemName);
                MelonLogger.Msg($"Count of item : {countOfItems}");

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
                            InventoryManager.boxItem(item, countOfItems);
                        }
                        else
                        {
                            MelonLogger.Msg($"Adding item to inventory: {itemName}");
                            InventoryManager.AddItem(item, countOfItems);
                        }
                    }
                }
                SaveManagement.UpdateItemsReceived(itemName);
            }
        }

        public static void GiveRadio()
        {
            MelonLogger.Msg("Setting Radio module installed to true");
            MelonLogger.Msg($"Current Radio Module Installed value : {RadioManager.moduleInstalled}");
            RadioManager.moduleInstalled = true;
        }

        public static void ListenForItemReceived(ArchipelagoSession session)
        {
            session.Items.ItemReceived += (receivedItemHelper) =>
            {
                ItemInfo item = receivedItemHelper.DequeueItem();
                if (item != null)
                {
                    MelonLogger.Msg($"Received item: {item.ItemName}");
                    AddItemToInventory(ArchipelagoStart.GetSignalisItemName(item.ItemName));
                }
            };
            // Fix to ensure the latest item will be dequeued when retrieved
            while (session.Items.Any())
            {
                ItemInfo removedItem = session.Items.DequeueItem();
                MelonLogger.Msg($"Removed item from the queue: {removedItem.ItemName}");
            }
        }

        public static int GetCountOfItemsToAddToInventory(string itemName)
        {
            MelonLogger.Msg($"AmmoPickupMultiplier : {DynamicDifficulty.AmmoPickupMultiplier}");
            MelonLogger.Msg($"HealthPickupMultiplier : {DynamicDifficulty.HealthPickupMultiplier}");
            if (itemName.Contains("Ammo"))
            {
                if (itemName.Contains("FlakGun")) return (int)(DynamicDifficulty.AmmoPickupMultiplier * 2);
                if (itemName.Contains("FlareGun")) return (int) (DynamicDifficulty.AmmoPickupMultiplier * 2);
                if (itemName.Contains("Pistol")) return (int) (DynamicDifficulty.AmmoPickupMultiplier * 10);
                if (itemName.Contains("Revolver")) return (int) (DynamicDifficulty.AmmoPickupMultiplier * 6);
                if (itemName.Contains("Rifle")) return (int) (DynamicDifficulty.AmmoPickupMultiplier * 3);
                if (itemName.Contains("Shotgun")) return (int) (DynamicDifficulty.AmmoPickupMultiplier * 3);
                if (itemName.Contains("Smg")) return (int) (DynamicDifficulty.AmmoPickupMultiplier * 16);
            } else if (itemName.Contains("Health"))
            {
                return (int) (DynamicDifficulty.HealthPickupMultiplier * 1);
            }
            else if (itemName.Contains("SignalFlare"))
            {
                return 2;
            }

            return 1;
        }
    }

    public class ApiResponse
    {
        [JsonProperty("item")] public string Item { get; set; }

        [JsonProperty("location")] public int Location { get; set; }

        [JsonProperty("player")] public string Player { get; set; }

        [JsonProperty("flags")] public int Flags { get; set; }
    }
}
