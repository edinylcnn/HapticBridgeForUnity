namespace Loupedeck.HapticBridgeForUnityPlugin
{
    using System.Collections.Generic;

    public static class HapticMapper
    {
        // The 15 waveform ids exposed by the Logi Actions SDK haptic event system.
        // Must mirror package/events/DefaultEventSource.yaml.
        public static readonly string[] Waveforms =
        {
            "sharp_collision",
            "sharp_state_change",
            "knock",
            "damp_collision",
            "mad",
            "ringing",
            "subtle_collision",
            "completed",
            "jingle",
            "damp_state_change",
            "firework",
            "happy_alert",
            "wave",
            "angry_alert",
            "square"
        };

        // Generic event name (from Unity side) → Logi SDK waveform id.
        static readonly Dictionary<string, string> Map = new Dictionary<string, string>
        {
            { "click",          "subtle_collision" },
            { "confirm",        "jingle" },
            { "success",        "completed" },
            { "failure",        "mad" },
            { "warning",        "damp_state_change" },
            { "notification",   "happy_alert" },
            { "achievement",    "firework" },
            { "impact_light",   "subtle_collision" },
            { "impact_medium",  "sharp_collision" }
        };

        public static bool TryGetWaveform(string eventName, out string waveform)
        {
            // Accept direct waveform ids as well — lets Unity side bypass the enum.
            if (System.Array.IndexOf(Waveforms, eventName) >= 0)
            {
                waveform = eventName;
                return true;
            }
            return Map.TryGetValue(eventName, out waveform);
        }

        public static void Register(string eventName, string waveform) => Map[eventName] = waveform;
    }
}
