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
                ProcessItemsAfterLogin(Session);

            }
        }
        public static void ProcessItemsAfterLogin(ArchipelagoSession session)
        {
            foreach (ItemInfo item in session.Items.AllItemsReceived)
            {
                // TODO: Only send items after last recieved item retrieved from Save Data
                RetrieveItem.AddItemToInventory(item.ItemName);
            }
        }
    }
}
