using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkCommandLine : MonoBehaviour
{
    private class NetworkCommandLineArgs
    {
        public const string Mlapi = "-mlapi";
    }

    private class MlapiArgValue
    {
        public const string Server = "server";
        public const string Client = "client";
        public const string Host = "host";
    }

    [SerializeField] private NetworkManager networkManager;
    private void Start()
    {
        if (Application.isEditor)
        {
            return;
        }

        var args = GetCommandLineArgs();
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
}
