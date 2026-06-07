using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Alibi.Plugins.API;
using Newtonsoft.Json;

namespace Alibi
{
    internal class Advertiser : IAdvertiser
    {
        private static readonly HttpClient _client = new();
        private readonly CancellationTokenSource _cts = new();
        
        public void Start(string url)
        {
            _ = PeriodicAsync(() => SendHeartbeat(url), TimeSpan.FromMinutes(3), _cts.Token);
        }

        private async Task SendHeartbeat(string url)
        {
            Server.Logger.Log(LogSeverity.Info, $"[Advertiser] Attempting to send heartbeat...", true);
            var server = Server.Instance;
            var json = new Heartbeat(
                server.ServerConfiguration.Port,
                server.ServerConfiguration.WebsocketPort,
                server.ConnectedPlayers,
                server.ServerConfiguration.ServerName,
                server.ServerConfiguration.ServerDescription);
            var response = await _client.PostAsync(url, new StringContent(JsonConvert.SerializeObject(json)));
            response.EnsureSuccessStatusCode();
        } 

        private async Task PeriodicAsync(Func<Task> action, TimeSpan interval,
            CancellationToken cancellationToken = default)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await action();
                    await Task.Delay(interval, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                Server.Logger.Log(LogSeverity.Info, $"[Advertiser] Stopping advertiser...", true);
            }
            catch (Exception exception)
            {
                Server.Logger.Log(LogSeverity.Error, $"[Advertiser] Exception: {exception}");
            }
            finally
            {
                _client.Dispose();
            }
        }

        public void Stop()
        {
            _cts.Cancel();
        }

        private record Heartbeat(int port, int ws_port, int players, string name, string description);
    }
}