# Apollo Mission Control - Console Reference
## KSA-Bridge Example Design Document

Based on "Apollo Flight Controller 101: Every console explained" by Lee Hutchinson, Ars Technica (Dec 24, 2019)

This document outlines the Apollo Mission Operations Control Room (MOCR) console positions and their roles, which will inform the design of KSA-Bridge's first multi-console example.

---

## Mission Control Room Layout

The Apollo MOCR featured **four rows of consoles**, each with specific responsibilities. Controllers were known by their position acronyms and call signs.

### **Flight Director** (FLIGHT)
- **Location**: Row 3, center position (elevated above the trench)
- **Role**: "Orchestra leader" with ultimate authority over mission success and crew safety
- **Authority**: 
  - Could override anyone to ensure crew safety
  - Only way to countermand: Fire the flight director
  - One of only two positions with direct abort authority during launch
- **Console Features**:
  - Passive monitoring (no direct spacecraft control)
  - Event indicator panel showing go/no-go status of all controllers
  - Abort request toggle
  - Mission logbooks
- **Famous Directors**: 
  - Gene Kranz (White Flight) - Apollo 11, 13, 15, 16, 17
  - Glynn Lunney (Black Flight) - Apollo 8, 10, 11, 13, 14, 15
  - Gerry Griffin (Gold Flight) - Apollo 7, 9, 11, 12, 13, 14, 15, 16, 17
  - Milt Windler (Maroon Flight) - Apollo 8, 10, 12, 13, 14, 15
  - Cliff Charlesworth - Apollo 11 green team (liftoff, TLI, EVA)

---

## ROW 1: "THE TRENCH" - Trajectory & Dynamics Specialists

The front row - lowest and darkest, closest to projection screens. Responsible for spacecraft trajectory and flight path.

### **BOOSTER** - Booster Systems Engineer
- **Position**: Far left of the trench
- **Crew**: Three people during launch (one per Saturn V stage)
- **Primary Responsibility**: Launch vehicle propulsion monitoring
- **Key Functions**:
  - Monitor all three Saturn V stages during ~9-minute ride to orbit
  - Monitor Trans-Lunar Injection (TLI) burn
  - Power to send abort command
- **After TLI**: Console occupied by mission-specific scientific experiment personnel
- **Staffing**: Employed at Marshall Space Flight Center, reported to JSC for launches

### **RETRO** - Retrofire Officer
- **Position**: Second from left in the trench
- **Primary Responsibility**: Getting the spacecraft back to Earth
- **Key Functions**:
  - Maintain abort option lists (procedures for when things go wrong)
  - Track Apollo Service Module main engine for lunar orbit departure
  - Plan Trans-Earth Injection (TEI) burns
  - Monitor retrofire systems and safe return trajectories
- **Authority**: Could call for mission abort

### **FDO** (FIDO) - Flight Dynamics Officer
- **Position**: Second from right in the trench
- **Pronunciation**: "FEE-doe"
- **Primary Responsibility**: Vehicle trajectory at all mission stages
- **Key Functions**:
  - Monitor spacecraft flight path
  - Calculate maneuver times
  - Track position via ground tracking stations
  - Coordinate with MSFN network
- **Special Authority**: 
  - Only station besides FLIGHT with direct abort authority during launch
  - Dedicated abort toggle switches
  - Critical role - trajectory deviations could be catastrophic
- **Notable**: Trajectories were "delicately calculated loops" - fractions of a degree could mean life or death

### **GUIDO** - Guidance Officer  
- **Position**: Far right of the trench
- **Pronunciation**: "GUIDE-oh"
- **Primary Responsibility**: Primary Guidance, Navigation, and Control Systems (PGNCS/"pings")
- **Key Functions**:
  - Monitor Command Module and Lunar Module computers
  - Track velocity and vector reporting
  - Ensure spacecraft computers' position matched reality
  - Monitor Lunar Module Abort Guidance System (AGS/"aggs")
- **Famous Moment**: 
  - Apollo 11 landing - LM computer overwhelmed (1202/1201 alarms)
  - GUIDO Steve Bales and backroom determined safe to proceed in 30 seconds
  - Saved the first moon landing
- **Note**: Software/computer focused (vs GNC hardware)

---

## ROW 2: Spacecraft Systems Specialists

Second row, elevated ~2 feet above the trench. Focused on spacecraft systems and crew health.

### **SURGEON** - Flight Surgeon
- **Position**: Far left of Row 2
- **Crew**: Medical doctor
- **Primary Responsibility**: Monitor crew health
- **Key Functions**:
  - Display electrocardiogram (ECG) and electropneumogram data
  - Track heart and breathing rates
  - Monitor body sensors
- **Reputation**: 
  - Often adversarial relationship with astronaut corps
  - "Pilots can leave doctor's office in one of two conditions: fine, or grounded"
  - Apollo 13: Chuck Berry grounded Ken Mattingly (measles exposure)
