using UnityEditor;
using UnityEngine;

namespace HapticBridge.Editor
{
    public class HapticsBridgeChecker : EditorWindow
    {
        static readonly HapticEvent[] AllEvents = (HapticEvent[])System.Enum.GetValues(typeof(HapticEvent));
        HapticBridgeService _probe;
        string _status = "Unknown — press Connect";

        [MenuItem("Window/HapticBridge/Test Panel")]
        public static void Open() => GetWindow<HapticsBridgeChecker>("HapticBridge");

        void OnDisable() { _probe?.Dispose(); _probe = null; }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Plugin Pipe", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Pipe name: {HapticBridgeService.PipeName}");
            EditorGUILayout.LabelField($"Temp path: {System.IO.Path.GetTempPath()}");
            EditorGUILayout.LabelField($"Status: {_status}");
            if (_probe != null && !string.IsNullOrEmpty(_probe.LastError))
                EditorGUILayout.HelpBox($"Last error: {_probe.LastError}", MessageType.Warning);

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
                    "Plugin pipe not connected. Install HapticBridgeForUnity_x.y.lplug4 in Logi Options+ and press Connect.",
                    MessageType.Info);
            }
        }

        void Probe()
        {
            _probe?.Dispose();
            _probe = new HapticBridgeService();
            var ok = _probe.TryConnect();
            _status = ok ? "Connected" : $"Could not connect — {_probe.LastError}";
            UnityEngine.Debug.Log($"[HapticBridge] Connect ok={ok} tmp={System.IO.Path.GetTempPath()} lastError={_probe.LastError}");
            Repaint();
        }
    }
}
