using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Models;

namespace ArchipelagoSignalis
{
    class ArchipelagoHelper
    {
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
