using System;
using System.Text;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;

namespace KSABridge;

public class Publisher : IAsyncDisposable
{
    private IMqttClient       _client;
    private BridgeConfig      _config;
    private readonly BridgeState    _state;
    private CancellationTokenSource _cts = new();
    private Task?                   _heartbeatTask;

    private const string Version      = "0.1.0";
    private const string StatusTopic  = "ksa/bridge/status";
    private const string CmdTopicRoot = "ksa/bridge/cmd/#";

    public Publisher(BridgeConfig config, BridgeState state)
    {
        _config = config;
        _state  = state;
        _client = new MqttFactory().CreateMqttClient();
        _client.ApplicationMessageReceivedAsync += OnMessageReceived;
    }

    // ── Start ─────────────────────────────────────────────────────
    public async Task StartAsync()
    {
        _state.Status          = ConnectionStatus.Connecting;
        _state.BrokerEndpoint  = $"{_config.BrokerHost}:{_config.BrokerPort}";

        await ConnectAsync();

        _cts          = new CancellationTokenSource();
        _heartbeatTask = RunAsync(_cts.Token);
    }

    // ── Connect (called on start and reconnect) ───────────────────
    private async Task ConnectAsync()
    {
        var offline = JsonSerializer.Serialize(new { status = "offline" });

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(_config.BrokerHost, (int?)_config.BrokerPort, AddressFamily.Unspecified)
            .WithClientId(_config.ClientId)
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(_config.KeepAlive))
            .WithWillTopic(StatusTopic)
            .WithWillPayload(offline)
            .WithWillRetain(true)
            .WithCleanSession(true)
            .Build();

