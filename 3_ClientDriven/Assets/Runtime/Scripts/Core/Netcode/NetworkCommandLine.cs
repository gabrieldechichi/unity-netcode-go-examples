using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkCommandLine : MonoBehaviour
{
    private class NetworkCommandLineArgs
    {
        public const string Mlapi = "-mlapi";
        public const string DelayMs = "-delayMs";
        public const string JitterMs = "-jitterMs";
        public const string DropRatePercent = "-dropRatePercent";
    }

    private class MlapiArgValue
    {
        public const string Server = "server";
        public const string Client = "client";
        public const string Host = "host";
    }

    private void Start()
    {
        if (Application.isEditor)
        {
            return;
        }

        var args = GetCommandLineArgs();
        HandleNetworkConditionArgs(args);
        HandleNetworkManagerArgs(args);
    }

    private void HandleNetworkManagerArgs(Dictionary<string, string> args)
    {
        var networkManager = NetworkManager.Singleton;
        if (args.TryGetValue(NetworkCommandLineArgs.Mlapi, out var mlapi))
        {
            switch (mlapi)
            {
                case MlapiArgValue.Server:
                    networkManager.StartServer();
                    break;
                case MlapiArgValue.Client:
                    networkManager.StartClient();
                    break;
                case MlapiArgValue.Host:
                    networkManager.StartHost();
                    break;
                default:
                    throw new System.NotImplementedException($"Unexpected MLAPI option: mlapi");
            }
        }
    }

    private void HandleNetworkConditionArgs(Dictionary<string, string> args)
    {
        //Code doesn't compile in editor (variables are set to private in UnityTransport.cs when in editor)
#if DEVELOPMENT_BUILD && !UNITY_EDITOR
        if (args.TryGetValue(NetworkCommandLineArgs.DelayMs, out var delayMsStr) &&
            int.TryParse(delayMsStr, out var delayMs))
        {
            UnityTransport.ClientPacketDelayMs = delayMs;
        }
        if (args.TryGetValue(NetworkCommandLineArgs.JitterMs, out var jitterMsStr) &&
            int.TryParse(jitterMsStr, out var jitterMs))
        {
            UnityTransport.ClientPacketJitterMs = jitterMs;
        }
        if (args.TryGetValue(NetworkCommandLineArgs.DropRatePercent, out var dropRateStr) &&
            int.TryParse(dropRateStr, out var dropRate))
        {
            UnityTransport.ClientPacketDropRate = dropRate;
        }
#endif
    }

    private Dictionary<string, string> GetCommandLineArgs()
    {
        var args = System.Environment.GetCommandLineArgs();
        var argsDict = new Dictionary<string, string>();
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            var nextArgIndex = i + 1;
            if (arg.StartsWith("-"))
            {
                var value = nextArgIndex < args.Length ? args[nextArgIndex] : null;
                value = (value?.StartsWith("-") ?? false) ? null : value;
                argsDict.Add(arg, value);
            }
        }

        return argsDict;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));

        var networkManager = NetworkManager.Singleton;
        var machineStatus = networkManager.IsHost
            ? "Host" : networkManager.IsClient
                ? "Client"
                : "Server";
        var networkTransport = networkManager.NetworkConfig.NetworkTransport.GetType().Name;
        GUILayout.TextField($"{machineStatus}\n{networkTransport}");
        GUILayout.EndArea();
    }

#endif
}

