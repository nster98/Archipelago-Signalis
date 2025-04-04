using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using UnityEngineInternal.Input;

namespace ArchipelagoSignalis
{
    class ArchipelagoUI
    {
        public static bool InSettingsMenu = false;
        private static bool _isEditing = false;
        private static int pointX = 5;
        private static int pointY = 305;

        public static void RenderArchipelagoSettingsUi()
        {
            if (InSettingsMenu)
            {
                if (!_isEditing)
                {
                    GUILayout.BeginArea(new Rect(pointX, pointY, 250, 200));
                    GUILayout.BeginHorizontal(null);
                    if (GUILayout.Button("Enter Archipelago Connection", null))
                    {
                        pointX = Camera.current.pixelWidth / 2 - 125;
                        ArchipelagoStart.settingsMenuGameObject.SetActive(false);
                        _isEditing = true;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.EndArea();
                }
                else
                {
                    GUILayout.BeginArea(new Rect(pointX, pointY, 250, 200));

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
                    if (_isEditing)
                    {
                        // Create a button to stop editing
                        if (GUILayout.Button("Done", null))
                        {
                            pointX = 5;
                            pointY = 305;
                            ArchipelagoStart.settingsMenuGameObject.SetActive(true);
                            _isEditing = false;
                        }
                    }
                    // else
                    // {
                    //     // Create a button to start editing
                    //     if (GUILayout.Button("Edit", null))
                    //     {
                    //         pointX = Camera.current.pixelWidth / 2 - 125;
                    //         ArchipelagoStart.settingsMenuGameObject.SetActive(false);
                    //         _isEditing = true;
                    //     }
                    // }
                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();
                    GUILayout.EndArea();
                }
                
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
