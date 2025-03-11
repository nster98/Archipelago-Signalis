using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace ArchipelagoSignalis
{
    class ArchipelagoUI
    {
        public static bool InSettingsMenu = false;
        private static bool _isEditing = false;

        public static void RenderArchipelagoSettingsUi()
        {
            if (InSettingsMenu)
            {
                GUILayout.BeginArea(new Rect(5, 5, 300, 200));

                GUILayout.BeginVertical(null);

                // Create an area for slot name
                GUILayout.BeginHorizontal(null);
                GUILayout.Label("Slot Name", null);
                ArchipelagoStart.SlotName = GUILayout.TextField(ArchipelagoStart.SlotName, 50, null);
                GUILayout.EndHorizontal();

                // Create an area for server name
                GUILayout.BeginHorizontal(null);
                GUILayout.Label("Server", null);
                ArchipelagoStart.Server = GUILayout.TextField(ArchipelagoStart.Server, 50, null);
                GUILayout.EndHorizontal();

                // Create an area for port number
                GUILayout.BeginHorizontal(null);
                GUILayout.Label("Port", null);
                ArchipelagoStart.Port = GUILayout.TextField(ArchipelagoStart.Port, 50, null);
                GUILayout.EndHorizontal();

                // Create an area for password
                GUILayout.BeginHorizontal(null);
                GUILayout.Label("Password", null);
                ArchipelagoStart.Password = GUILayout.TextField(ArchipelagoStart.Password, 50, null);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(null);
                if (_isEditing)
                {
                    // Create a button to stop editing
                    if (GUILayout.Button("Done", null))
                    {
                        Time.timeScale = 1;
                        _isEditing = false;
                    }
                }
                else
                {
                    // Create a button to start editing
                    if (GUILayout.Button("Edit", null))
                    {
                        Time.timeScale = 0;
                        _isEditing = true;
                    }
                }
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
