# KSA-Bridge Sample Telemetry

Sample JSON payloads and a publisher script for testing the example consoles
without running KSA. These simulate an Apollo-style low Earth orbit mission.

## Quick Start

### Prerequisites

- **Mosquitto** installed and running with WebSocket support on port 9001
  - Windows: `winget install EclipseMosquitto.Mosquitto`
  - Linux: `sudo apt install mosquitto mosquitto-clients`
  - Mac: `brew install mosquitto`

- Ensure your `mosquitto.conf` includes a WebSocket listener:
  ```
  listener 1884
  allow_anonymous true
  listener 9001
  protocol websockets
  ```

- Start Mosquitto:
  ```
  mosquitto -c mosquitto.conf
  ```

### Option 1: Publish All Samples at Once (Script)

The `publish-samples.sh` script publishes all sample payloads to the broker
in one shot, then continues publishing mission time updates every second so
the console stays alive.

```bash
# Linux / Mac / Git Bash on Windows
cd samples
chmod +x publish-samples.sh
./publish-samples.sh
```

For Windows Command Prompt, use the batch file:
```cmd
cd samples
publish-samples.bat
```

Then open a console in your browser:
- Apollo FDO: `http://127.0.0.1:8088/apollo-mission-control/fdo-console.html`
- Hard Sci-Fi: `http://127.0.0.1:8088/hard-scifi/hardscifi-fdo-console.html`

(Start the HTTP server first: `python -m http.server 8088` from the `examples/` directory.)

### Option 2: Publish Individual Topics Manually

Each `.json` file in this directory contains a single payload for one telemetry
topic. Publish them individually using `mosquitto_pub`:

```bash
mosquitto_pub -h 127.0.0.1 -p 1884 -t ksa/telemetry/vehicle -f vehicle.json
mosquitto_pub -h 127.0.0.1 -p 1884 -t ksa/telemetry/orbit -f orbit.json
mosquitto_pub -h 127.0.0.1 -p 1884 -t ksa/telemetry/velocity -f velocity.json
mosquitto_pub -h 127.0.0.1 -p 1884 -t ksa/telemetry/altitude -f altitude.json
mosquitto_pub -h 127.0.0.1 -p 1884 -t ksa/telemetry/attitude -f attitude.json
mosquitto_pub -h 127.0.0.1 -p 1884 -t ksa/telemetry/resources -f resources.json
mosquitto_pub -h 127.0.0.1 -p 1884 -t ksa/telemetry/maneuver -f maneuver.json
mosquitto_pub -h 127.0.0.1 -p 1884 -t ksa/telemetry/mission-time -f mission-time.json
```

## Sample Mission Profile

The sample data represents the following scenario:

- **Vehicle**: Explorer-7, a crewed spacecraft in low Earth orbit
- **Orbit**: ~250 km circular orbit at 51.6° inclination (ISS-like)
- **Phase**: Coasting in orbit, upcoming plane change maneuver in 5 minutes
- **Resources**: Approximately 60% propellant remaining

## Data Format Notes

The console code expects specific units from the bridge. These samples match
what the actual KSA-Bridge mod publishes:

| Field | Unit | Notes |
|-------|------|-------|
| Angles (inclination, LAN, AOP, heading, pitch, roll) | **Radians** | Console converts to degrees for display |
| Semi-major axis | **Meters** | Console converts to km |
| Orbital period | **Seconds** | Console converts to minutes |
| Altitude | **Kilometers** | Displayed as-is |
| Velocities | **m/s** | Displayed as-is |
| Mass | **kg** | Displayed as-is |
| Thrust | **Newtons** | Used to calculate TWR |
| Resources (fuel, oxidizer, etc.) | **Units** | Game-specific resource units |
| Mission time | **Seconds** | Console formats as HH:MM:SS |