- **Anecdote**: Chris Kraft once played heart attack ECG tape - SURGEON said crew "looking just fine"
- **Cultural Note**: "Would show up 20 minutes before mission, knew where the camera was, just comb his hair"

### **CAPCOM** - Capsule Communicator
- **Position**: Second from left, Row 2
- **Primary Responsibility**: ONLY voice allowed to talk directly to crew
- **Crew**: Always an astronaut
- **Key Functions**:
  - Single point of contact with spacecraft
  - Translate controller recommendations into crew instructions
  - Provide "friendly, well-known voice"
  - Speak in "astronaut terms"
- **Rationale**:
  - Resource management (track all transmissions)
  - Crew safety (single instruction source)
  - Familiarity with spacecraft layout
- **Exception**: Gemini 4 - Chris Kraft directly ordered Ed White to end EVA (safety/time constraints)

### **EECOM** - Electrical, Environmental, and Consumables Manager
- **Position**: Center of Row 2
- **Original**: Electrical, Environmental, and COMmunications
- **After Apollo 10**: Communications removed (moved to INCO)
- **Primary Responsibility**: Command/Service Module electrical and environmental systems
- **Key Functions**:
  - Monitor power generation and distribution
  - Track fuel cell cryogenic levels
  - Cabin cooling and pressure control systems
  - Life support systems (oxygen for crew and fuel cells)
  - Vehicle lighting systems
- **Famous EECOMs**:
  - Sy Liebergot - On duty during Apollo 13 oxygen tank explosion
  - John Aaron - "Steely-eyed missile man"
    * Apollo 12: Lightning strike - switched to backup power filter
    * Apollo 13: Led power-up checklist development
- **Shuttle Era**: Position broken into two separate stations due to scope

### **GNC** - Guidance, Navigation, and Control Systems Engineer
- **Position**: Fourth from left, Row 2
- **Primary Responsibility**: Hardware side of spacecraft pointing/attitude
- **Key Functions**:
  - Monitor reaction control systems (RCS)
  - Track Service Module main engine hardware
  - Monitor guidance system hardware components
