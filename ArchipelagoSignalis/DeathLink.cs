using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace ArchipelagoSignalis
{
    class DeathLink
    {
        public static bool sendDeathLinkOnce = true;
        public static void CheckDeath()
        {
            if (PlayerState.gameOver && sendDeathLinkOnce)
            {
                MelonLogger.Msg("Sending Death Link");
                ArchipelagoHelper.DeathLinkService.SendDeathLink(new Archipelago.MultiClient.Net.BounceFeatures.DeathLink.DeathLink(ArchipelagoHelper.SlotName, "Elster died"));
                sendDeathLinkOnce = false;
            }

            if (!PlayerState.gameOver)
            {
                sendDeathLinkOnce = true;
            }
            
            // if (Input.GetKeyDown(KeyCode.F3))
            // {
            //     MelonLogger.Msg("Received Death Link");
            //     PlayerState.HurtPlayer.Invoke(10000, Vector2.down);
            //     PlayerState.HurtPlayer.Invoke(10000, Vector2.down);
            //     PlayerState.HurtPlayer.Invoke(10000, Vector2.down);
            //     PlayerState.HurtPlayer.Invoke(10000, Vector2.down);
            //     PlayerState.HurtPlayer.Invoke(10000, Vector2.down);
            // }
            
        }

        public static void KillElster()
        {
            MelonLogger.Msg("Received Death Link");
            PlayerState.HurtPlayer.Invoke(10000, Vector2.down);
            PlayerState.HurtPlayer.Invoke(10000, Vector2.down);
            PlayerState.HurtPlayer.Invoke(10000, Vector2.down);
            PlayerState.HurtPlayer.Invoke(10000, Vector2.down);
            PlayerState.HurtPlayer.Invoke(10000, Vector2.down);
        }
    }
}
