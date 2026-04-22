using System.Collections.Generic;

namespace LogiHapticsUnity.Plugin
{
    public static class HapticMapper
    {
        // Event name (snake_case, from pipe) → Logi Actions SDK waveform id.
        // Waveform ids follow the SDK's named haptic library; adjust as SDK exposes them.
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
            => Map.TryGetValue(eventName, out waveform);

        public static void Register(string eventName, string waveform)
            => Map[eventName] = waveform;
    }
}
