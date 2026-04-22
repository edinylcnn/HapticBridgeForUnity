using System;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HapticBridge
{
    public sealed class HapticBridgeService : IHapticService
    {
        public const string PipeName = "HapticBridgeForUnity";
        const int ConnectTimeoutMs = 200;

        readonly object _lock = new object();
        readonly bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        Stream _stream;
        StreamWriter _writer;
        string _lastError;
        bool _connectAttempted;
        bool _disposed;

        public bool IsAvailable
        {
            get { lock (_lock) { return _stream != null; } }
        }

        public string LastError
        {
            get { lock (_lock) { return _lastError; } }
        }

        public bool TryConnect()
        {
            lock (_lock)
            {
                if (_disposed) return false;
                DisposeStreamLocked();
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
            catch (Exception ex)
            {
                lock (_lock) { _lastError = ex.Message; DisposeStreamLocked(); _connectAttempted = false; }
            }
        }

        bool EnsureConnectedLocked()
        {
            if (_stream != null) return true;
            if (_connectAttempted) return false;
            _connectAttempted = true;

            try
            {
                _stream = _isWindows ? ConnectWindows() : ConnectUnix();
                _writer = new StreamWriter(_stream) { AutoFlush = false };
                _lastError = null;
                return true;
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
                DisposeStreamLocked();
                return false;
            }
        }

        Stream ConnectWindows()
        {
            var pipe = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            pipe.Connect(ConnectTimeoutMs);
            return pipe;
        }

        Stream ConnectUnix()
        {
            // .NET named pipes on Unix map to a Unix Domain Socket at $TMPDIR/CoreFxPipe_<name>.
            // Unity's Mono runtime's NamedPipeClientStream does not always target this path,
            // so we connect directly via UnixDomainSocketEndPoint — matches the plugin's server.
            var tmp = Environment.GetEnvironmentVariable("TMPDIR");
            if (string.IsNullOrEmpty(tmp)) tmp = "/tmp/";
            if (!tmp.EndsWith("/")) tmp += "/";
            var socketPath = tmp + "CoreFxPipe_" + PipeName;

            var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            var endpoint = new UnixDomainSocketEndPoint(socketPath);

            var ar = socket.BeginConnect(endpoint, null, null);
            if (!ar.AsyncWaitHandle.WaitOne(ConnectTimeoutMs))
            {
                try { socket.Dispose(); } catch { }
                throw new TimeoutException($"unix socket connect timeout at {socketPath}");
            }
            socket.EndConnect(ar);

            return new NetworkStream(socket, ownsSocket: true);
        }

        void DisposeStreamLocked()
        {
            try { _writer?.Dispose(); } catch { }
            try { _stream?.Dispose(); } catch { }
            _writer = null;
            _stream = null;
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed) return;
                _disposed = true;
                DisposeStreamLocked();
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
