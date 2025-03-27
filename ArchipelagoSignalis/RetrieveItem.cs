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
        // TODO: Add only when not in inventory to prevent crashing?
        public static void AddItemToInventory(string itemName)
        {
            MelonLogger.Msg($"Attempting to add item to inventory: {itemName}");
            if (itemName.Contains("Radio Module"))
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

            var itemNameToAdd = ParseMultipleItemName(itemName);

            foreach (AnItem item in InventoryManager.allItems.Values)
            {
                if (string.Equals(itemNameToAdd, item.name, StringComparison.OrdinalIgnoreCase))
                {
                    var itemCount = InventoryManager.elsterItems.Count;
                    if (elsterItems.Contains("PhotoModule")) itemCount--;
                    if (elsterItems.Contains("Flashlight")) itemCount--;

                    if (itemCount > 6)
                    {
                        MelonLogger.Msg($"Adding item to box: {itemNameToAdd}");
                        InventoryManager.boxItem(item, 1);
                    }
                    else
                    {
                        MelonLogger.Msg($"Adding item to inventory: {itemNameToAdd}");
                        InventoryManager.AddItem(item, 1);
                    }
                }
            }
            SaveManagement.UpdateItemsReceived(itemName);
        }

        public static void GiveRadio()
        {
            // if (Input.GetKeyDown(KeyCode.F10))
            // {
                // MelonLogger.Msg("F10 key pressed");
                RadioManager.moduleInstalled = true;
            // }
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
                    SaveManagement.UpdateItemsReceived(item.ItemName);
                }
            };
        }

        public static string ParseMultipleItemName(string itemName)
        {
            if (itemName.Contains("Ammo"))
            {
                if (itemName.Contains("FlakGun")) return "FlakGunAmmo";
                if (itemName.Contains("FlareGun")) return "FlareGunAmmo";
                if (itemName.Contains("Pistol")) return "PistolAmmo";
                if (itemName.Contains("Revolver")) return "RevolverAmmo";
                if (itemName.Contains("Rifle")) return "RifleAmmo";
                if (itemName.Contains("Shotgun")) return "ShotgunAmmo";
                if (itemName.Contains("Smg")) return "SmgAmmo";
            } else if (itemName.Contains("Health"))
            {
                if (itemName.Contains("25")) return "Health25";
                if (itemName.Contains("50")) return "Health50";
                if (itemName.Contains("100")) return "Health100";
                if (itemName.Contains("Fast")) return "HealthFast";
            } else if (itemName.Contains("Injector"))
            {
                return "Injector";
            }

            return itemName;
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
