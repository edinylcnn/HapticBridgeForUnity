namespace Loupedeck.LogiHapticsUnityPlugin
{
    using System;
    using System.IO;
    using System.IO.Pipes;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class PipeServer : IDisposable
    {
        public const string PipeName = "LogiHapticsUnity";

        readonly Action<string> _onEvent;
        readonly CancellationTokenSource _cts = new CancellationTokenSource();
        Task _loop;

        public PipeServer(Action<string> onEvent)
        {
            _onEvent = onEvent ?? throw new ArgumentNullException(nameof(onEvent));
        }

        public void Start()
        {
            if (_loop != null) return;
            CleanupStaleSocket();
            _loop = Task.Run(() => AcceptLoop(_cts.Token));
        }

        async Task AcceptLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                NamedPipeServerStream server = null;
                try
                {
                    server = new NamedPipeServerStream(
                        PipeName,
                        PipeDirection.In,
                        maxNumberOfServerInstances: 1,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous);

                    PluginLog.Info("pipe: waiting for connection");
                    await server.WaitForConnectionAsync(ct).ConfigureAwait(false);
                    PluginLog.Info("pipe: client connected");

                    using (var reader = new StreamReader(server))
                    {
                        string line;
                        while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                        {
                            if (ct.IsCancellationRequested) break;
                            PluginLog.Info($"pipe: raw line '{line}'");
                            var trimmed = line.Trim();
                            if (trimmed.Length == 0) continue;
                            try { _onEvent(trimmed); }
                            catch (Exception ex) { PluginLog.Error(ex, "pipe handler error"); }
                        }
                    }
                    PluginLog.Info("pipe: client disconnected");
                }
                catch (OperationCanceledException) { break; }
                catch (IOException ex) when (ex.Message.Contains("AllPipeInstances", StringComparison.OrdinalIgnoreCase))
                {
                    PluginLog.Warning("pipe: stale socket detected, cleaning up");
                    CleanupStaleSocket();
                    try { await Task.Delay(250, ct).ConfigureAwait(false); } catch { break; }
                }
                catch (Exception ex)
                {
                    PluginLog.Warning($"pipe error: {ex.Message}");
                    try { await Task.Delay(250, ct).ConfigureAwait(false); } catch { break; }
                }
                finally
                {
                    try { server?.Dispose(); } catch { }
                }
            }

            CleanupStaleSocket();
        }

        static void CleanupStaleSocket()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

            var tmp = Environment.GetEnvironmentVariable("TMPDIR");
            if (string.IsNullOrEmpty(tmp)) tmp = "/tmp/";
            if (!tmp.EndsWith("/")) tmp += "/";
            var path = tmp + "CoreFxPipe_" + PipeName;

            if (!File.Exists(path)) return;

            // If a live server is listening, connecting succeeds — don't remove it.
            try
            {
                using var probe = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                probe.Connect(new UnixDomainSocketEndPoint(path));
                PluginLog.Info($"pipe: socket at {path} is live, not removing");
                return;
            }
            catch
            {
                // No listener — stale. Remove.
            }

            try { File.Delete(path); PluginLog.Info($"pipe: removed stale socket {path}"); }
            catch (Exception ex) { PluginLog.Warning($"pipe: could not remove stale socket {path}: {ex.Message}"); }
        }

        public void Dispose()
        {
            try { _cts.Cancel(); } catch { }
            try { _loop?.Wait(500); } catch { }
            _cts.Dispose();
            CleanupStaleSocket();
        }
    }
}
