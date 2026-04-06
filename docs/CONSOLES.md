# KSA-Bridge Example Consoles

KSA-Bridge includes two ready-to-use example consoles that receive live telemetry data from the game via MQTT. Both consoles connect to the Mosquitto broker and display real-time vehicle, orbital, and trajectory data.

## Running the Consoles

1. **Start the HTTP Server**
   ```
   cd examples
   python -m http.server 8088
   ```
   The server will listen on `http://127.0.0.1:8088`

2. **Ensure Mosquitto is running**
   - The broker must be accessible at `127.0.0.1:9001` (WebSocket)
   - Typically started automatically when the game launches with KSA-Bridge

3. **Launch the game**
   - Load Kerbal Space Program with the KSA-Bridge mod
   - The mod automatically publishes telemetry to the MQTT broker

4. **Open a console in your browser**
   - Apollo: `http://127.0.0.1:8088/apollo-mission-control/`
   - Hard Sci-Fi: `http://127.0.0.1:8088/hard-scifi/`

## Apollo Mission Control FDO Console

A faithful recreation of the Apollo-era Flight Dynamics Officer display from NASA's Mission Control Center.

**Features:**
- Dense columnar data layout matching 1960s-70s mission control aesthetics
- Blue text on dark background with CRT scanlines effect
- Real-time telemetry updates at 1-10 Hz
- Organized into logical sections: orbital dynamics, velocity, altitude/position, attitude, vehicle, resources, maneuvers, mission elapsed time

**Display Sections:**

| Section | Data Points |
|---------|------------|
| **ODO** (Orbital Dynamics) | Apoapsis, Periapsis, Inclination, Eccentricity, Semi-Major Axis, LAN, AOP, Period |
| **VEL** (Velocity) | Orbital, Surface, Vertical |
| **ALT** (Altitude & Position) | Altitude, Latitude, Longitude |
| **ATT** (Attitude) | Heading, Pitch, Roll |
| **VEH** (Vehicle) | Name, Mass |
| **RES** (Resources) | Fuel, Oxidizer, Monopropellant, Electric, TWR |
| **MAN** (Maneuver) | Time to Ignition, Burn Duration, Delta-V |
| **MET** (Mission Elapsed Time) | Mission clock |

**Technical Details:**
- MQTT Topics: `ksa/telemetry/#` (wildcard subscription)
- Update Rate: Responds to incoming telemetry messages in real-time
- Library: mqtt.min.js (local copy in `lib/` directory)
- Protocol: WebSocket connection to MQTT broker

**Responsive Design:**
- Scales fluidly from mobile phones to 4K displays
- Font sizes adapt between 10px–16px based on viewport width
- Mobile optimization (≤768px): adjusted layout and touch-friendly spacing
- Large displays (>1920px): automatic 2-column layout for better use of horizontal space
- Ultra-wide displays (>2560px): 3-column layout

![Apollo Mission Control Console](/docs/screenshots/apollo-console.png)

---

## Hard Sci-Fi FDO Console

A modern, visually rich Flight Dynamics Officer console featuring a 3D orbital visualization using Three.js.

**Features:**
- Real-time 3D globe visualization showing vehicle position and orbit
- Orbital element markers (apoapsis, periapsis) projected onto the globe
- Comprehensive data panels with trajectory state, orbital elements, resources, and maneuver planning
- Light/Dark theme toggle
- Event log tracking MQTT connection status and mission events
- Trajectory history visualization (sparklines)
- Advanced mission planning displays including state vectors and orbital timings

**Layout:**
- **Center**: 3D Globe with orbital visualization
- **Left Panels**: Trajectory state, orbital elements, resources, trajectory history
- **Right Panels**: Maneuver planning, event log, display controls
- **Header**: Vehicle name, current body, mission phase, GET, MQTT status
- **Footer**: Status indicators and system health LEDs

**Technical Details:**
- MQTT Topics: `ksa/telemetry/#` (wildcard subscription) + `ksa/bridge/status`
- Update Rate: 1-10 Hz depending on telemetry source
- Libraries: mqtt.min.js, three.min.js, topojson-client.min.js (all local copies in `lib/`)
- Protocol: WebSocket connection to MQTT broker
- 3D Rendering: Three.js with interactive controls

