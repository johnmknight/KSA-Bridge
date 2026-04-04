using System;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using KSA;

// BridgeUi.cs - In-game ImGui status bar and debug panel
//
// Based on ksa_oemloader mod by arthomnix.dev
// Uses Brutal.ImGuiApi (not Dear ImGui directly)

namespace KSABridge;

public class BridgeUi
{
    private readonly BridgeState  _state;
    private          BridgeConfig _config;
    
    // Callback for config reload
    private readonly Action? _onReloadConfig;
    
    // Window visibility flags
    public bool ShowStatusBar  = true;
    public bool ShowDebugPanel = false;
    
    // Color constants
    private readonly float4 _colorGreen  = new(0.0f, 1.0f, 0.0f, 1.0f);
    private readonly float4 _colorRed    = new(0.863f, 0.196f, 0.184f, 1f);
    private readonly float4 _colorYellow = new(1.0f, 1.0f, 0.0f, 1.0f);
    private readonly float4 _colorGray   = new(0.5f, 0.5f, 0.5f, 1.0f);

    public BridgeUi(BridgeState state, BridgeConfig config, Action? onReloadConfig = null)
    {
        _state          = state;
        _config         = config;
        _onReloadConfig = onReloadConfig;
        ShowStatusBar   = config.ShowStatusBar;
        ShowDebugPanel  = config.ShowDebugPanel;
    }

    public void UpdateConfig(BridgeConfig config)
    {
        _config = config;
    }

    // Main draw call - invoke from game render tick
    public void Draw()
    {
        if (!Program.DrawUI) return;  // Respect game's UI toggle
        
        if (ShowStatusBar)
            DrawStatusBar();
        
        if (ShowDebugPanel)
            DrawDebugPanelWindow();
    }

    // Status Bar (top-right corner)
    private void DrawStatusBar()
    {
        var displaySize = ImGui.GetIO().DisplaySize;
        ImGui.SetNextWindowPos(new float2(displaySize.X - 220, 10));
        ImGui.SetNextWindowSize(new float2(210, 100), ImGuiCond.Always);
        
        var flags = ImGuiWindowFlags.NoTitleBar 
                  | ImGuiWindowFlags.NoResize 
                  | ImGuiWindowFlags.NoMove
                  | ImGuiWindowFlags.NoCollapse;
        
        if (ImGui.Begin("##KSABridgeStatus", flags))
        {
            ImGui.Text("KSA-Bridge");
            ImGui.Separator();
            
            // Connection status with color indicator
            var color  = _state.Connected ? _colorGreen : _colorRed;
            var symbol = _state.Connected ? "●" : "○";
            var status = _state.Connected ? "Connected" : "Disconnected";
            
            ImGui.TextColored(color, $"{symbol} {status}");
            ImGui.Text($"Port: {_config.BrokerPort}");
            ImGui.Text($"Telemetry: {_config.TelemetryHz}Hz");
            
            // Click hint
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Press Ctrl+B for debug panel");
        }
        ImGui.End();
    }

