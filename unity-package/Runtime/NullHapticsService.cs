namespace LogiHaptics
{
    public sealed class NullHapticsService : IHapticService
    {
        public bool IsAvailable => false;
        public void Trigger(HapticEvent evt) { }
        public void TriggerRaw(string eventName) { }
        public void Dispose() { }
    }
}
