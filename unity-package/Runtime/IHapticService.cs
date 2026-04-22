using System;

namespace LogiHaptics
{
    public interface IHapticService : IDisposable
    {
        bool IsAvailable { get; }
        void Trigger(HapticEvent evt);
        void TriggerRaw(string eventName);
    }
}
