using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace ClientDriven.Editor
{
    public enum NetcodePlayModeStartupOption
    {
        None = 0, Server, Client, Host
    }

    [InitializeOnLoad]
    public static class NetcodePlayModeEditorHelper
    {
        private static string NetcodePlaymodeStartupOptionKey => $"NetcodeGameObjects_{Application.productName}_NetcodePlayModeStartupOption";
        public static NetcodePlayModeStartupOption NetCodeMode
        {
            get => (NetcodePlayModeStartupOption)EditorPrefs.GetInt(NetcodePlaymodeStartupOptionKey, 0);
            set => EditorPrefs.SetInt(NetcodePlaymodeStartupOptionKey, (int)value);
        }

        static NetcodePlayModeEditorHelper()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                switch (NetCodeMode)
                {
                    case NetcodePlayModeStartupOption.None:
                        break;
                    case NetcodePlayModeStartupOption.Host:
                        NetworkManager.Singleton.StartHost();
                        break;
                    case NetcodePlayModeStartupOption.Server:
                        NetworkManager.Singleton.StartServer();
                        break;
                    case NetcodePlayModeStartupOption.Client:
                        NetworkManager.Singleton.StartClient();
                        break;
                }
            }
        }
    }
}
