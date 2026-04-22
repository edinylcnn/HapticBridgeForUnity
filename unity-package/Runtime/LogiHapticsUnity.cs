using System;

namespace LogiHaptics
{
    public static class LogiHapticsUnity
    {
        static IHapticService _service;
        static readonly object _lock = new object();

        public static IHapticService Service
        {
            get
            {
                if (_service != null) return _service;
                lock (_lock)
                {
                    if (_service == null) _service = new LogiHapticsService();
                }
                return _service;
            }
        }

        public static bool IsAvailable => Service.IsAvailable;

        public static void Trigger(HapticEvent evt) => Service.Trigger(evt);

        public static void TriggerRaw(string eventName) => Service.TriggerRaw(eventName);

        public static void Override(IHapticService service)
        {
            lock (_lock)
            {
                _service?.Dispose();
                _service = service ?? new NullHapticsService();
            }
        }

        public static void Shutdown()
        {
            lock (_lock)
            {
                _service?.Dispose();
                _service = null;
            }
        }
    }
}
