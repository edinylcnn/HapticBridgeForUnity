using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace LogiHaptics
{
    public sealed class LogiHapticsService : IHapticService
    {
        public const string PipeName = "LogiHapticsUnity";
        const int ConnectTimeoutMs = 50;

        readonly object _lock = new object();
        NamedPipeClientStream _pipe;
        StreamWriter _writer;
        bool _connectAttempted;
        bool _disposed;

        public bool IsAvailable
        {
            get
            {
                lock (_lock) { return _pipe != null && _pipe.IsConnected; }
            }
        }

        public bool TryConnect()
        {
            lock (_lock)
            {
                if (_disposed) return false;
                DisposePipeLocked();
                _connectAttempted = false;
                return EnsureConnectedLocked();
            }
        }

        public void Trigger(HapticEvent evt) => TriggerRaw(EventName(evt));

        public void TriggerRaw(string eventName)
        {
            if (_disposed || string.IsNullOrEmpty(eventName)) return;
            Task.Run(() => SendLine(eventName));
        }

        void SendLine(string line)
        {
            try
            {
                lock (_lock)
                {
                    if (_disposed) return;
                    if (!EnsureConnectedLocked()) return;
                    _writer.WriteLine(line);
                    _writer.Flush();
                }
            }
            catch (Exception)
            {
                ResetConnection();
            }
        }

        bool EnsureConnectedLocked()
        {
            if (_pipe != null && _pipe.IsConnected) return true;
            if (_connectAttempted && _pipe == null) return false;

            DisposePipeLocked();
            _connectAttempted = true;

            try
            {
                var pipe = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                pipe.Connect(ConnectTimeoutMs);
                _pipe = pipe;
                _writer = new StreamWriter(pipe) { AutoFlush = false };
                return true;
            }
            catch (TimeoutException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
        }

        void ResetConnection()
        {
            lock (_lock) { DisposePipeLocked(); _connectAttempted = false; }
        }

        void DisposePipeLocked()
        {
            try { _writer?.Dispose(); } catch { }
            try { _pipe?.Dispose(); } catch { }
            _writer = null;
            _pipe = null;
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed) return;
                _disposed = true;
                DisposePipeLocked();
            }
        }

        static string EventName(HapticEvent evt)
        {
            switch (evt)
            {
                case HapticEvent.Click:         return "click";
                case HapticEvent.Confirm:       return "confirm";
                case HapticEvent.Success:       return "success";
                case HapticEvent.Failure:       return "failure";
                case HapticEvent.Warning:       return "warning";
                case HapticEvent.Notification:  return "notification";
                case HapticEvent.Achievement:   return "achievement";
                case HapticEvent.ImpactLight:   return "impact_light";
                case HapticEvent.ImpactMedium:  return "impact_medium";
                default:                        return evt.ToString().ToLowerInvariant();
            }
        }
    }
}
