using System;
using System.IO;
using Tomlyn;
using Tomlyn.Model;

namespace KSABridge;

public class BridgeConfig
{
    // ── Broker ────────────────────────────────────────────────────
    public string BrokerHost    { get; set; } = "192.168.4.51";
    public int    BrokerPort    { get; set; } = 1883;
    public string ClientId      { get; set; } = "ksa-bridge";
    public int    KeepAlive     { get; set; } = 60;

    // ── Publish rates ─────────────────────────────────────────────
    public string TopicPrefix            { get; set; } = "ksa";
    public string PublishMode            { get; set; } = "processed"; // "raw" | "processed"
    public int    TelemetryHz            { get; set; } = 10;
    public int    OrbitHz                { get; set; } = 2;
    public int    ResourcesHz            { get; set; } = 2;
    public int    UniverseHz             { get; set; } = 1;
    public int    HeartbeatS             { get; set; } = 5;
    public bool   UniverseBodiesOnChange { get; set; } = true;
    public bool   LowercaseTopics        { get; set; } = false;

    // ── UI ────────────────────────────────────────────────────────
    public bool   ShowStatusBar  { get; set; } = true;
    public bool   ShowDebugPanel { get; set; } = false;

    // ── Derived: millisecond intervals ───────────────────────────
    public int TelemetryIntervalMs  => 1000 / Math.Max(1, TelemetryHz);
    public int OrbitIntervalMs      => 1000 / Math.Max(1, OrbitHz);
    public int ResourcesIntervalMs  => 1000 / Math.Max(1, ResourcesHz);
    public int UniverseIntervalMs   => 1000 / Math.Max(1, UniverseHz);
    public int HeartbeatIntervalMs  => HeartbeatS * 1000;

    // ── Load from TOML ────────────────────────────────────────────
    public static BridgeConfig Load(string path)
    {
        var cfg = new BridgeConfig();

        if (!File.Exists(path))
        {
            Console.WriteLine($"[KSA-Bridge] Config not found at {path} — using defaults.");
            return cfg;
        }

        try
        {
            var text  = File.ReadAllText(path);
            var model = Toml.ToModel(text);

            // [broker]
            if (model.TryGetValue("broker", out var brokerRaw) && brokerRaw is TomlTable broker)
            {
                cfg.BrokerHost  = GetString(broker,  "host",      cfg.BrokerHost);
                cfg.BrokerPort  = GetInt(broker,     "port",      cfg.BrokerPort);
                cfg.ClientId    = GetString(broker,  "client_id", cfg.ClientId);
                cfg.KeepAlive   = GetInt(broker,     "keepalive", cfg.KeepAlive);
            }

            // [publish]
            if (model.TryGetValue("publish", out var pubRaw) && pubRaw is TomlTable pub)
            {
                cfg.TopicPrefix            = GetString(pub, "topic_prefix",              cfg.TopicPrefix);
                cfg.PublishMode            = GetString(pub, "publish_mode",              cfg.PublishMode);
                cfg.TelemetryHz            = GetInt(pub,    "telemetry_hz",              cfg.TelemetryHz);
                cfg.OrbitHz                = GetInt(pub,    "orbit_hz",                  cfg.OrbitHz);
                cfg.ResourcesHz            = GetInt(pub,    "resources_hz",              cfg.ResourcesHz);
                cfg.UniverseHz             = GetInt(pub,    "universe_hz",               cfg.UniverseHz);
                cfg.HeartbeatS             = GetInt(pub,    "heartbeat_s",               cfg.HeartbeatS);
                cfg.UniverseBodiesOnChange = GetBool(pub,   "universe_bodies_on_change", cfg.UniverseBodiesOnChange);
                cfg.LowercaseTopics        = GetBool(pub,   "lowercase_topics",          cfg.LowercaseTopics);
            }

            // [ui]
            if (model.TryGetValue("ui", out var uiRaw) && uiRaw is TomlTable ui)
            {
                cfg.ShowStatusBar  = GetBool(ui, "show_status_bar",  cfg.ShowStatusBar);
                cfg.ShowDebugPanel = GetBool(ui, "show_debug_panel", cfg.ShowDebugPanel);
            }

            Console.WriteLine($"[KSA-Bridge] Config loaded from {path}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[KSA-Bridge] Config parse error — using defaults. ({ex.Message})");
        }

        return cfg;
    }

    // ── Helpers ───────────────────────────────────────────────────
    private static string GetString(TomlTable t, string key, string fallback)
        => t.TryGetValue(key, out var v) && v is string s ? s : fallback;

    private static int GetInt(TomlTable t, string key, int fallback)
        => t.TryGetValue(key, out var v) ? Convert.ToInt32(v) : fallback;

    private static bool GetBool(TomlTable t, string key, bool fallback)
        => t.TryGetValue(key, out var v) && v is bool b ? b : fallback;
}
