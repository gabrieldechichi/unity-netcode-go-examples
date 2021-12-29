using UnityEditor;

namespace ClientDriven.Editor
{
    public class NetcodePlaymodeWindow : EditorWindow
    {
        [MenuItem("Netcode/Playmode Configuration")]
        public static void ShowWindow()
        {
            GetWindow<NetcodePlaymodeWindow>(false, "Playmode Configuration", true);
        }

        private void OnGUI()
        {
            NetcodePlayModeEditorHelper.NetCodeMode = (NetcodePlayModeStartupOption)EditorGUILayout.EnumPopup(
                "Play Mode Config: ",
                (NetcodePlayModeStartupOption)NetcodePlayModeEditorHelper.NetCodeMode);
        }
    }
}

