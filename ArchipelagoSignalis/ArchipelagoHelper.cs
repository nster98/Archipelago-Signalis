using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using MelonLoader;

namespace ArchipelagoSignalis
{
    class ArchipelagoHelper
    {
        public static string SlotName = "Signalis";
        public static string Server = "localhost";
        public static string Port = "38281";
        public static string Password = "";
        public const string GameName = "Signalis";

        public static ArchipelagoSession Session;
        public static DeathLinkService DeathLinkService;

        public static void InitializeArchipelago()
        {
            Session = ArchipelagoSessionFactory.CreateSession(Server, int.Parse(Port));
            Task<LoginResult> loginResultTask = null;
            try
            {
                MelonLogger.Msg("Attempting to Connect");
                Task<RoomInfoPacket> connection = Session.ConnectAsync();
                connection.Wait();

                MelonLogger.Msg("Attempting to Login");
                loginResultTask = Session.LoginAsync(GameName, SlotName, ItemsHandlingFlags.AllItems,
                    password: Password);
                loginResultTask.Wait();

                MelonLogger.Msg("Logged In!");

            }
            catch (Exception ex)
            {
                var loginDone = loginResultTask.Result;
                loginDone = new LoginFailure(ex.GetBaseException().Message);
                LoginFailure failure = (LoginFailure)loginDone;
                string errorMessage = $"Failed to Connect to {Server + ":" + Port} as {SlotName}:";
                foreach (string error in failure.Errors)
                {
                    errorMessage += $"\n    {error}";
                }

                foreach (ConnectionRefusedError error in failure.ErrorCodes)
                {
                    errorMessage += $"\n    {error}";
                }

                MelonLogger.Msg(errorMessage);
                return;
            }

            MelonLogger.Msg($"Login was successful: {loginResultTask.Result}");
            if (loginResultTask.Result.Successful)
            {
                MelonLogger.Msg($"Connected to {Server + ":" + Port} as {SlotName}");
                RetrieveItemsAfterLogin(Session);
                PopulateItemsCollectedAfterLogin(Session);
                SendItemsAfterLogin(Session);
                RetrieveItem.ListenForItemReceived(Session);
                
                var isDeathLinkEnabled = Session.ConnectionInfo.Tags.Contains("DeathLink");
                if (isDeathLinkEnabled)
                {
                    DeathLinkService = Session.CreateDeathLinkService();
                    DeathLinkService.EnableDeathLink();

                    DeathLinkService.OnDeathLinkReceived += (deathLinkObject) =>
                    {
                        MelonLogger.Msg($"Player {deathLinkObject.Source} has died.");
                        DeathLink.KillElster();
                    };

                }
            }
            else
            {
                Session = null;
            }
        }

        private static void PopulateItemsCollectedAfterLogin(ArchipelagoSession session)
        {
            foreach (long locationId in session.Locations.AllLocationsChecked)
            {
                var itemsCollected = SaveManagement.LocationsChecked.Split(',').ToList();

                MelonLogger.Msg($"Location checked: {locationId}");
                string itemName = session.Locations.GetLocationNameFromId(locationId);
                // string translatedItemName = ArchipelagoStart.GetSignalisItemNameFromArchipelagoLocation(itemName);

                if (!itemsCollected.Contains(itemName))
                {
                    SaveManagement.UpdateLocationsChecked(itemName);
                }
            }
        }

        public static void SendItemsAfterLogin(ArchipelagoSession session)
        {
            // TODO: Test if this works, may need to be async
            var lastItemSent = session.DataStorage["LastItemSent"];
            var itemsCollected = SaveManagement.LocationsChecked.Split(',').ToList();
            int itemIndex = itemsCollected.IndexOf(lastItemSent);

            if (itemIndex != -1 && itemIndex < itemsCollected.Count - 1)
            {
                var itemsToSend = itemsCollected.Skip(itemIndex + 1).ToList();
                foreach (var itemToSend in itemsToSend)
                {
                    MelonLogger.Msg($"Sending item {itemToSend} to Archipelago");
                    SendLocation.SendCheckToArchipelago(itemToSend, "");
                }
            }
            
        }

        public static void RetrieveItemsAfterLogin(ArchipelagoSession session)
        {
            int archipelagoItemsSize = session.Items.AllItemsReceived.Count;
            int gameItemsSize = SaveManagement.ItemsReceived.Split(',').Length - 1;

            MelonLogger.Msg($"Archipelago Items List {string.Join(",", session.Items.AllItemsReceived.Select(item => item.ItemName))}");
            MelonLogger.Msg($"Archipelago items count: {archipelagoItemsSize}, Game items count: {gameItemsSize}");

            if (archipelagoItemsSize > gameItemsSize)
            {
                MelonLogger.Msg($"Retrieving {archipelagoItemsSize - gameItemsSize} items from Archipelago");
                List<ItemInfo> itemsToReceive = session.Items.AllItemsReceived.Skip(gameItemsSize).ToList();
                foreach (ItemInfo item in itemsToReceive)
                {
                    RetrieveItem.AddItemToInventory(ArchipelagoStart.GetSignalisItemName(item.ItemName));
                }
            }
        }

        public static void ConnectToArchipelagoOnBeginAnew(string sceneName)
        {
            if (sceneName == "PEN_Wreck" && (null == Session || !Session.Socket.Connected))
            {
                SaveManagement.ResetItemsReceived();
                MelonLogger.Msg("Connecting to Archipelago on Begin Anew");
                Task.Run(InitializeArchipelago);
            }
        }

        public static void CheckDisconnectFromArchipelago(string sceneName)
        {
            if (sceneName == "MainMenu" && null != Session)
            {
                Session.Socket.DisconnectAsync();
            }
        }
    }
}
