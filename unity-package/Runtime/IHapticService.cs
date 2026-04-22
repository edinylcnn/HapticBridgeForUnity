using System;

namespace HapticBridge
{
    public interface IHapticService : IDisposable
    {
        bool IsAvailable { get; }
        void Trigger(HapticEvent evt);
        void TriggerRaw(string eventName);
    }
}
