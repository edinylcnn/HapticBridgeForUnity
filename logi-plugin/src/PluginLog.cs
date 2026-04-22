namespace Loupedeck.HapticBridgeForUnityPlugin
{
    using System;

    internal static class PluginLog
    {
        static PluginLogFile _log;

        public static void Init(PluginLogFile log) => _log = log;

        public static void Verbose(string text) => _log?.Verbose(text);
        public static void Info(string text) => _log?.Info(text);
        public static void Warning(string text) => _log?.Warning(text);
        public static void Error(string text) => _log?.Error(text);
        public static void Error(Exception ex, string text) => _log?.Error(ex, text);
    }
}
