# KSA-Bridge Installation Guide

## Cross-Platform Support

KSA-Bridge works on **Windows, Linux, and macOS** with automatic config path detection.

---

## Quick Install by Platform

### Windows — recommended path: use the project's scripts

The project ships ready-to-run scripts that handle build, deploy, and the OneDrive-vs-plain-Documents path detection automatically. Most users should use these instead of manually running `dotnet` and `Copy-Item`.

**First-time setup** — verifies prerequisites, builds Release, deploys:
```powershell
.\setup.bat
```

**Iterative dev** — builds Debug, rotates backups (keeps 5 timestamped copies of the previous DLL), deploys:
```powershell
.\build-and-deploy.bat
```

After deploy, launch KSA and look for `[KSA-Bridge] Connected to 127.0.0.1:1884` in the game logs.

See [SETUP.md](SETUP.md) for the full walkthrough including Mosquitto setup and the **service-vs-manual conflict** that affects users who installed Mosquitto with the Windows Service option (or via Chocolatey).

### Windows — manual install (advanced)

If you'd rather know exactly what the scripts do, the manual equivalent is:

**1. Build the mod:**
```powershell
cd KSA-Bridge
dotnet build --configuration Release
```

**2. Deploy to user mods directory:**
```powershell
$modsPath = "$env:USERPROFILE\OneDrive\Documents\My Games\Kitten Space Agency\mods\KSA-Bridge"
# Or without OneDrive:
# $modsPath = "$env:USERPROFILE\Documents\My Games\Kitten Space Agency\mods\KSA-Bridge"

New-Item -ItemType Directory -Force -Path $modsPath
Copy-Item -Recurse -Force "bin\Release\net10.0\*" $modsPath
```

**3. Add config files:**
```powershell
Copy-Item "KSA-Bridge\mod.toml" $modsPath
Copy-Item "KSA-Bridge\ksa-bridge.toml" $modsPath
```

**4. Start KSA and verify:**
```
[KSA-Bridge] Found config at: C:\Users\{user}\Documents\My Games\Kitten Space Agency\mods\KSA-Bridge\ksa-bridge.toml
[KSA-Bridge] Connected to 127.0.0.1:1884
```

---

### Linux

**1. Build the mod:**
```bash
cd KSA-Bridge
dotnet build --configuration Release
```

**2. Deploy to user mods directory:**
```bash
# Primary location (XDG Data Home)
mkdir -p ~/.local/share/"Kitten Space Agency"/mods/KSA-Bridge
cp -r bin/Release/net10.0/* ~/.local/share/"Kitten Space Agency"/mods/KSA-Bridge/

# Alternative location (XDG Config Home)
# mkdir -p ~/.config/"Kitten Space Agency"/mods/KSA-Bridge
# cp -r bin/Release/net10.0/* ~/.config/"Kitten Space Agency"/mods/KSA-Bridge/
```

**3. Add config file:**
```bash
cp ksa-bridge.toml ~/.local/share/"Kitten Space Agency"/mods/KSA-Bridge/
```

**4. Start KSA and verify:**
```
[KSA-Bridge] Found config at: /home/{user}/.local/share/Kitten Space Agency/mods/KSA-Bridge/ksa-bridge.toml
[KSA-Bridge] Connected to 127.0.0.1:1884
```

---

### macOS

**1. Build the mod:**
```bash
cd KSA-Bridge
dotnet build --configuration Release
```

**2. Deploy to user mods directory:**
```bash
mkdir -p ~/Library/Application\ Support/Kitten\ Space\ Agency/mods/KSA-Bridge
cp -r bin/Release/net10.0/* ~/Library/Application\ Support/Kitten\ Space\ Agency/mods/KSA-Bridge/
```

**3. Add config file:**
```bash
cp ksa-bridge.toml ~/Library/Application\ Support/Kitten\ Space\ Agency/mods/KSA-Bridge/
```

**4. Start KSA and verify:**
```
[KSA-Bridge] Found config at: /Users/{user}/Library/Application Support/Kitten Space Agency/mods/KSA-Bridge/ksa-bridge.toml
[KSA-Bridge] Connected to 127.0.0.1:1884
```

---

## Config File Locations

The mod automatically searches for `ksa-bridge.toml` in these locations (in order):

| Platform | Priority | Location |
|----------|----------|----------|
| **Windows** | 1st | `Documents\My Games\Kitten Space Agency\mods\KSA-Bridge\` |
| **Linux** | 1st | `~/.local/share/Kitten Space Agency/mods/KSA-Bridge/` |
| **Linux** | 2nd | `~/.config/Kitten Space Agency/mods/KSA-Bridge/` |
| **macOS** | 1st | `~/Library/Application Support/Kitten Space Agency/mods/KSA-Bridge/` |
| **All** | Fallback | `{KSA_install}/Content/KSA-Bridge/` |

**The mod finds your config automatically** - just put it in your platform's standard location!

---

## MQTT Broker Setup

### Install Mosquitto

**Windows:**
```powershell
choco install mosquitto
# Or download from: https://mosquitto.org/download/
```

**Linux:**
```bash
# Ubuntu/Debian
sudo apt install mosquitto

