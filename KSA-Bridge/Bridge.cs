using System;
using System.IO;
using Brutal.ImGuiApi;
using StarMap.API;

#if MODMENU_AVAILABLE
using ModMenu;
#endif

// ═══════════════════════════════════════════════════════════════════
//  Bridge.cs — StarMap mod entry point
//
//  Uses StarMap 0.4.x attribute-based API. Available attributes:
//    [StarMapMod]               ← marks the mod class
//    [StarMapAllModsLoaded]     ← initialization hook (ONLY hook in 0.4.x)
//    [StarMapAfterGui]          ← render hook
//    [StarMapUnload]            ← cleanup hook
//
//  NOTE: StarMap 0.4.x removed [StarMapImmediateLoad] - all init
//        must happen in OnAllModsLoaded().
// ═══════════════════════════════════════════════════════════════════

namespace KSABridge;

[StarMapMod]
public class Bridge
{
    private Publisher?           _publisher;
    private TelemetryPublisher?  _telemetry;
    private BridgeUi?            _ui;
    private BridgeState          _state  = new();
    private BridgeConfig         _config = new();
    private string               _configPath = string.Empty;

    // Static constructor - runs when class loads
    static Bridge()
    {
        Console.WriteLine("[KSA-Bridge] STATIC CONSTRUCTOR - Bridge class loaded!");
    }

    // ── StarMap 0.4.x: ONLY initialization hook ──────────────────
    // All initialization must happen here. StarMap 0.4.x removed
    // the ImmediateLoad hook that existed in 0.3.x
    [StarMapAllModsLoaded]
    public void OnAllModsLoaded()
    {
        Console.WriteLine("[KSA-Bridge] ====== OnAllModsLoaded START ======");
        
        try
        {
            Console.WriteLine("[KSA-Bridge] Resolving config path...");
            
            // Try multiple config locations (cross-platform)
            var possiblePaths = new List<string>();
            
            // Windows: Documents/My Games
            possiblePaths.Add(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "My Games", "Kitten Space Agency", "mods", "KSA-Bridge", "ksa-bridge.toml"));
            
            // Linux: XDG_DATA_HOME or ~/.local/share
            string xdgData = Environment.GetEnvironmentVariable("XDG_DATA_HOME") ?? 
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share");
            possiblePaths.Add(Path.Combine(xdgData, "Kitten Space Agency", "mods", "KSA-Bridge", "ksa-bridge.toml"));
            
            // Linux: XDG_CONFIG_HOME or ~/.config
            string xdgConfig = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") ?? 
                              Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
            possiblePaths.Add(Path.Combine(xdgConfig, "Kitten Space Agency", "mods", "KSA-Bridge", "ksa-bridge.toml"));
            
            // macOS: ~/Library/Application Support
            possiblePaths.Add(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Library", "Application Support", "Kitten Space Agency", "mods", "KSA-Bridge", "ksa-bridge.toml"));
            
            // Install directory (cross-platform fallback)
            possiblePaths.Add(Path.Combine(
                AppContext.BaseDirectory, "Content", "KSA-Bridge", "ksa-bridge.toml"));
            
            // Check each path in order
            _configPath = null;
            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    _configPath = path;
                    Console.WriteLine($"[KSA-Bridge] Found config at: {path}");
                    break;
                }
            }
            
            // If not found, default to first user path
            if (_configPath == null)
            {
                _configPath = possiblePaths[0];
                Console.WriteLine($"[KSA-Bridge] Config not found. Checked {possiblePaths.Count} locations:");
                foreach (var path in possiblePaths)
                {
                    Console.WriteLine($"[KSA-Bridge]   - {path}");
                }
                Console.WriteLine($"[KSA-Bridge] Will use defaults, would save to: {_configPath}");
            }
            
            Console.WriteLine("[KSA-Bridge] Loading config...");
            _config    = BridgeConfig.Load(_configPath);
            