- **Distinction**: Hardware focus (vs GUIDO's software focus)

### **TELMU** - Telemetry, Electrical, and EVA Mobility Unit Officer
- **Position**: Fifth from left, Row 2
- **Primary Responsibility**: Lunar Module electrical and environmental systems
- **Key Functions**:
  - LM life-support monitoring
  - LM power systems (batteries - no fuel cells due to weight)
  - EVA spacesuit monitoring
- **Relationship**: EECOM's counterpart for the Lunar Module
- **Famous Moment**: Apollo 13 - solved LM lifeboat power-up chicken-and-egg problem

### **CONTROL** - LM Guidance, Navigation, and Control
- **Position**: Far right of Row 2
- **Primary Responsibility**: Lunar Module guidance hardware
- **Key Functions**:
  - Landing radar monitoring
  - Attitude thrusters
  - Ascent and descent rockets (hardware)
- **Relationship**: GNC's counterpart for Lunar Module
- **Note**: Software handled by GUIDO (LM and CM used same computer)

---

## ROW 3: Support & Command

Third row, another ~2 feet elevation. Support functions and flight director command.

### **INCO** - Instrumentation and Communications Officer
- **Position**: Far left of Row 3
- **Created**: After Apollo 10
- **Primary Responsibility**: All data, voice, and video communications
- **Key Functions**:
  - Monitor CM and LM communications systems
  - Manage telemetry link spacecraft↔ground
  - Handle all data transmission
- **Origin**: Tasks taken from EECOM and TELMU

### **PROCEDURES** (O&P) - Operations and Procedures
- **Position**: Second from left, Row 3
- **Primary Responsibility**: Ensure controllers follow procedures
- **Key Functions**:
  - Enforce flight control operations handbook
  - Process data printout requests
  - Process voice playback requests
  - Verify proper forms and GMT timestamps
- **Role**: Procedural compliance gatekeeper

### **Assistant FLIGHT** - Assistant Flight Director
- **Position**: Left of Flight Director, Row 3
- **Primary Responsibility**: Duplicate and supplement FLIGHT duties
- **Key Functions**:
  - Monitor mission alongside FLIGHT
  - "Did whatever the flight director wanted him to do"
  - Provide backup authority

### **FAO** - Flight Activities Officer
- **Position**: Right of Flight Director, Row 3
- **Primary Responsibility**: Mission timeline management
- **Key Functions**:
  - Ensure preplanned activities occur on schedule
  - Know all mission checklists
  - Understand time requirements for each step
- **Skills**: Comprehensive spacecraft systems knowledge

### **NETWORK** - Network Controller
- **Position**: Far right of Row 3
- **Primary Responsibility**: Interface with global MSFN tracking network
- **Key Functions**:
  - Ensure Mission Control receives needed data
  - Fix downed computers at remote sites
  - Manage data transmission inside MCC
  - First-line tech support for console problems
- **Network**: Manned Space Flight Network (MSFN) - global tracking stations
- **Pressure**: "Had a really pressured job" - remote failures were NETWORK's problem

---

## ROW 4: Management & Public Relations

Top row - management oversight and public interface.

### **PAO** - Public Affairs Officer
- **Position**: Far left of Row 4
- **Primary Responsibility**: "Voice of mission control" to the public
- **Key Functions**:
  - Provide audio narration for broadcasts
  - Explain mission events to public
  - Give media commentary foundation
- **Coverage**: Radio and television broadcasts
- **Note**: "You see those guys quite a bit - because the TV camera was up in that corner"

### **Flight Operations Director**
- **Position**: Second from left, Row 4
- **Crew**: Senior MSC director (often Chris Kraft during Apollo)
- **Primary Responsibility**: Interface between Mission Control and space center management
- **Authority**: Management liaison, not operational control

### **NASA HQ** - NASA Headquarters Liaison
- **Position**: Third from left, Row 4
- **Crew**: Representative from NASA Washington DC
- **Primary Responsibility**: Liaison with NASA headquarters
- **Activity**: "Rarely had anything to do, but usually someone there watching"

### **Department of Defense** - DOD Liaison
- **Position**: Far right of Row 4
- **Crew**: Usually a general
- **Primary Responsibility**: Coordinate spacecraft recovery after splashdown
- **Rationale**: Military responsible for Gemini/Apollo recovery operations
- **Console Feature**: "Really cool red phone"

---

## Projection Screens

**Five large rear-projection displays** dominated the front of MOCR 2:

### Center Display - "Ten by Twenty"
- **Size**: 10 feet tall × 20 feet wide
- **Purpose**: Vehicle position and status for current mission phase
- **Technology**: Physical slides overlaid on plots/number columns

### Side Displays (2 left, 2 right)
- **Left Screens**: Vehicle command history, current flight plan page
- **Right Screen**: Television images (mission cameras or network TV)

### Technology: Eidophor Projectors
- Powerful quartz-lamp video projectors
- Bounced images off mirrors onto screens
- "Very sharp image" quality

### Chronographic Displays
- Nine smaller displays above main screens
- Time and mission clock information

---

## Console Technology

### Interior Construction
- Original wiring/circuitry mostly removed
- Restoration plans included blinking lights (unfunded)
- Currently disconnected husks with Apollo panels
- Some Shuttle-era hardware remains (dated "July 1980")
- Pneumatic tube (p-tube) systems for document delivery
- Some circuit boards dated 1968-1969

### Event Indicator Panels
- Color-coded status: Green (ready), Amber (standby), Red (no-go)
- Used for "go/no-go poll" before launch/critical events
- Used during shift changes
- Provided immediate visual status to FLIGHT

---

## Key Concepts for KSA-Bridge Implementation

### Console Roles & Responsibilities
1. **Trajectory Specialists** (Trench) - Where the spacecraft is going
2. **Systems Specialists** (Row 2) - How spacecraft systems are performing  
3. **Support & Command** (Row 3) - Timeline, procedures, communications
4. **Management** (Row 4) - Public relations, oversight

### Authority Structure
- **Flight Director**: Ultimate authority
- **FLIGHT + FDO**: Only positions with direct abort authority
- **Booster**: Could send abort command during launch
- **Information Flow**: Backroom → Frontroom → Flight → Crew

### Critical Design Elements
- **Go/No-Go Polling**: Visual status indicators
- **Single Voice Rule**: Only CAPCOM talks to crew
- **Backroom Support**: Each console had expert support staff
- **Timeline Management**: FAO ensures mission stays on schedule
- **Network Coordination**: Global tracking network interface

### Monitoring Focus Areas
- **Trajectory**: Orbital mechanics, flight path, guidance
- **Power**: Generation, distribution, fuel cells, batteries
- **Life Support**: Oxygen, cabin pressure, temperature
- **Communications**: Voice, data, telemetry, video
- **Propulsion**: Engines, thrusters, fuel systems
- **Health**: Crew medical monitoring

---

## Implementation Notes for KSA-Bridge

This reference will inform:
1. **Multi-console dashboard layout** (4-row structure)
2. **Role-based data displays** (each console shows relevant subsystems)
3. **Authority/permission model** (FLIGHT has override, specific abort authorities)
4. **Communication patterns** (CAPCOM-only crew contact, go/no-go polling)
5. **Visual design** (Eidophor-style projection screens, event indicators)
6. **Icon usage** (Use SmartLab icons for spacecraft systems, sensors, communications)

The example will demonstrate how KSA-Bridge can simulate a mission control environment with different console stations monitoring different aspects of a simulated mission.

---

**Source**: "Apollo Flight Controller 101: Every console explained" by Lee Hutchinson, Ars Technica, December 24, 2019  
**Local Copy**: `C:\Users\john_\Downloads\Apollo Flight Controller 101_ Every console explained - Ars Technica.pdf`
