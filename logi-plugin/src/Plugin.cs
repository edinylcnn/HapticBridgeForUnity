using System;

namespace LogiHapticsUnity.Plugin
{
    // Entry point for the Logi Options+ plugin.
    // TODO: Inherit from the Logi Actions SDK plugin base class and wire its lifecycle
    // hooks (OnLoad/OnUnload) to Start()/Dispose() below once the SDK is added.
    public sealed class LogiHapticsUnityPlugin : IDisposable
    {
        PipeServer _server;

        public void Start()
        {
            _server = new PipeServer(HandleEvent);
            _server.Start();
            Console.WriteLine($"[LogiHapticsUnity] pipe server started on '{PipeServer.PipeName}'");
        }

        void HandleEvent(string eventName)
        {
            if (!HapticMapper.TryGetWaveform(eventName, out var waveform))
            {
                Console.WriteLine($"[LogiHapticsUnity] unknown event: {eventName}");
                return;
            }
            PlayHaptic(waveform);
        }

        void PlayHaptic(string waveformId)
        {
            // TODO: Replace with Logi Actions SDK haptic trigger call, e.g.
            //   _sdk.Device.PlayHaptic(waveformId);
            Console.WriteLine($"[LogiHapticsUnity] play haptic: {waveformId}");
        }

        public void Dispose() => _server?.Dispose();
    }
}
