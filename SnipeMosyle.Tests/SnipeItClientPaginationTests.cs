// Copyright (c) Jordan Molenaar (jordanm.cloud). All rights reserved.

using System.Net;
using System.Net.Sockets;
using System.Text;
using jordanm.cloud.mosyle.Clients;
using jordanm.cloud.mosyle.Clients.SnipeIt;
using Xunit;

namespace SnipeMosyle.Tests;

public sealed class SnipeItClientPaginationTests
{
    [Fact]
    public async Task GetAllModelsAsync_FetchesOffsetsUntilTotalIsCollected()
    {
        await using var server = new TestHttpServer((path, _) =>
        {
            var offset = QueryInt(path, "offset");
            return offset switch
            {
                0 => TestResponse.Json(ModelPage(3, 0, 2, (1, "iPad 9"), (2, "iPad 10"))),
                2 => TestResponse.Json(ModelPage(3, 2, 2, (3, "iPad Air"))),
                _ => TestResponse.Json(ModelPage(3, offset, 2))
            };
        });

        var client = new SnipeItClient(server.BaseUrl, "token", pageDelayMilliseconds: 0);

        var response = await client.GetAllModelsAsync(limit: 2);

        Assert.NotNull(response);
        Assert.Equal(["iPad 9", "iPad 10", "iPad Air"], response.Rows.Select(model => model.Name));
        Assert.Contains(server.Paths, path => path.Contains("/api/v1/models?limit=2&offset=0", StringComparison.Ordinal));
        Assert.Contains(server.Paths, path => path.Contains("/api/v1/models?limit=2&offset=2", StringComparison.Ordinal));
        Assert.Equal(2, server.Paths.Count);
    }

    [Fact]
    public async Task GetAllAssetsAsync_FetchesOffsetsUntilTotalIsCollected()
    {
        await using var server = new TestHttpServer((path, _) =>
        {
            var offset = QueryInt(path, "offset");
            return offset switch
            {
                0 => TestResponse.Json(AssetPage(3, 0, 2, (1, "HB-1"), (2, "HB-2"))),
                2 => TestResponse.Json(AssetPage(3, 2, 2, (3, "HB-3"))),
                _ => TestResponse.Json(AssetPage(3, offset, 2))
            };
        });

        var client = new SnipeItClient(server.BaseUrl, "token", pageDelayMilliseconds: 0);

        var response = await client.GetAllAssetsAsync(limit: 2);

        Assert.NotNull(response);
        Assert.Equal(["HB-1", "HB-2", "HB-3"], response.Rows.Select(asset => asset.AssetTag));
        Assert.Contains(server.Paths, path => path == "/api/v1/hardware?limit=2&offset=0");
        Assert.Contains(server.Paths, path => path == "/api/v1/hardware?limit=2&offset=2");
        Assert.Equal(2, server.Paths.Count);
    }

    [Fact]
    public async Task SendRequestAsync_RetriesRateLimitResponseWithRetryAfterHeader()
    {
        await using var server = new TestHttpServer((_, requestNumber) =>
        {
            return requestNumber == 1
                ? TestResponse.Json("{\"status\":\"error\",\"rows\":[]}", HttpStatusCode.TooManyRequests, ("Retry-After", "0"))
                : TestResponse.Json(AssetPage(1, 0, 1, (1, "HB-1")));
        });

        var client = new SnipeItClient(server.BaseUrl, "token", pageDelayMilliseconds: 0);

        var response = await client.GetAllAssetsAsync(limit: 1);

        Assert.NotNull(response);
        Assert.Single(response.Rows);
        Assert.Equal(2, server.Paths.Count);
    }

    [Fact]
    public void ApiRetryPolicy_UsesExponentialDelayWhenRetryAfterIsMissing()
    {
        var delay = ApiRetryPolicy.GetRetryDelay(headers: null, pageDelayMilliseconds: 250, retryAttempt: 3);

        Assert.Equal(TimeSpan.FromSeconds(1), delay);
    }

    private static string ModelPage(int total, int offset, int limit, params (int Id, string Name)[] models)
    {
        var rows = string.Join(",", models.Select(model => $"{{\"id\":{model.Id},\"name\":\"{model.Name}\"}}"));
        return $"{{\"status\":\"success\",\"total\":{total},\"offset\":{offset},\"limit\":{limit},\"rows\":[{rows}]}}";
    }

