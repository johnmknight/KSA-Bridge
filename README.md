# KSA-Bridge

MQTT telemetry bridge for [Kitten Space Agency](https://store.steampowered.com/app/2503020/Kitten_Space_Agency/). Publishes real-time vehicle state, orbital mechanics, and mission data to an MQTT broker, enabling web-based mission control consoles and external tools.

## What It Does

KSA-Bridge is a C# StarMap mod that reads the KSA game API every frame and publishes structured telemetry over MQTT. Any MQTT client — a web page, a Python script, a Node.js app — can subscribe and get live data.

**13 telemetry topics** covering vehicle state, orbital elements, state vectors, attitude, navigation, dynamics, atmosphere, maneuvers, encounters, and parent body rotation.

## Sample Consoles

The `examples/` directory contains ready-to-use mission control displays that consume KSA-Bridge telemetry:

### Hard Sci-Fi — FDO Console
`examples/hard-scifi/hardscifi-fdo-console.html`

A Three.js holographic 3D globe with live orbit rendering, continent outlines, and projected markers. Inspired by the UI design language of *The Martian*, *The Expanse*, and *Project Hail Mary*.

- Real-time 3D orbit ellipse with Ap/Pe/Ship markers
- Planet rotation from CCF→CCI quaternion telemetry
- Natural Earth 110m coastline overlay
- Ship tracking camera mode
- Dark (Ship Bridge) and Light (HABitat Research) themes
- Trajectory history sparklines, orbital timing, maneuver plan panels

### Apollo Mission Control — FDO Console
`examples/apollo-mission-control/apollo-fdo-console.html`

A phosphor-green terminal aesthetic inspired by 1960s NASA mission control.

## Architecture

```
KSA Game ←→ KSA-Bridge (C# mod) → MQTT Broker (Mosquitto) → Web Consoles
                                         ↑
                                    Port 1884 (MQTT)
                                    Port 9001 (WebSocket)
```

The mod uses the StarMap 0.4.x API to read vehicle and orbit data, then publishes JSON payloads via MQTTnet. Web consoles connect over WebSocket (port 9001) using mqtt.js.

## MQTT Topics

| Topic | Rate | Contents |
|-------|------|----------|
| `ksa/telemetry/vehicle` | 10 Hz | Vehicle name, parent body, situation, orbital speed |
| `ksa/telemetry/orbit` | 2 Hz | Apoapsis, periapsis, eccentricity, inclination, LAN, AoP, SMA, period |
| `ksa/telemetry/state_vectors` | 10 Hz | CCI position & velocity (double3) |
| `ksa/telemetry/attitude` | 10 Hz | Heading, pitch, roll, angular rates |
| `ksa/telemetry/navigation` | 10 Hz | Altitude, speed, orbital speed |
| `ksa/telemetry/dynamics` | 2 Hz | Body rates, acceleration, angular acceleration |
| `ksa/telemetry/resources` | 2 Hz | Fuel, propellant mass, total mass |
| `ksa/telemetry/performance` | 2 Hz | Delta-V, TWR |
| `ksa/telemetry/situation` | 2 Hz | Situation enum, landed/splashed/flying flags |
| `ksa/telemetry/atmosphere` | 2 Hz | Atmospheric density, pressure, terrain radius |
| `ksa/telemetry/maneuver` | 2 Hz | Burn count, active burns, flight plan status |
| `ksa/telemetry/encounter` | 2 Hz | SOI encounters, closest approach distance |
| `ksa/telemetry/parent_body` | 2 Hz | CCF→CCI rotation quaternion, axial tilt, radius, mass |
| `ksa/bridge/status` | 1 Hz | Bridge connection status |

## Prerequisites

- [Kitten Space Agency](https://store.steampowered.com/app/2503020/Kitten_Space_Agency/) with StarMap mod support
- [Mosquitto MQTT Broker](https://mosquitto.org/) (or any MQTT broker with WebSocket support)
- .NET 10.0 SDK (for building the mod)

## Quick Start

1. **Install Mosquitto** and configure it to listen on port 1884 (MQTT) and port 9001 (WebSocket).

2. **Build the mod:**
   ```bash
   cd KSA-Bridge
   dotnet build --configuration Release
   ```

3. **Deploy** the built DLL to your KSA mods directory:
   ```
   <KSA Install>/mods/KSA-Bridge/KSA-Bridge.dll
   ```
   Also copy `mod.toml` to the same directory.

4. **Launch KSA** — the mod auto-connects to the MQTT broker and starts publishing.

5. **Open a console** — open `examples/hard-scifi/hardscifi-fdo-console.html` in a browser.

## Coordinate Systems

KSA uses a Z-up coordinate system (CCI — Celestial Centered Inertial). The web consoles convert to Three.js Y-up:

- **Position:** CCI `(x, y, z)` → Three.js `(x, z, -y)`
- **Quaternion:** CCI `(qx, qy, qz, qw)` → Three.js `(qx, qz, -qy, qw)`
- **Keplerian rotation:** CCI `Rz(LAN)·Rx(Inc)·Rz(AoP)` → Three.js `Ry(LAN)·Rx(Inc)·Ry(AoP)` — same angles, no negation

## UI Style Guide

The `docs/` directory contains the Near-Future Hard Sci-Fi UI/UX Style Guide, which defines the visual language for all sample consoles. It synthesizes the design principles of *The Martian* (Territory Studio), *The Expanse*, and *Project Hail Mary* into a practical component catalog.

## License

MIT

## Author

John M. Knight — Florida, USA — 2026
