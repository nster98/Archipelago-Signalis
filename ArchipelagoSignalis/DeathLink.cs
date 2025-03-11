using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                sendDeathLinkOnce = false;
            }

            if (!PlayerState.gameOver)
            {
                sendDeathLinkOnce = true;
            }
            
            if (Input.GetKeyDown(KeyCode.F3))
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
}
