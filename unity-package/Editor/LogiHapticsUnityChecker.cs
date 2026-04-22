using UnityEditor;
using UnityEngine;

namespace LogiHaptics.Editor
{
    public class LogiHapticsUnityChecker : EditorWindow
    {
        static readonly HapticEvent[] AllEvents = (HapticEvent[])System.Enum.GetValues(typeof(HapticEvent));
        LogiHapticsService _probe;
        string _status = "Unknown — press Connect";

        [MenuItem("Window/LogiHaptics/Test Panel")]
        public static void Open() => GetWindow<LogiHapticsUnityChecker>("LogiHaptics");

        void OnDisable() { _probe?.Dispose(); _probe = null; }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Plugin Pipe", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Pipe name: {LogiHapticsService.PipeName}");
            EditorGUILayout.LabelField($"Status: {_status}");

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Connect / Refresh")) Probe();
                if (GUILayout.Button("Disconnect")) { _probe?.Dispose(); _probe = null; _status = "Disconnected"; }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Trigger Events", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(_probe == null || !_probe.IsAvailable);
            foreach (var evt in AllEvents)
            {
                if (GUILayout.Button(evt.ToString())) _probe.Trigger(evt);
            }
            EditorGUI.EndDisabledGroup();

            if (_probe == null || !_probe.IsAvailable)
            {
                EditorGUILayout.HelpBox(
                    "Plugin pipe not connected. Install LogiHapticsUnity_x.y.lplug4 in Logi Options+ and press Connect.",
                    MessageType.Info);
            }
        }

        void Probe()
        {
            _probe?.Dispose();
            _probe = new LogiHapticsService();
            _status = _probe.TryConnect()
                ? "Connected"
                : "Could not connect — plugin not running?";
            Repaint();
        }
    }
}