**Color Scheme:**
- Dark theme (default): Cyan accents (#4a9eff), dark blue background (#060810)
- Light theme: Orange/green accents for NASA Martian habitat aesthetic
- Status indicators: Green (nominal), Yellow (caution), Red (alarm)

![Hard Sci-Fi FDO Console](/docs/screenshots/hardscifi-console.png)

---

## Data Transformations and Calculations

Both consoles process raw telemetry data and perform calculations before display. This ensures values are in the correct units and scale for human interpretation.

### Unit Conversions

**Angle Conversions (Radians → Degrees):**
- Heading (HDG): `heading_rad × (180/π)`
- Pitch: `pitch_rad × (180/π)`
- Roll: `roll_rad × (180/π)`
- Inclination (INCL): `inclination_rad × (180/π)`
- LAN (Long. Asc. Node): `lan_rad × (180/π)`
- AOP (Argument of Perigee): `aop_rad × (180/π)`

If your MQTT source publishes angles in degrees already, these conversions will double-convert them. Verify your data source format.

**Orbital Period:**
- Converted from seconds to minutes: `period_seconds / 60`

**Semi-Major Axis:**
- Converted from meters to kilometers: `semiMajorAxis_m / 1000`

### Calculated Values

**Thrust-to-Weight Ratio (TWR):**
```
TWR = Thrust (N) / (Mass (kg) × g₀)
where g₀ = 9.81 m/s²
```

This indicates how many Gs of acceleration the vehicle can achieve. A TWR of 1.0 means the vehicle can achieve 1G of acceleration at current mass and thrust.

---

## Telemetry Data Format

Both consoles expect telemetry messages in JSON format published to MQTT topics. The topics follow this pattern:

```
ksa/telemetry/vehicle
ksa/telemetry/orbit
ksa/telemetry/velocity
ksa/telemetry/altitude
ksa/telemetry/attitude
ksa/telemetry/resources
ksa/telemetry/maneuver
ksa/telemetry/mission-time
ksa/bridge/status
```

**Example Messages:**

Vehicle:
```json
{"name": "Rocket-1", "status": "active", "mass": 45000}
```

Orbit:
```json
{"apoapsis": 6378000, "periapsis": 6371000, "inclination": 51.6, "eccentricity": 0.001, "period": 5400}
```

Velocity:
```json
{"orbital": 7800, "surface": 7800, "vertical": 0}
```

Altitude:
```json
{"value": 250, "latitude": 0.123, "longitude": 45.678}
```

Attitude:
```json
{"heading": 90, "pitch": 0, "roll": 0}
```

Resources:
```json
{"fuel": 5000, "oxidizer": 6000, "monopropellant": 500, "electric": 800}
```

Maneuver:
```json
{"timeToBurn": 300, "burnDuration": 120, "deltaV": 1500}
```

Mission Time:
```json
{"value": 3661}
```

---

## Architecture

### MQTT Data Flow

```
Game (KSA-Bridge Mod)
    ↓
Mosquitto MQTT Broker (ports 1884 MQTT, 9001 WebSocket)
    ↓
Web Consoles (WebSocket clients)
    ↓
Browser Display (Apollo or Hard Sci-Fi)
```

### Connection Details

- **Broker Address**: `127.0.0.1:1884` (MQTT) / `127.0.0.1:9001` (WebSocket)
- **Allow Anonymous**: Yes (no authentication required)
- **QoS**: 0 (at-most-once, real-time data)
- **Retained Messages**: Not used (fresh data only)

### Web Server

- **Server Type**: Python built-in HTTP server
- **Port**: 8088 (configurable)
- **Root Directory**: `examples/`
- **Static Files Only**: No server-side processing

---

## Customization

### Modifying Console Appearance

Both consoles are standalone HTML files with embedded CSS and JavaScript. To customize:

1. **Colors**: Edit CSS color variables at the top of the `<style>` block
   - Apollo: `#00d4ff` (text), `#000011` (background)
   - Hard Sci-Fi: CSS custom properties in `:root`

2. **Font Sizes**: Apollo uses responsive `clamp()` for automatic scaling
   - Edit `--base-font-size`, `--header-font-size` variables

3. **Display Layout**: Modify the console-wrapper styles or add media queries

4. **Data Display**: Update the `updateDisplay()` JavaScript function to change what's shown

### Adding New Data Fields

1. Add a new data property to `telemetryData` object
2. Parse it in the MQTT `message` handler
3. Display it in `updateDisplay()` function

Example:
```javascript
else if (topic.includes('custom')) {
    telemetryData.customValue = data.value;
}
```

---

## Troubleshooting

**Console shows "DISCONNECTED"**
- Verify Mosquitto is running: `mosquitto -c config/mosquitto.conf`
- Check WebSocket listener on port 9001: `netstat -an | grep 9001`
- Ensure the game is running and KSA-Bridge mod is active
- Check browser console (F12) for JavaScript errors

**No telemetry data appearing**
- Verify the game is running
- Check that the vehicle has been launched
- Confirm MQTT broker shows incoming messages: `mosquitto_sub -h 127.0.0.1 -p 1884 -t 'ksa/telemetry/#'`

**Consoles not loading**
- Verify HTTP server is running: `python -m http.server 8088` in the `examples/` directory
- Check that you're accessing `http://127.0.0.1:8088` (not `localhost`)
- Clear browser cache (Ctrl+Shift+Delete)

**Hard Sci-Fi 3D globe not rendering**
- Ensure `lib/three.min.js` is present and accessible
- Check browser console for WebGL errors
- Try a different browser (Chrome, Firefox, Edge)

---

## Technical Requirements

- **Browser**: Modern browser with WebSocket and WebGL support (Chrome, Firefox, Edge, Safari)
- **MQTT Broker**: Mosquitto 1.6+ with WebSocket listener enabled
- **Game**: Kerbal Space Program with StarMap 0.4.x API support
- **Network**: Local network access (no internet required)

---

## File Structure

```
examples/
├── apollo-mission-control/
│   ├── fdo-console.html          # Apollo console (responsive)
│   ├── apollo-fdo-console.html   # Legacy variant
│   ├── README.md                 # Console documentation
│   └── lib/                       # Shared libraries
│       ├── mqtt.min.js
│       └── topojson-client.min.js
├── hard-scifi/
│   ├── hardscifi-fdo-console.html # Hard Sci-Fi console
│   ├── hardscifi-fdo-console-cdn.html # CDN variant
│   ├── lib/                       # Console-specific libraries
│   │   ├── mqtt.min.js
│   │   ├── three.min.js
│   │   ├── topojson-client.min.js
│   │   └── LICENSES.md
│   └── data/                      # Terrain/celestial body data
│       ├── jupiter_bands.geojson
│       ├── mars_contacts.geojson
│       └── ...
└── serve-examples.bat             # HTTP server launcher
```

---

## Future Enhancements

- [ ] WebGL-based Apollo console with 3D visualization
- [ ] Custom telemetry field configuration
- [ ] Data recording and playback
- [ ] Multi-vehicle tracking
- [ ] Alarm/warning system for orbital parameters
- [ ] Integration with Kerbal Alarm Clock
- [ ] Mobile app version
- [ ] Real-time collaboration (multiple consoles synced)