            Console.WriteLine("[KSA-Bridge] Creating state...");
            _state     = new BridgeState();
            _state.ConfigPath = _configPath;  // Set config path for UI display
            
            Console.WriteLine("[KSA-Bridge] Creating publisher...");
            _publisher = new Publisher(_config, _state);
            _publisher.SetConfigPath(_configPath);
            
            Console.WriteLine("[KSA-Bridge] Creating telemetry...");
            _telemetry = new TelemetryPublisher(_publisher, _config, _state);
            
            Console.WriteLine("[KSA-Bridge] Creating UI...");
            _ui        = new BridgeUi(_state, _config, ReloadConfig);
            
            Console.WriteLine("[KSA-Bridge] ====== OnAllModsLoaded: Starting MQTT ======");
            
            // Start MQTT connection (fire-and-forget; Publisher handles reconnect)
            _ = _publisher.StartAsync();

            Console.WriteLine("[KSA-Bridge] Ready. Status bar: Ctrl+B or ksa/bridge/cmd/* for control.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[KSA-Bridge] !!!!! OnAllModsLoaded FAILED: {ex.Message}");
            Console.WriteLine($"[KSA-Bridge] Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    // ── GUI render hook ────────────────────────────────────────────
    // Called every frame after ImGui is set up. Keep this fast.
    private static int _guiCallCount = 0;  // Debug counter
    
    [StarMapAfterGui]
    public void OnAfterGui(double dt)  // dt = delta time in seconds
    {
        _guiCallCount++;
        
        // DEBUG: Log first 10 calls to verify this hook is being called
        if (_guiCallCount <= 10)
        {
            Console.WriteLine($"[KSA-Bridge] OnAfterGui() call #{_guiCallCount}, dt={dt:F3}s");
        }
        
        // Keyboard shortcut: Ctrl+B to toggle debug panel
        if (ImGui.GetIO().KeyCtrl && ImGui.IsKeyPressed(ImGuiKey.B))
        {
            if (_ui != null)
                _ui.ShowDebugPanel = !_ui.ShowDebugPanel;
        }
        
        _telemetry?.Update();  // Publish telemetry if enabled
        _ui?.Draw();           // Draw UI if initialized
    }

    // ── Config reload ──────────────────────────────────────────────
    private void ReloadConfig()
    {
        try
        {
            Console.WriteLine("[KSA-Bridge] Reloading config from: " + _configPath);
            
            if (_configPath != null && File.Exists(_configPath))
            {
                var newConfig = BridgeConfig.Load(_configPath);
                _config = newConfig;
                
                // Update UI with new config
                _ui?.UpdateConfig(newConfig);
                
                // Update telemetry with new rates
                if (_telemetry != null)
                {
                    _telemetry = new TelemetryPublisher(_publisher!, newConfig, _state);
                }
                
                Console.WriteLine("[KSA-Bridge] Config reloaded successfully");
            }
            else
            {
                Console.WriteLine("[KSA-Bridge] Config file not found: " + _configPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[KSA-Bridge] Config reload failed: {ex.Message}");
        }
    }

    // ── Cleanup hook ───────────────────────────────────────────────
    // Called when KSA is shutting down or mod is being unloaded
    [StarMapUnload]
    public void OnUnload()
    {
        Console.WriteLine("[KSA-Bridge] Unloading");
        
        if (_publisher != null)
        {
            _ = _publisher.DisposeAsync();
        }
    }

    // ── ModMenu Integration (optional) ─────────────────────────────
#if MODMENU_AVAILABLE
    [ModMenuEntry("KSA-Bridge")]
    public static void DrawModMenu()
    {
        if (_instance?._ui != null)
        {
            ImGuiHelper.DrawMenuItem("Status Bar", ref _instance._ui.ShowStatusBar);
            ImGuiHelper.DrawMenuItem("Debug Panel", ref _instance._ui.ShowDebugPanel);
        }
    }
#endif
}
