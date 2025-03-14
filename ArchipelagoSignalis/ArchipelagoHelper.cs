using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using MelonLoader;

namespace ArchipelagoSignalis
{
    class ArchipelagoHelper
    {
        public static string SlotName = "NathanSIG";
        public static string Server = "archipelago.gg";
        public static string Port = "37509";
        public static string Password = "";
        public const string GameName = "Signalis";

        public static ArchipelagoSession Session;

        public static void InitializeArchipelago()
        {
            Session = ArchipelagoSessionFactory.CreateSession(Server, int.Parse(Port));
            Task<LoginResult> loginResultTask = null;
            try
            {
                Task<RoomInfoPacket> connection = Session.ConnectAsync();
                connection.Wait();

                loginResultTask = Session.LoginAsync(GameName, SlotName, ItemsHandlingFlags.AllItems,
                    password: Password);
                loginResultTask.Wait();

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
                SendItemsAfterLogin(Session);
                RetrieveItem.ListenForItemReceived(Session);
            }
            else
            {
                Session = null;
            }
        }

        public static void SendItemsAfterLogin(ArchipelagoSession session)
        {
            // TODO: Test if this works, may need to be async
            var lastItemSent = session.DataStorage["LastItemSent"];
            var itemsCollected = SaveManagement.ItemsCollected.Split(',').ToList();
            int itemIndex = itemsCollected.IndexOf(lastItemSent);

            if (itemIndex != -1 && itemIndex < itemsCollected.Count - 1)
            {
                var itemsToSend = itemsCollected.Skip(itemIndex + 1).ToList();
                foreach (var itemToSend in itemsToSend)
                {
                    MelonLogger.Msg($"Sending item {itemToSend} to Archipelago");
                    SendItem.SendCheckToArchipelago(itemToSend);
                }
            }
            
        }

        public static void RetrieveItemsAfterLogin(ArchipelagoSession session)
        {
            int archipelagoItemsSize = session.Items.AllItemsReceived.Count;
            int gameItemsSize = SaveManagement.ItemsReceived.Split(',').Length;

            if (archipelagoItemsSize > gameItemsSize)
            {
                MelonLogger.Msg($"Retrieving {archipelagoItemsSize - gameItemsSize} items from Archipelago");
                List<ItemInfo> itemsToReceive = session.Items.AllItemsReceived.Skip(gameItemsSize).ToList();
                foreach (ItemInfo item in itemsToReceive)
                {
                    RetrieveItem.AddItemToInventory(item.ItemName);
                }
            }
        }
    }
}
