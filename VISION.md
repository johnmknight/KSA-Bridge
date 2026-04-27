# KSA-Bridge
## Vision Document v0.1

John Knight | April 2026

License: MIT | Runtime: .NET 10.0 / StarMap 0.4.x | Broker: Mosquitto MQTT

---

## 1. What KSA-Bridge Is

KSA-Bridge is a C# mod for Kitten Space Agency that reads game state every frame and publishes structured telemetry over MQTT. It is a dumb pipe. It does not interpret the data, does not make decisions, and does not present anything to the user. It reads, serializes, and publishes. What happens on the other side of the broker is not its concern.

Any MQTT client — a web page, a Python script, a Node.js app, an ESP32, a Home Assistant automation — can subscribe and receive live mission data. KSA-Bridge does not know or care what its consumers are. It publishes to a broker. The broker routes to subscribers. The architecture is fully decoupled.

This is the foundational design decision. Everything else follows from it.

## 2. Why It Exists

Kitten Space Agency is a space simulation with a rich internal model — orbital mechanics, attitude dynamics, atmospheric flight, maneuver planning, SOI transitions, multi-body physics. At its heart, KSA is about inspiration and education, free for everyone. That data, however, is locked inside the game. KSA-Bridge unlocks it.

KSA-Bridge exists to extend KSA's mission of inspiration and education beyond the game window. By publishing structured telemetry over an open protocol, it turns a space sim into a live data source that anyone can build on — students learning data visualization, makers building physical mission control panels, hobbyists writing companion apps, educators connecting orbital mechanics to real data pipelines.

The game provides the simulation. KSA-Bridge provides the data. The builder provides the imagination.
## 3. Design Philosophy

![KSA-Bridge Data Flow](docs/diagrams/ksa-bridge-dataflow.svg)

### 3.1 Dumb Pipe

KSA-Bridge is a publisher, not an application. It reads the KSA API, serializes the result as JSON, and publishes it to MQTT topics. It does not filter, aggregate, smooth, interpolate, or enrich the data. If a consumer needs a moving average, the consumer computes it. If a consumer needs derived values, the consumer derives them. The bridge is stateless between frames.

This is the same architecture used by EDMC-Telemetry (Edward Wright, @fasteddy516), which bridges Elite Dangerous game state to MQTT for external consumption. EDMC-Telemetry proved the model: a dumb pipe publishing game state for open-ended consumption. KSA-Bridge follows that lineage directly. See Section 7 for full prior art discussion.

### 3.2 Decoupled by Design

KSA-Bridge connects outbound to an MQTT broker. It does not open any ports. It does not serve web pages. It does not host an API. The mod's only network activity is a single outbound TCP connection to Mosquitto.

Consumers connect to the broker independently. A web console connects over WebSocket (port 9001). A Python script connects over MQTT (port 1884). An ESP32 connects over MQTT. None of them know about each other. None of them know about KSA-Bridge. They know about topics on a broker.

This means consumers can be developed, tested, and run without the game. Record a telemetry session to a log file, replay it to the broker, and your console works identically. This is critical for the educational and maker use cases — a student building a dashboard does not need KSA running to develop and test their work.

### 3.3 Publish Everything, Curate Nothing

KSA-Bridge publishes 13 telemetry topics at rates from 1 Hz to 10 Hz. The topic structure follows the game's data model, not any particular consumer's needs. A consumer subscribes to the topics it needs and ignores the rest. Each consumer defines its own data surface from the full telemetry stream.

### 3.4 The Broker Is the Integration Point

Mosquitto runs on the home network (192.168.4.51, ports 1884/9001). It is the single integration point for everything. The broker is not just a transport for KSA-Bridge telemetry; it is the bus for the entire KSA ecosystem. Any consumer can also be a producer — publishing derived data, generated content, or control signals back to the broker for other subscribers to consume.
## 4. STEM and Enablement

KSA is built on the idea that space simulation should inspire and educate, free for all. KSA-Bridge extends that principle by turning the game into a live data platform. The same telemetry that drives the simulation — Keplerian orbital elements, state vectors, attitude quaternions, atmospheric models — becomes accessible to anyone who can connect an MQTT client.

### 4.1 What This Enables

The combination of a real physics simulation, structured JSON telemetry, and a standard IoT protocol creates a platform that spans disciplines:

**Data analytics and visualization.** Live telemetry at 2-10 Hz is a real data stream. Students and hobbyists can build dashboards, plot orbital elements over time, compute derived quantities, practice real-time data processing. The sample FDO consoles are proof of concept — Three.js globe with live orbit rendering, sparkline history, orbital timing panels — but the telemetry is available for any visualization framework: D3, Grafana, Python matplotlib, Excel, anything that can subscribe to MQTT.

