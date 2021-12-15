using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HelloWorldManager : MonoBehaviour
{
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();
            SubmitNewPositions();
        }

        GUILayout.EndArea();
    }

    private void StartButtons()
    {
        if (GUILayout.Button("Host"))
        {
            NetworkManager.Singleton.StartHost();
        }
        if (GUILayout.Button("Client"))
        {
            NetworkManager.Singleton.StartClient();
        }
        if (GUILayout.Button("Server"))
        {
            NetworkManager.Singleton.StartServer();
        }
    }
    private void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost
            ? "Host" : NetworkManager.Singleton.IsServer
                ? "Server" : "Client";
        GUILayout.Label($"Transport: {NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name}");
        GUILayout.Label($"Model: {mode}");
    }
    private void SubmitNewPositions()
    {
        if (GUILayout.Button(NetworkManager.Singleton.IsServer ? "Move" : "Request Move"))
        {
            var player = NetworkManager.Singleton.SpawnManager
                .GetLocalPlayerObject()
                .GetComponent<HelloWorldPlayer>();
            player.Move();
        }
    }
}
