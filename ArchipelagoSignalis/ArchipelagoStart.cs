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
            OpenStorageBoxFromAnywhereListener();
            GiveRadio();
            CheckForF9KeyPress();
            OpenIntruderLevelSelect();
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

        private List<string> intruderLevelNames = ["END", "PEN", "EXC", "MED", "ROT", "MEM", "LAB", "DET", "BIO", "RES", "LOV"];

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            MelonLogger.Msg($"Scene loaded: {sceneName} build index {buildIndex}");

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

        private static async void CheckForF9KeyPress()
        {
            if (Input.GetKeyDown(KeyCode.F9))
            {
                MelonLogger.Msg("F9 key pressed");
                await FetchDataFromApi();
            }
        }

        private static void OpenIntruderLevelSelect()
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                MelonLogger.Msg("F8 key pressed");

                SceneHelper sceneHelper = new SceneHelper();
                sceneHelper.LoadSceneDirect("scenes_");
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
        [JsonProperty("item")] public string Item { get; set; }

        [JsonProperty("location")] public int Location { get; set; }

        [JsonProperty("player")] public string Player { get; set; }

        [JsonProperty("flags")] public int Flags { get; set; }
    }

    [HarmonyPatch(typeof(ItemPickup), "release")]
    public static class DetectItemPickup
    {
        private static int numItemsInInventory = 0;

        private static void Prefix(ItemPickup __instance)
        {
            numItemsInInventory = InventoryManager.elsterItems.Count;
            var item = __instance._item;
            var playerState = PlayerState.currentRoom;
            MelonLogger.Msg($"Item picked up: {item._name} in room: {playerState.roomName}");
        }

        private static void Postfix(ItemPickup __instance)
        {
            var currentNumItemsInInventory = InventoryManager.elsterItems.Count;
            if (currentNumItemsInInventory != numItemsInInventory)
            {
                if (string.Equals(__instance._item._item.ToString(), "FalkeSpear", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                MelonLogger.Msg($"Removed item {__instance._item.name} from inventory");
                var item = __instance._item;
                InventoryManager.RemoveItem(item);
                //TODO: Call Archipelago API for check
            }

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

    [HarmonyPatch(typeof(END_Manager), "EvaluateEnding")]
    public static class DetectEnding
    {
        private static void Prefix()
        {
            MelonLogger.Msg("Ending triggered");
        }
    }

    [HarmonyPatch(typeof(Steganography), "Encode")]
    public static class DetectSteganography
    {
        private static void Prefix(Texture2D image, ref string message)
        {
            MelonLogger.Msg("Steganography triggered");
            MelonLogger.Msg($"Message: {message}");

            // Parse the JSON message
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(message);

            // Add the new string to the strings array
            var strings = jsonObject["strings"] as JArray;
            strings.Add("Item1,Item2,Item3");

            // Add the new stringKey to the stringKeys array
            var stringKeys = jsonObject["stringKeys"] as JArray;
            stringKeys.Add("archipelagoData");

            // Serialize the modified JSON back to a string
            message = JsonConvert.SerializeObject(jsonObject);
            MelonLogger.Msg($"Modified Message: {message}");
        }
    }

    [HarmonyPatch(typeof(SaveManager), "Load")]
    public static class LoadSave
    {
        private static void Postfix()
        {
            MelonLogger.Msg("Loading save");
            MelonLogger.Msg(SProgress.GetString("archipelagoData", ""));
        }
    }

    [HarmonyPatch(typeof(ROT_MeatBlocker), "Block")]
    public static class TurnOffMeatBlockers
    {
        public static void Postfix(ROT_MeatBlocker __instance)
        {
            MelonLogger.Msg("Attempting to turn off meat blockers");
            MelonLogger.Msg(__instance.blocked);
            __instance.blocked = false;
            __instance.enabled = false;
            SProgress.SetBool("Meat_" + __instance.ID, __instance.blocked);


            foreach (var blocker in __instance.Blockers)
            {
                blocker.SetActive(false);
            }
            foreach (var unblocker in __instance.UnBlockers)
            {
                unblocker.SetActive(true);
            }

            foreach (var connectedDoor in __instance.Locks)
            {
                connectedDoor.locked = false;
                connectedDoor.BacktrackDoor = true;
                // could be this need to test
                connectedDoor.externalUnlocker = true;
                connectedDoor.UpdateProperties();
            }
        }
        
    }
}