**Physical computing and IoT.** MQTT is the standard protocol for IoT devices. An ESP32 with an OLED display can subscribe to `ksa/telemetry/navigation` and show altitude and velocity in real time. A Raspberry Pi can drive a physical mission control panel with LED bar graphs for fuel level and TWR. An Arduino with servo motors can build a physical attitude indicator. These are real maker projects that connect a space sim to tangible hardware — the same skills used in professional telemetry and monitoring systems.

**Robotics and control interfaces.** The telemetry stream provides real-time state data that can drive robotic displays, motorized indicators, or ambient installations. A model solar system with motorized planets could track SOI transitions. A motorized camera mount could follow the ship's attitude quaternion. The data is structured and continuous enough to drive physical actuators.

**Digital twin concepts.** The FDO console is already a digital twin — a 3D representation of the vehicle's state, updated in real time from telemetry. The architecture generalizes: any 3D environment (Unity, Unreal, Three.js, Godot) can subscribe to state vectors and attitude and render a synchronized external view of the mission. This is the same concept used in professional mission operations, spacecraft engineering, and simulation training.

**Home automation integration.** Mosquitto is the same broker used by Home Assistant, Node-RED, and other home automation platforms. Mission events can trigger automations: dim the lights at liftoff, flash an alert on anomaly detection, display mission status on a smart display. The telemetry is already on the home network in the format these systems expect.
### 4.2 Classroom Use Cases

A physics teacher can point a class at a live orbital mechanics simulation with real Keplerian elements updating in real time. A computer science class can write MQTT subscribers in Python as a networking exercise — with a data source more compelling than temperature sensors. A maker club can build physical mission control panels as a group project, each station subscribing to different telemetry topics.

The barrier to entry is deliberately low. MQTT client libraries exist for Python, JavaScript, C, C++, Go, Rust, Java, and every major language. A minimal subscriber that prints live altitude to the console is five lines of Python. The sample consoles in the examples/ directory are standalone HTML files with no build system — open in a browser and they connect.

### 4.3 Maker and Hobbyist Projects

Beyond the classroom, the open protocol invites the kind of creative projects that thrive in maker and hobbyist communities: ambient displays that show mission status, physical telemetry gauges, LED installations that pulse with engine thrust, e-ink dashboards on a Raspberry Pi, OBS streaming overlays, voice-controlled mission queries. The data is there. The protocol is standard. What people build with it is up to them.

## 5. Architecture

![KSA-Bridge Architecture](docs/diagrams/ksa-bridge-architecture.svg)

KSA-Bridge connects outbound only. It does not open any ports. The broker is the single integration point. Consumers connect to the broker independently over MQTT or WebSocket. No consumer knows about KSA-Bridge or any other consumer. They know about topics on a broker.
## 6. Prior Art and Lineage

### 6.1 EDMC-Telemetry

EDMC-Telemetry (github.com/fasteddy516/EDMC-Telemetry) by Edward Wright is the closest known prior art for game-state-to-MQTT bridging. It reads Elite Dangerous game state via the EDMC plugin API and publishes it to an MQTT broker for open-ended consumption. The architecture — a dumb pipe with no opinions about what consumers do with the data — is the model KSA-Bridge follows.

Key lessons from EDMC-Telemetry's approach: publish raw game state, not derived values. Use standard protocols so any client in any language can subscribe. Keep the bridge stateless — if it crashes and restarts, it just starts publishing again. The bridge does not know its consumers. This is a feature, not a limitation.

### 6.2 Telemachus (KSP)

Telemachus (KSP mod by Rich Mayfield / TehGimp) was an earlier approach to the same problem: exposing KSP telemetry for external consumption. It used an embedded HTTP server inside the KSP mod, serving JSON over REST and WebSocket. Consumers connected directly to the game process.

KSA-Bridge explicitly avoids this architecture. An embedded HTTP server in the game process couples the transport to the game lifecycle, requires the game to accept inbound connections (firewall/NAT issues), and means a consumer crash can potentially affect the game. MQTT decouples all of this — the broker is a separate process, consumers connect to the broker, and the game only makes outbound connections.

### 6.3 Chatterer (KSP)

Chatterer (Athlonic / Iannic-ann-od) added ambient radio chatter to KSP using pre-recorded NASA audio clips played at random intervals. The effect — background voice traffic that makes a mission feel inhabited — is the direct inspiration for KSA-PAO's flight loop chatter layer. KSA-PAO replaces random pre-recorded clips with AI-generated contextual dialog tied to live telemetry.