# Fedora
sudo dnf install mosquitto

# Arch
sudo pacman -S mosquitto
```

**macOS:**
```bash
brew install mosquitto
```

### Configure Mosquitto

**The project ships a working config at `config/mosquitto.conf`** — you do **not** need to write your own. It defines the two listeners KSA-Bridge needs:

```conf
listener 1884
protocol mqtt
allow_anonymous true

listener 9001
protocol websockets
allow_anonymous true
```

### Start Mosquitto

**Windows (using the project's restart script):**
```powershell
.\scripts\restart-mosquitto.bat
```
This kills any running mosquitto and starts a new one with the project's config. See SETUP.md for the **service-vs-manual conflict** — if you installed Mosquitto as a Windows Service, you'll likely want to disable the service first (`Stop-Service mosquitto; Set-Service mosquitto -StartupType Manual` from an elevated PowerShell).

**Windows (manual):**
```powershell
& "C:\Program Files\Mosquitto\mosquitto.exe" -c config\mosquitto.conf -v
```

**Linux/macOS:**
```bash
mosquitto -c config/mosquitto.conf -v
```

---

## Configuration File

Edit `ksa-bridge.toml` with your MQTT settings:

```toml
[broker]
host = "127.0.0.1"
port = 1884
client_id = "ksa-bridge"
keepalive = 60

[publish]
topic_prefix = "ksa"
publish_mode = "processed"
telemetry_hz = 10
orbit_hz = 2
resources_hz = 2
```

---

## Verification

After installation, launch KSA and check the logs for:

✅ **Config Found:**
```
[KSA-Bridge] Found config at: {platform-specific-path}
```

✅ **Connected:**
```
[KSA-Bridge] Connected to 127.0.0.1:1884
```

✅ **Publishing:**
```
[KSA-Bridge] Published successfully to ksa/telemetry/vehicle
```

---

## Troubleshooting

### Config Not Found

The mod will list all locations it checked:
```
[KSA-Bridge] Config not found. Checked 5 locations:
[KSA-Bridge]   - {path1}
[KSA-Bridge]   - {path2}
...
```

Copy your config to the **first** listed location for your platform.

### Connection Refused

1. Verify Mosquitto is running:
   ```bash
   # Linux/macOS
   ps aux | grep mosquitto
   
   # Windows
   Get-Process | Where-Object {$_.Name -like "*mosquitto*"}
   ```

2. Check Mosquitto is listening on port 1884:
   ```bash
   # Linux/macOS
   netstat -an | grep 1884
   
   # Windows
   netstat -an | findstr 1884
   ```

3. Test MQTT connection:
   ```bash
   mosquitto_sub -h 127.0.0.1 -p 1884 -t "ksa/#" -v
   ```

### Platform-Specific Issues

**Linux - Wine/Proton:**
If running KSA through Wine/Proton, the mod will check Wine-compatible paths:
- `~/Documents/My Games/Kitten Space Agency/mods/KSA-Bridge/`

**macOS - Case Sensitivity:**
macOS is case-insensitive by default, but the mod uses exact paths. Ensure folder names match exactly:
- `Library` not `library`
- `Application Support` not `application support`

**Windows - OneDrive:**
If Documents is redirected to OneDrive, use:
- `%USERPROFILE%\OneDrive\Documents\My Games\...`

---

## Advanced: Custom Install Location

If KSA is installed to a non-standard location (e.g., `D:\Games\KSA`), the mod's fallback will still work:

The install directory fallback (`{install_dir}/Content/KSA-Bridge/`) discovers the actual install location at runtime using `AppContext.BaseDirectory`.

**No manual configuration needed** - the mod automatically adapts to any install location!

---

## Next Steps

After installation:

1. **Start the broker:** `scripts\restart-mosquitto.bat` (Windows) or `mosquitto -c config/mosquitto.conf -v` (Linux/macOS).
2. **Start the example web server:** `scripts\serve-examples.bat` (Windows) or `cd examples && python3 -m http.server 8088` (Linux/macOS).
3. **Launch KSA from the StarMap install directory.** Run `C:\Program Files\StarMap\launch-starmap.bat` (NOT the repo's `scripts\launch-starmap.bat`). StarMap.exe inherits its working directory from the launcher, and it requires CWD = its own install dir for mod loading to work. The `setup.bat` script deploys the repo's launch-starmap.bat to `C:\Program Files\StarMap\` at install time so the deployed copy stays in sync. Once the game is loaded, watch for `[KSA-Bridge] Connected to 127.0.0.1:1884` in the KSA log.
4. **Open a console** in your browser:
   - Hard Sci-Fi FDO: `http://localhost:8088/hard-scifi/hardscifi-fdo-console.html`
   - Apollo Mission Control: `http://localhost:8088/apollo-mission-control/apollo-fdo-console.html`
5. **Monitor MQTT directly** if you want raw topic output:
   ```bash
   mosquitto_sub -h 127.0.0.1 -p 1884 -t "ksa/#" -v
   ```

For the full day-to-day workflow including the canonical scripts and the typical iterative cycle, see [SETUP.md](SETUP.md) → "Day-to-Day Workflow (After First Install)".
