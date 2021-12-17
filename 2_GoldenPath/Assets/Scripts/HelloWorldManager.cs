using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class HelloWorldManager : MonoBehaviour
    {
        bool IsMultiplayerRunning => NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient;

        private void OnGUI()
        {
            using (new GUILayout.AreaScope(new Rect(10, 10, 300, 300)))
            {
                if (IsMultiplayerRunning)
                {
                    DrawStatus();
                    DrawMultiplayerRunningButtons();

                }
                else
                {
                    DrawStartButtons();
                }
            }

        }

        private void DrawMultiplayerRunningButtons()
        {
            if (GUILayout.Button("Set random position"))
            {
                HelloWorldPlayer player = null;
                if (NetworkManager.Singleton.IsClient)
                {

                    player = NetworkManager.Singleton.SpawnManager
                        .GetLocalPlayerObject()
                        .GetComponent<HelloWorldPlayer>();
                }
                else
                {
                    player = NetworkManager.Singleton.ConnectedClients
                        .Values
                        .First()
                        .PlayerObject
                        .GetComponent<HelloWorldPlayer>();
                }
                player?.Move();
            }
        }

        private void DrawStatus()
        {
            var mode = NetworkManager.Singleton.IsHost
                ? "Host"
                : NetworkManager.Singleton.IsServer
                    ? "Server" : "Client";
            GUILayout.Label($"Runnig as: {mode}\nTransport: {NetworkManager.Singleton.NetworkConfig.NetworkTransport}");
        }

        private static void DrawStartButtons()
        {
            if (GUILayout.Button("Host"))
            {
                NetworkManager.Singleton.StartHost();
            }
            if (GUILayout.Button("Server"))
            {
                NetworkManager.Singleton.StartServer();
            }
            if (GUILayout.Button("Client"))
            {
                NetworkManager.Singleton.StartClient();
            }
        }
    }

}