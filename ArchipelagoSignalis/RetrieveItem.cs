using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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

        public static void GiveRadio()
        {
            if (Input.GetKeyDown(KeyCode.F10))
            {
                MelonLogger.Msg("F10 key pressed");
                RadioManager.moduleInstalled = !RadioManager.moduleInstalled;
            }
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
