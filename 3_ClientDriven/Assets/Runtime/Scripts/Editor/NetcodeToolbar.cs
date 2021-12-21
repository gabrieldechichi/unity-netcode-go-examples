using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace ClientDriven.Editor
{
    [InitializeOnLoad]
    public static class NetcodeToolbar
    {
        private enum NetcodePlayModeStartupOption
        {
            None = 0, Server, Client, Host
        }

        private static NetcodePlayModeStartupOption NetCodeMode
        {
            get => (NetcodePlayModeStartupOption)EditorPrefs.GetInt("NetcodePlayModeStartupOption", 0);
            set => EditorPrefs.SetInt("NetcodePlayModeStartupOption", (int)value);
        }

        static NetcodeToolbar()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        [MenuItem("Netcode/Play/As Host")]
        public static void PlayAsHost()
        {
            if (!EditorApplication.isPlaying)
            {
                NetCodeMode = NetcodePlayModeStartupOption.Host;
                EditorApplication.isPlaying = true;
            }
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
