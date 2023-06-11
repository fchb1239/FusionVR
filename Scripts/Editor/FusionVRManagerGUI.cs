#if UNITY_EDITOR
using System;
using System.Net;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Fusion.VR.Editor
{
    [CustomEditor(typeof(FusionVRManager))]
    public class FusionVRManagerGUI : UnityEditor.Editor
    {
        private static Texture2D logo;

        public override void OnInspectorGUI()
        {
            if (logo == null)
                logo = Resources.Load<Texture2D>("FusionVR/Assets/FusionVRLogoNoBackSmall");
            GUILayout.Label(new GUIContent() { image = logo });

            FusionVRManager manager = (FusionVRManager)target;

            base.OnInspectorGUI();
            GUILayout.Space(10);

            // Set some default values
            if (PrefabStageUtility.GetCurrentPrefabStage() == null)
            {
                manager.CheckDefaultValues();
            }

            if (manager.Runner != null)
            {
                if (manager.Runner.IsConnectedToServer)
                {
                    GUILayout.Label($"State: {manager.Runner.State}");
                    //GUILayout.Label($"Ping: {manager.Runner.State}");
                    //GUILayout.Label($"Room: {(PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.Name : "Not in a room")}");
                    if (!manager.JoinRoomOnConnect)
                        if (GUILayout.Button(CreateContent("Join", "Join a random public lobby")))
                            FusionVRManager.JoinRandomRoom("Default", 16);
                }
                else if (!manager.ConnectOnAwake)
                    if (GUILayout.Button("Connect"))
                        FusionVRManager.Connect();
            }
        }

        private bool CheckForRig(FusionVRManager manager)
        {
            GameObject[] objects = FindObjectsOfType<GameObject>();

            bool b = false;

            if (manager.Head == null)
            {
                b = true;
                foreach (GameObject obj in objects)
                {
                    if (obj.name.Contains("Camera") || obj.name.Contains("Head"))
                    {
                        manager.Head = obj.transform;
                        break;
                    }
                }
            }

            if (manager.LeftHand == null)
            {
                b = true;
                foreach (GameObject obj in objects)
                {
                    if (obj.name.Contains("Left") && (obj.name.Contains("Hand") || obj.name.Contains("Controller")))
                    {
                        manager.LeftHand = obj.transform;
                        break;
                    }
                }
            }

            if (manager.RightHand == null)
            {
                b = true;
                foreach (GameObject obj in objects)
                {
                    if (obj.name.Contains("Right") && (obj.name.Contains("Hand") || obj.name.Contains("Controller")))
                    {
                        manager.RightHand = obj.transform;
                        break;
                    }
                }
            }

            return b;
        }

        private GUIContent CreateContent(string text, string tooltip)
        {
            return new GUIContent()
            {
                text = text,
                tooltip = tooltip
            };
        }
    }
}
#endif