    private static string AssetPage(int total, int offset, int limit, params (int Id, string AssetTag)[] assets)
    {
        var rows = string.Join(
            ",",
            assets.Select(asset =>
                $"{{\"id\":{asset.Id},\"name\":\"Asset {asset.Id}\",\"asset_tag\":\"{asset.AssetTag}\",\"serial\":\"SERIAL-{asset.Id}\"}}"));

        return $"{{\"status\":\"success\",\"total\":{total},\"offset\":{offset},\"limit\":{limit},\"rows\":[{rows}]}}";
    }

    private static int QueryInt(string path, string name)
    {
        var queryIndex = path.IndexOf('?', StringComparison.Ordinal);
        if (queryIndex < 0)
            return 0;

        var pairs = path[(queryIndex + 1)..].Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2);
            if (parts.Length == 2 && parts[0] == name && int.TryParse(parts[1], out var value))
                return value;
        }

        return 0;
    }

    private sealed class TestHttpServer : IAsyncDisposable
    {
        private readonly TcpListener _listener;
        private readonly Func<string, int, TestResponse> _handler;
        private readonly CancellationTokenSource _cancellation = new();
        private readonly Task _acceptLoop;
        private int _requestCount;

        public TestHttpServer(Func<string, int, TestResponse> handler)
        {
            _handler = handler;
            _listener = new TcpListener(IPAddress.Loopback, 0);
            _listener.Start();

            var endpoint = (IPEndPoint)_listener.LocalEndpoint;
            BaseUrl = $"http://127.0.0.1:{endpoint.Port}";
            _acceptLoop = Task.Run(AcceptLoopAsync);
        }

        public string BaseUrl { get; }
        public List<string> Paths { get; } = [];

        public async ValueTask DisposeAsync()
        {
            await _cancellation.CancelAsync();
            _listener.Stop();

            try
            {
                await _acceptLoop.WaitAsync(TimeSpan.FromSeconds(1));
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SocketException)
            {
            }

            _cancellation.Dispose();
        }

        private async Task AcceptLoopAsync()
        {
            while (!_cancellation.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync(_cancellation.Token);
                _ = Task.Run(() => HandleClientAsync(client));
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            using (client)
            {
                var stream = client.GetStream();
                var request = await ReadHeadersAsync(stream);
                var path = ParsePath(request);
                var requestNumber = Interlocked.Increment(ref _requestCount);

                lock (Paths)
                {
                    Paths.Add(path);
                }

                var response = _handler(path, requestNumber);
                await WriteResponseAsync(stream, response);
            }
        }

        private static async Task<string> ReadHeadersAsync(NetworkStream stream)
        {
            var buffer = new byte[1024];
            var builder = new StringBuilder();

            while (true)
            {
                var bytesRead = await stream.ReadAsync(buffer);
                if (bytesRead == 0)
                    break;

                builder.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                if (builder.ToString().Contains("\r\n\r\n", StringComparison.Ordinal))
                    break;
            }

            return builder.ToString();
        }

        private static string ParsePath(string request)
        {
            var firstLine = request.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
            var parts = firstLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2 ? parts[1] : "/";
        }

        private static async Task WriteResponseAsync(NetworkStream stream, TestResponse response)
        {
            var bodyBytes = Encoding.UTF8.GetBytes(response.Body);
            var headers = new StringBuilder()
                .Append("HTTP/1.1 ")
                .Append((int)response.StatusCode)
                .Append(' ')
                .Append(response.StatusCode)
                .Append("\r\nContent-Type: application/json\r\nContent-Length: ")
                .Append(bodyBytes.Length)
                .Append("\r\nConnection: close\r\n");

            foreach (var header in response.Headers)
                headers.Append(header.Name).Append(": ").Append(header.Value).Append("\r\n");

            headers.Append("\r\n");

            var headerBytes = Encoding.ASCII.GetBytes(headers.ToString());
            await stream.WriteAsync(headerBytes);
            await stream.WriteAsync(bodyBytes);
        }
    }

    private sealed record TestResponse(
        HttpStatusCode StatusCode,
        string Body,
        IReadOnlyCollection<(string Name, string Value)> Headers)
    {
        public static TestResponse Json(
            string body,
            HttpStatusCode statusCode = HttpStatusCode.OK,
            params (string Name, string Value)[] headers)
        {
            return new TestResponse(statusCode, body, headers);
        }
    }
}
