namespace Loupedeck.HapticBridgeForUnityPlugin
{
    using System;
    using System.Threading.Tasks;

    public class HapticBridgeForUnityPlugin : Plugin
    {
        public override Boolean UsesApplicationApiOnly => true;
        public override Boolean HasNoApplication => true;

        PipeServer _pipe;

        public HapticBridgeForUnityPlugin()
        {
            PluginLog.Init(this.Log);
        }

        public override void Load()
        {
            RegisterHapticEvents();
            _pipe = new PipeServer(HandleEvent);
            _pipe.Start();
            PluginLog.Info($"HapticBridgeForUnity pipe server listening on '{PipeServer.PipeName}'");
            this.OnPluginStatusChanged(Loupedeck.PluginStatus.Normal, null);
        }

        public override void Unload()
        {
            _pipe?.Dispose();
            _pipe = null;
        }

        void RegisterHapticEvents()
        {
            // The host (Logi Options+) requires each haptic waveform to be registered as a plugin event.
            foreach (var waveform in HapticMapper.Waveforms)
            {
                this.PluginEvents.AddEvent(waveform, waveform, null);
            }
        }

        void HandleEvent(string eventName)
        {
            if (!HapticMapper.TryGetWaveform(eventName, out var waveform))
            {
                PluginLog.Warning($"unknown event from pipe: {eventName}");
                return;
            }

            // RaiseEvent drives the actual haptic pulse on the paired device (MX Master 4).
            Task.Run(() =>
            {
                try { this.PluginEvents.RaiseEvent(waveform); }
                catch (Exception ex) { PluginLog.Error(ex, $"haptic raise failed: {waveform}"); }
            });
        }
    }
}
