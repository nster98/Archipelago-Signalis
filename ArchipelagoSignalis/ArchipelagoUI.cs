using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngineInternal.Input;

namespace ArchipelagoSignalis
{
    class ArchipelagoUI
    {
        public static bool InSettingsMenu = false;
        private static int pointX = 260;
        private static int pointY = 280;

        public static void RenderArchipelagoSettingsUi()
        {
            if (InSettingsMenu)
            {
                    GUILayout.BeginArea(new Rect(Camera.current.pixelWidth - pointX, pointY, 250, 200));

                    GUILayout.BeginVertical(null);

                    // Create an area for slot name
                    GUILayout.BeginHorizontal(null);
                    GUILayout.Label("Slot Name", null);
                    ArchipelagoHelper.SlotName = GUILayout.TextField(ArchipelagoHelper.SlotName, 50, null);
                    GUILayout.EndHorizontal();

                    // Create an area for server name
                    GUILayout.BeginHorizontal(null);
                    GUILayout.Label("Server", null);
                    ArchipelagoHelper.Server = GUILayout.TextField(ArchipelagoHelper.Server, 50, null);
                    GUILayout.EndHorizontal();

                    // Create an area for port number
                    GUILayout.BeginHorizontal(null);
                    GUILayout.Label("Port", null);
                    ArchipelagoHelper.Port = GUILayout.TextField(ArchipelagoHelper.Port, 50, null);
                    GUILayout.EndHorizontal();

                    // Create an area for password
                    GUILayout.BeginHorizontal(null);
                    GUILayout.Label("Password", null);
                    ArchipelagoHelper.Password = GUILayout.TextField(ArchipelagoHelper.Password, 50, null);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(null);
                    GUILayout.Label("DeathLink", null);
                    ArchipelagoHelper.IsDeathLinked = GUILayout.Toggle(ArchipelagoHelper.IsDeathLinked, "", null);
                    GUILayout.EndHorizontal();

                    GUIStyle style = new GUIStyle();
                    // style.font = Font.CreateDynamicFontFromOSFont("visitor2", 24);

                    GUILayout.BeginHorizontal(null);
                    GUILayout.Label("Custom Keybinds for Archipelago", null);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(null);
                    GUILayout.Label("F8 - Opens Level Select", null);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(null);
                    GUILayout.Label("F11 - Open Storage Box From Anywhere", null);
                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();
                    GUILayout.EndArea();
                
            }
        }
    }

    [HarmonyPatch(typeof(NewMainMenu), "openOptions")]
    public static class InOptionsMenu
    {
        public static void Postfix()
        {
            ArchipelagoUI.InSettingsMenu = true;
        }
    }

    [HarmonyPatch(typeof(NewMainMenu), "closeOptions")]
    public static class OutOptionsMenu
    {
        public static void Postfix()
        {
            ArchipelagoUI.InSettingsMenu = false;
        }
    }
}