        try
        {
            _state.Status = ConnectionStatus.Connecting;
            await _client.ConnectAsync(options);

            // Subscribe to command topics
            await _client.SubscribeAsync(CmdTopicRoot);

            // On (re)connect: clear change-detection cache (Lesson 3.4)
            // and publish immediate status
            _state.ConnectedAt = DateTime.UtcNow;
            _state.Status      = ConnectionStatus.Connected;
            _state.LastError   = string.Empty;

            await PublishStatusAsync("online");
            Console.WriteLine($"[KSA-Bridge] Connected to {_config.BrokerHost}:{_config.BrokerPort}");
        }
        catch (Exception ex)
        {
            _state.Status    = ConnectionStatus.Disconnected;
            _state.LastError = ex.Message;
            Console.WriteLine($"[KSA-Bridge] Connect failed: {ex.Message}");
        }
    }

    // ── Main loop ─────────────────────────────────────────────────
    private async Task RunAsync(CancellationToken ct)
    {
        var nextHeartbeat = Environment.TickCount64 + _config.HeartbeatIntervalMs;

        while (!ct.IsCancellationRequested)
        {
            // ── Reload signal ──────────────────────────────────────
            if (_state.ReloadRequested)
            {
                _state.ReloadRequested = false;
                await HandleReloadAsync();
            }

            // ── Reconnect signal ───────────────────────────────────
            if (_state.ReconnectRequested)
            {
                _state.ReconnectRequested = false;
                await HandleReconnectAsync();
            }

            // ── Heartbeat ──────────────────────────────────────────
            var now = Environment.TickCount64;
            if (now >= nextHeartbeat && _client.IsConnected && !_state.FeedPaused)
            {
                await PublishStatusAsync("online");
                _state.LastHeartbeatTick = now;
                nextHeartbeat = now + _config.HeartbeatIntervalMs;
            }

            // ── Auto-reconnect if dropped ──────────────────────────
            if (!_client.IsConnected && _state.Status != ConnectionStatus.Connecting)
            {
                _state.Status = ConnectionStatus.Reconnecting;
                Console.WriteLine("[KSA-Bridge] Broker connection lost — reconnecting...");
                await Task.Delay(3000, ct);
                await ConnectAsync();
            }

            await Task.Delay(100, ct);
        }
    }

    // ── Inbound command handler ───────────────────────────────────
    private Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs e)
    {
        var topic   = e.ApplicationMessage.Topic;
        var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

        Console.WriteLine($"[KSA-Bridge] CMD received: {topic} {payload}");

        // ksa/bridge/cmd/reload  → re-read ksa-bridge.toml
        if (topic.EndsWith("/reload",    StringComparison.OrdinalIgnoreCase))
            _state.ReloadRequested = true;

        // ksa/bridge/cmd/reconnect → drop and reconnect to broker
        else if (topic.EndsWith("/reconnect", StringComparison.OrdinalIgnoreCase))
            _state.ReconnectRequested = true;

        // ksa/bridge/cmd/ping → immediate status publish
        else if (topic.EndsWith("/ping", StringComparison.OrdinalIgnoreCase))
            _ = PublishStatusAsync("online");

        // ksa/bridge/cmd/pause → toggle feed pause
        else if (topic.EndsWith("/pause", StringComparison.OrdinalIgnoreCase))
            _state.FeedPaused = !_state.FeedPaused;

        return Task.CompletedTask;
    }

    // ── Reload ────────────────────────────────────────────────────
    private string _configPath = string.Empty;

    public void SetConfigPath(string path) => _configPath = path;

    private async Task HandleReloadAsync()
    {
        Console.WriteLine("[KSA-Bridge] Reloading config...");
        var newConfig = BridgeConfig.Load(_configPath);

        // If broker settings changed, reconnect
        bool brokerChanged = newConfig.BrokerHost != _config.BrokerHost
                          || newConfig.BrokerPort != _config.BrokerPort
                          || newConfig.ClientId   != _config.ClientId;

        _config = newConfig;

        if (brokerChanged)
        {
            Console.WriteLine("[KSA-Bridge] Broker settings changed — reconnecting.");
            await HandleReconnectAsync();
        }
        else
        {
            Console.WriteLine("[KSA-Bridge] Config reloaded (no broker change).");
        }
    }

    private async Task HandleReconnectAsync()
    {
        _state.Status = ConnectionStatus.Reconnecting;
        try { await _client.DisconnectAsync(); } catch { /* ignore */ }
        await Task.Delay(500);
        await ConnectAsync();
    }

    // ── Publish helpers ───────────────────────────────────────────
    private async Task PublishStatusAsync(string status)
    {
        if (!_client.IsConnected) return;

        var payload = JsonSerializer.Serialize(new
        {
            status,
            version = Version,
            broker  = _state.BrokerEndpoint,
            paused  = _state.FeedPaused
        });

        await PublishAsync(StatusTopic, payload, retain: true);
    }

    public async Task PublishAsync(string topic, string payload, bool retain = false)
    {
        // DEBUG: Log every publish attempt
        Console.WriteLine($"[KSA-Bridge] PublishAsync called - topic: {topic}, connected: {_client.IsConnected}");
        
        if (!_client.IsConnected)
        {
            Console.WriteLine($"[KSA-Bridge] PublishAsync FAILED - client not connected!");
            return;
        }

        if (_config.LowercaseTopics) topic = topic.ToLowerInvariant();

        var msg = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(Encoding.UTF8.GetBytes(payload))
            .WithRetainFlag(retain)
            .Build();

        try
        {
            Console.WriteLine($"[KSA-Bridge] Publishing to {topic}...");
            await _client.PublishAsync(msg);
            _state.IncrementMessagesSent();
            Console.WriteLine($"[KSA-Bridge] Published successfully to {topic}");
        }
        catch (Exception ex)
        {
            _state.LastError = ex.Message;
            Console.WriteLine($"[KSA-Bridge] Publish ERROR: {ex.Message}");
        }
    }

    // ── Stop / Dispose ────────────────────────────────────────────
    public async Task StopAsync()
    {
        _cts.Cancel();
        if (_heartbeatTask is not null)
        {
            try { await _heartbeatTask; } catch (OperationCanceledException) { }
        }

        if (_client.IsConnected)
        {
            await PublishStatusAsync("offline");
            await _client.DisconnectAsync();
        }

        _state.Status = ConnectionStatus.Disconnected;
        Console.WriteLine("[KSA-Bridge] Publisher stopped.");
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        _client.Dispose();
    }
}
