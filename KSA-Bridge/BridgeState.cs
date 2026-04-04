using System;

namespace KSABridge;

/// <summary>
/// Shared state bag written by Publisher, read by BridgeUi.
/// All fields are volatile or Interlocked-safe — no locking required.
/// Publisher runs on a background thread; UI reads on the game render thread.
/// </summary>
public class BridgeState
{
    // ── Connection ────────────────────────────────────────────────
    public volatile ConnectionStatus Status = ConnectionStatus.Disconnected;
    public volatile string BrokerEndpoint = string.Empty;
    public volatile string LastError = string.Empty;
    public DateTime ConnectedAt = DateTime.MinValue;

    // ── Message counters (use Interlocked for thread safety) ───────
    private long _messagesSent;
    public long MessagesSent => System.Threading.Interlocked.Read(ref _messagesSent);
    public void IncrementMessagesSent() => System.Threading.Interlocked.Increment(ref _messagesSent);

    // ── Per-topic-group last publish time ─────────────────────────
    // Written by Publisher; read by UI for the "last seen" display.
    // .NET 10 removed volatile long — use Interlocked for thread safety
    private long _lastHeartbeatTick;
    private long _lastTelemetryTick;
    private long _lastOrbitTick;
    private long _lastResourcesTick;
    private long _lastCrewTick;
    private long _lastEventTick;

    public long LastHeartbeatTick
    {
        get => System.Threading.Interlocked.Read(ref _lastHeartbeatTick);
        set => System.Threading.Interlocked.Exchange(ref _lastHeartbeatTick, value);
    }
    public long LastTelemetryTick
    {
        get => System.Threading.Interlocked.Read(ref _lastTelemetryTick);
        set => System.Threading.Interlocked.Exchange(ref _lastTelemetryTick, value);
    }
    public long LastOrbitTick
    {
        get => System.Threading.Interlocked.Read(ref _lastOrbitTick);
        set => System.Threading.Interlocked.Exchange(ref _lastOrbitTick, value);
    }
    public long LastResourcesTick
    {
        get => System.Threading.Interlocked.Read(ref _lastResourcesTick);
        set => System.Threading.Interlocked.Exchange(ref _lastResourcesTick, value);
    }
    public long LastCrewTick
    {
        get => System.Threading.Interlocked.Read(ref _lastCrewTick);
        set => System.Threading.Interlocked.Exchange(ref _lastCrewTick, value);
    }
    public long LastEventTick
    {
        get => System.Threading.Interlocked.Read(ref _lastEventTick);
        set => System.Threading.Interlocked.Exchange(ref _lastEventTick, value);
    }

    // ── Feed pause flag (set by UI Pause button or MQTT cmd) ───────
    public volatile bool FeedPaused = false;

    // ── Reload signal (set by UI button or MQTT cmd) ───────────────
    // Publisher checks this on each tick and performs reload if true.
    public volatile bool ReloadRequested = false;

    // ── Reconnect signal ──────────────────────────────────────────
    public volatile bool ReconnectRequested = false;

    // ── UI Display Fields ──────────────────────────────────────────
    public volatile string? ConfigPath = null;
    public DateTime LastPublishTime = DateTime.MinValue;
    
    // Last telemetry data (for UI display) - volatile for thread safety
    public volatile VehicleTelemetry? LastVehicleData = null;
    public volatile OrbitTelemetry? LastOrbitData = null;
    
    // ── Helpers ───────────────────────────────────────────────────
    public bool Connected => Status == ConnectionStatus.Connected;
    
    public TimeSpan Uptime => Status == ConnectionStatus.Connected
        ? DateTime.UtcNow - ConnectedAt
        : TimeSpan.Zero;

    public string AgoString(long tick)
    {
        if (tick == 0) return "never";
        var ms = Environment.TickCount64 - tick;
        if (ms < 2000) return $"{ms}ms ago";
        return $"{ms / 1000}s ago";
    }
}

public enum ConnectionStatus
{
    Disconnected,
    Connecting,
    Connected,
    Reconnecting
}

// ── Telemetry data structures for UI display ────────────────────
public class VehicleTelemetry
{
    public string Position { get; set; } = "";
    public string Velocity { get; set; } = "";
    public double Altitude { get; set; }
}

public class OrbitTelemetry
{
    public double Apoapsis { get; set; }
    public double Periapsis { get; set; }
    public double Period { get; set; }
}
