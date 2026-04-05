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
- Per-body atmosphere glow (blue for Earth, amber for Mars, grey for Moon, etc.)
- USGS SIM 3292 Mars geologic boundary data, color-coded by contact type (Certain, Approximate, Internal, Border)

### Apollo Mission Control — FDO Console
`examples/apollo-mission-control/apollo-fdo-console.html`

A phosphor-green terminal aesthetic inspired by 1960s NASA mission control.

## Architecture

```
KSA Game ←→ KSA-Bridge (C# mod) → MQTT Broker (Mosquitto) → Web Consoles
                                        └
                                   Port 1884 (MQTT)
                                   Port 9001 (WebSocket)
```

The mod uses the StarMap 0.4.x API to read vehicle and orbit data, then publishes JSON payloads via MQTTnet. Web consoles connect to the broker over WebSocket (port 9001) using mqtt.js.

The mod itself does not open any ports — it only connects outbound to the MQTT broker. The ports in the diagram (1884 and 9001) are Mosquitto's listeners, configured in your `mosquitto.conf`.

The sample consoles in `examples/` are standalone HTML files — serve them however you like (any local web server, open directly in a browser, host on a Raspberry Pi, etc.). The mod's only job is publishing telemetry over MQTT. What you do with that data on the receiving end is up to you.

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

### For Beginners: Step-by-Step Setup

Follow **[SETUP.md](SETUP.md)** — it walks you through everything for your platform:

- **Windows** users: Run `setup.bat`
- **Linux/macOS** users: Run `./setup.sh`
- **Docker users**: `docker-compose up`

Each includes verification steps and troubleshooting.

### For Experienced Developers: Manual Setup

1. Install prerequisites: .NET 10.0 SDK, Mosquitto MQTT broker
2. Build: `cd KSA-Bridge && dotnet build --configuration Release`
3. Deploy DLL + config files to your KSA mods directory
4. Launch KSA and verify `[KSA-Bridge] Connected to 127.0.0.1:1884` in logs
5. Serve examples: `cd examples && python -m http.server 8088`
6. Open console: `http://localhost:8088/hard-scifi/hardscifi-fdo-console.html`

See [INSTALLATION.md](INSTALLATION.md) for platform-specific paths.

### One-Command Setup (Docker)

If you have Docker installed:
```bash
docker-compose up
```

Then launch KSA and open `http://localhost:8088/hard-scifi/hardscifi-fdo-console.html`

## Surface Data

The FDO console renders surface features on the 3D globe for spatial context. Data is loaded per-body from the `examples/hard-scifi/data/` directory:

| Body | Data Source | Format |
|------|-------------|--------|
| Earth | Natural Earth 110m | TopoJSON (CDN) |
| Moon | Mare boundaries, craters | TopoJSON |
| Mars | USGS SIM 3292 Global Geologic Map | GeoJSON |
| Jupiter | Cloud band boundaries, storm outlines | GeoJSON |
| Mercury | Crater rings | TopoJSON |
| Venus | Feature outlines | TopoJSON |

**Mars geologic contacts** are derived from the [USGS SIM 3292](https://pubs.usgs.gov/sim/3292/) Global Geologic Map of Mars (Tanaka et al., 2014). The source shapefile (`SIM3292_Global_Contacts.shp`, ~32 MB) uses the GCS Mars 2000 Sphere projection (geographic lat/lon on a Mars ellipsoid). To regenerate the simplified GeoJSON from the raw USGS data:

```bash
# Requires: pip install geopandas fiona shapely
# Place USGS SIM3292 shapefiles in examples/hard-scifi/data/usgs_raw/
python scripts/data-gen/convert_mars.py
```

This reads the contacts shapefile, simplifies geometry (tolerance 1.0°), removes null geometries, and writes `examples/hard-scifi/data/mars_contacts.geojson` (~842 KB, 3708 features). Contact types are color-coded on the globe: Certain boundaries in amber, Approximate in dark rust, Internal in teal, and Border contacts in bright gold.

**Jupiter cloud bands** are generated by `generate_jupiter.py` from established belt/zone latitude boundaries (Rogers 1995, Voyager/Cassini jet stream data). The script produces `jupiter_bands.geojson` with band boundary lines, halftone-style dot fill inside the dark belts, and storm outlines with concentric rings and dot fill for the Great Red Spot and Oval BA. Orbit and marker positions are automatically scaled to match the globe for all bodies.

## Coordinate Systems

KSA uses a Z-up coordinate system (CCI — Celestial Centered Inertial). The web consoles convert to Three.js Y-up:

- **Position:** CCI `(x, y, z)` → Three.js `(x, z, -y)`
- **Quaternion:** CCI `(qx, qy, qz, qw)` → Three.js `(qx, qz, -qy, qw)`
- **Keplerian rotation:** CCI `Rz(LAN)·Rx(Inc)·Rz(AoP)` → Three.js `Ry(LAN)·Rx(Inc)·Ry(AoP)` — same angles, no negation

## UI Style Guide

The `docs/` directory contains the Near-Future Hard Sci-Fi UI/UX Style Guide, which defines the visual language for all sample consoles. It synthesizes the design principles of *The Martian* (Territory Studio), *The Expanse*, and *Project Hail Mary* into a practical component catalog.

## Related Projects

- **[KSA-PAO](docs/KSA-PAO-Design-v0.2.md)** — Public Affairs Officer companion app. Subscribes to KSA-Bridge telemetry and generates NASA-style mission commentary in real time using templates and LLM-generated narrative via Piper TTS. Publishes commentary back to the broker on `ksa/pao/announcement`.

## License

MIT

## Author

John M. Knight — Florida, USA — 2026
