# Apollo Mission Control Examples

This directory contains authentic Apollo-era mission control console interfaces that display live telemetry from KSA via MQTT.

## Available Consoles

### FDO (Flight Dynamics Officer) Console
**File**: `fdo-console.html`

Authentic CRT-style display showing real-time trajectory data with green phosphor aesthetic.

**Features**:
- Live MQTT telemetry integration
- Velocity, altitude, and orbital element monitoring
- Abort authority panel (FDO had direct abort authority during launch)
- Mission event log with timestamps
- Authentic Apollo-era styling (scanlines, flicker, CRT effects)

**MQTT Topics**:
- `ksa/telemetry/vehicle` - Vehicle state data
- `ksa/telemetry/orbit` - Orbital parameters
- `ksa/bridge/status` - Connection status

**Status Indicators**:
- `[NOM]` - Normal operation, connected to MQTT
- `[HOLD]` - Disconnected or waiting for data

## Quick Start

### Prerequisites
1. Mosquitto MQTT broker running with WebSocket support on port 9001
2. KSA-Bridge mod loaded in KSA game
3. KSA game running and publishing telemetry

### Launch FDO Console
Simply open `fdo-console.html` in your web browser:
```
file:///C:/Users/john_/dev/KSA-Bridge/examples/apollo-mission-control/fdo-console.html
```

The console will automatically:
1. Connect to MQTT broker at `ws://127.0.0.1:9001`
2. Subscribe to telemetry topics
3. Display live data as it arrives
4. Log connection events

## Reference Documentation

See `../../docs/apollo-mission-control-reference.md` for:
- Complete console position descriptions
- Authority structures and responsibilities
- Go/no-go polling procedures
- Implementation notes for additional consoles

## Implementation Notes

### Design Principles
- **Authenticity**: Based on actual Apollo Mission Control console layouts
- **Live Data**: No simulated data - all values come from KSA via MQTT
- **Period Accuracy**: CRT aesthetics match 1960s-70s displays
- **Operational Focus**: Displays information FDO would actually need

### Technical Stack
- Pure HTML/CSS/JavaScript (no build step required)
- MQTT.js library via CDN for WebSocket connectivity
- CSS animations for CRT effects (scanlines, flicker, glow)
- Monospace fonts for authentic terminal appearance

### Future Console Examples
Additional console positions could be implemented:
- EECOM - Environmental and electrical systems
- CAPCOM - Capsule communicator (crew interface)
- FLIGHT - Flight director (mission authority)
- RETRO - Retrofire officer (entry trajectory)
- GUIDO - Guidance officer

**Note**: Only FDO console is currently implemented per user request.

## Troubleshooting

### Console shows [HOLD] status
- Check Mosquitto is running: `Get-Service mosquitto`
- Verify WebSocket listener on port 9001 in mosquitto config
- Ensure KSA game is running with KSA-Bridge mod loaded

### No telemetry data appearing
- Check KSA game console for "[KSA-Bridge] Publishing..." messages
- Verify MQTT broker is accessible at 127.0.0.1:1884 and ws://127.0.0.1:9001
- Check browser console (F12) for connection errors

### CRT effects not showing
- Ensure you're viewing in a modern browser (Chrome, Firefox, Edge)
- Check browser console for CSS errors
- Try hard refresh (Ctrl+F5)

## Credits

Console designs based on:
- "Apollo Flight Controller 101" by Lee Hutchinson, Ars Technica (Dec 24, 2019)
- NASA Apollo Mission Control documentation
- Apollo-era console photographs and diagrams

Icon library: SmartLab Icons (combined NetOps + ArtemisOps sets)
Located at: `../../smartlab-icons-complete.js`