    // Debug Panel (draggable window with tabs)
    private void DrawDebugPanelWindow()
    {
        ImGui.SetNextWindowSize(new float2(500, 600), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(new float2(400, 400), new float2(800, 800));
        
        if (ImGui.Begin("KSA-Bridge Debug", ref ShowDebugPanel))
        {
            if (ImGui.BeginTabBar("##DebugTabs"))
            {
                if (ImGui.BeginTabItem("Connection"))
                {
                    DrawConnectionTab();
                    ImGui.EndTabItem();
                }
                
                if (ImGui.BeginTabItem("Telemetry"))
                {
                    DrawTelemetryTab();
                    ImGui.EndTabItem();
                }
                
                if (ImGui.BeginTabItem("Config"))
                {
                    DrawConfigTab();
                    ImGui.EndTabItem();
                }
                
                ImGui.EndTabBar();
            }
        }
        ImGui.End();
    }

    // Connection Tab
    private void DrawConnectionTab()
    {
        ImGui.Text("MQTT Broker");
        ImGui.Separator();
        
        ImGui.Text($"Host: {_config.BrokerHost}");
        ImGui.Text($"Port: {_config.BrokerPort}");
        
        var color = _state.Connected ? _colorGreen : _colorRed;
        var status = _state.Connected ? "Connected" : "Disconnected";
        ImGui.TextColored(color, $"Status: {status}");
        
        if (_state.Connected && _state.LastPublishTime != DateTime.MinValue)
        {
            var elapsed = (DateTime.UtcNow - _state.LastPublishTime).TotalSeconds;
            ImGui.Text($"Last publish: {elapsed:F1}s ago");
        }
        
        ImGui.Spacing();
        ImGui.Text("Publishing Rates");
        ImGui.Separator();
        ImGui.Text($"Telemetry: {_config.TelemetryHz} Hz");
        ImGui.Text($"Orbit:     {_config.OrbitHz} Hz");
        ImGui.Text($"Resources: {_config.ResourcesHz} Hz");
        
        ImGui.Spacing();
        ImGui.Text("Topics");
        ImGui.Separator();
        ImGui.Text($"Prefix: {_config.TopicPrefix}");
        ImGui.Text($"• {_config.TopicPrefix}/telemetry/vehicle");
        ImGui.Text($"• {_config.TopicPrefix}/telemetry/orbit");
        ImGui.Text($"• {_config.TopicPrefix}/telemetry/resources");
    }

    // Telemetry Tab
    private void DrawTelemetryTab()
    {
        ImGui.Text("Last Published Data");
        ImGui.Separator();
        
        // Vehicle Telemetry
        if (_state.LastVehicleData != null)
        {
            ImGui.Text("Vehicle");
            ImGui.Indent();
            ImGui.TextColored(_colorGreen, $"Position: {_state.LastVehicleData.Position}");
            ImGui.TextColored(_colorGreen, $"Velocity: {_state.LastVehicleData.Velocity} m/s");
            ImGui.TextColored(_colorGreen, $"Altitude: {_state.LastVehicleData.Altitude:F1} m");
            ImGui.Unindent();
            ImGui.Spacing();
        }
        else
        {
            ImGui.Text("Vehicle");
            ImGui.Indent();
            ImGui.TextColored(_colorGray, "No data published yet");
            ImGui.Unindent();
            ImGui.Spacing();
        }
        
        // Orbit Telemetry
        if (_state.LastOrbitData != null)
        {
            ImGui.Text("Orbit");
            ImGui.Indent();
            ImGui.TextColored(_colorGreen, $"Apoapsis:  {_state.LastOrbitData.Apoapsis / 1000:F1} km");
            ImGui.TextColored(_colorGreen, $"Periapsis: {_state.LastOrbitData.Periapsis / 1000:F1} km");
            ImGui.TextColored(_colorGreen, $"Period:    {_state.LastOrbitData.Period / 60:F1} min");
            ImGui.Unindent();
            ImGui.Spacing();
        }
        else
        {
            ImGui.Text("Orbit");
            ImGui.Indent();
            ImGui.TextColored(_colorGray, "No data published yet");
            ImGui.Unindent();
            ImGui.Spacing();
        }
        
        // Publish Stats
        ImGui.Separator();
        ImGui.Text("Publishing Stats");
        ImGui.Indent();
        ImGui.Text($"Total messages: {_state.MessagesSent}");
        if (_state.Connected)
        {
            var uptime = _state.Uptime;
            ImGui.Text($"Uptime: {uptime.Hours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2}");
        }
        ImGui.Unindent();
    }

    // Config Tab
    private void DrawConfigTab()
    {
        ImGui.Text("Configuration");
        ImGui.Separator();
        
        ImGui.Text($"Config file: {_state.ConfigPath ?? "Not found"}");
        ImGui.Spacing();
        
        if (ImGui.Button("Reload Config"))
        {
            _onReloadConfig?.Invoke();
        }
        
        if (_onReloadConfig == null)
        {
            ImGui.SameLine();
            ImGui.TextColored(_colorYellow, "(No reload handler)");
        };
        
        ImGui.Spacing();
        ImGui.Text("Current Settings");
        ImGui.Separator();
        
        ImGui.Text("MQTT Broker");
        ImGui.Indent();
        ImGui.Text($"Host: {_config.BrokerHost}");
        ImGui.Text($"Port: {_config.BrokerPort}");
        ImGui.Unindent();
        
        ImGui.Spacing();
        ImGui.Text("Publishing");
        ImGui.Indent();
        ImGui.Text($"Telemetry: {_config.TelemetryHz} Hz");
        ImGui.Text($"Orbit:     {_config.OrbitHz} Hz");
        ImGui.Text($"Resources: {_config.ResourcesHz} Hz");
        ImGui.Text($"Topic Prefix: {_config.TopicPrefix}");
        ImGui.Unindent();
    }
}
