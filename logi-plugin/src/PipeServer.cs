using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace LogiHapticsUnity.Plugin
{
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
            _loop = Task.Run(() => AcceptLoop(_cts.Token));
        }

        async Task AcceptLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    using var server = new NamedPipeServerStream(
                        PipeName,
                        PipeDirection.In,
                        maxNumberOfServerInstances: 1,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous);

                    await server.WaitForConnectionAsync(ct).ConfigureAwait(false);
                    using var reader = new StreamReader(server);

                    string line;
                    while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        if (ct.IsCancellationRequested) break;
                        var trimmed = line.Trim();
                        if (trimmed.Length == 0) continue;
                        try { _onEvent(trimmed); }
                        catch (Exception ex) { Console.Error.WriteLine($"[LogiHapticsUnity] handler error: {ex}"); }
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[LogiHapticsUnity] pipe error: {ex.Message}");
                    await Task.Delay(250, ct).ConfigureAwait(false);
                }
            }
        }

        public void Dispose()
        {
            try { _cts.Cancel(); } catch { }
            try { _loop?.Wait(500); } catch { }
            _cts.Dispose();
        }
    }
}
