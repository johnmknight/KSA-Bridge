# KSA-Bridge Setup Guide

**For educators and students getting started with KSA-Bridge for the first time.**

This guide walks you through prerequisites, installation, and verification. Choose your platform below.

---

## Prerequisites Overview

KSA-Bridge has three components:

| Component | What It Does | Status |
|-----------|-------------|--------|
| **Kitten Space Agency** | The game (purchased from Steam) | You already have this |
| **Mosquitto MQTT Broker** | Message broker that connects the game to web consoles | Need to install/verify |
| **.NET 10.0 SDK** | Required to build the mod | Need to install/verify |

**Total setup time: 10-15 minutes** (mostly waiting for downloads)

---

## Platform: Windows

### Step 1: Verify .NET 10.0 SDK

Open PowerShell and run:
```powershell
dotnet --version
```

**Expected output:** `10.0.xxx` or higher

**If not installed:**
1. Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)
2. Choose ".NET 10.0 SDK" (not Runtime)
3. Run the installer, accept defaults
4. Restart PowerShell and re-run `dotnet --version`

### Step 2: Install Mosquitto MQTT Broker

Mosquitto is a lightweight message broker. KSA-Bridge needs it running in the background.

**Option A: Installer (Recommended for beginners)**
1. Download from [mosquitto.org/download](https://mosquitto.org/download/)
2. Choose "Windows" → download the `.exe` installer
3. Run installer, accept defaults (installs to `C:\Program Files\Mosquitto\`)
4. Choose "Install as Windows Service" during setup (lets it auto-start)

**Option B: Quick Check (Already Installed?)**
Open PowerShell:
```powershell
Get-Process mosquitto -ErrorAction SilentlyContinue
```
If you see `mosquitto` in the output, it's already running. Skip to **Step 3**.

### Step 3: Build and Deploy

Navigate to the KSA-Bridge repository folder and run:

```powershell
.\setup.bat
```

This script will:
- ✓ Verify Mosquitto is installed
- ✓ Verify Mosquitto is running
- ✓ Build the mod with .NET
- ✓ Deploy it to your KSA mods folder
- ✓ Display next steps

**Troubleshooting Step 3:**

If `setup.bat` fails:
- **"mosquitto: The term is not recognized"** → Mosquitto not in PATH. Restart computer after installing.
- **"Cannot find path to KSA mods"** → Edit `setup.bat` line 5 to match your KSA installation path
- **"dotnet: The term is not recognized"** → .NET SDK not installed or PATH not updated. Restart PowerShell.

### Step 4: Launch Game and Test

**Start Mosquitto** (if not auto-starting):
```powershell
Start-Process "C:\Program Files\Mosquitto\mosquitto.exe" -ArgumentList "-c config\mosquitto.conf"
```

**Launch KSA:**
```
C:\Program Files\StarMap\launch-starmap.bat
```

**Verify in game logs** (Check `Documents\My Games\Kitten Space Agency\logs\`):
```
[KSA-Bridge] Found config at: ...
[KSA-Bridge] Connected to 127.0.0.1:1884
```

### Step 5: Start Web Console

Open a new PowerShell window and run:
```powershell
.\scripts\serve-examples.bat
```

Then open your browser to: **http://localhost:8088/hard-scifi/hardscifi-fdo-console.html**

You should see live telemetry from KSA appearing on the console.

---

## Platform: Linux

### Step 1: Verify .NET 10.0 SDK

```bash
dotnet --version
```

**Expected output:** `10.0.xxx` or higher

**If not installed:**
```bash
# Ubuntu/Debian
sudo apt update
sudo apt install dotnet-sdk-10.0

# Fedora/RHEL
sudo dnf install dotnet-sdk-10.0

# Arch
sudo pacman -S dotnet-sdk-bin
```

### Step 2: Install Mosquitto MQTT Broker

```bash
# Ubuntu/Debian
sudo apt install mosquitto mosquitto-clients

# Fedora/RHEL
sudo dnf install mosquitto

# Arch
sudo pacman -S mosquitto

# macOS
brew install mosquitto
```

**Verify installation:**
```bash
which mosquitto
```

Should output a path like `/usr/sbin/mosquitto` or `/usr/local/bin/mosquitto`.

### Step 3: Build and Deploy

Navigate to the KSA-Bridge repository and run:

```bash
chmod +x setup.sh
./setup.sh
```

This script will:
- ✓ Verify Mosquitto is installed
- ✓ Verify .NET SDK is installed
- ✓ Build the mod
- ✓ Deploy to your KSA mods folder
- ✓ Display next steps

**Troubleshooting Step 3:**

If `setup.sh` fails:
- **"mosquitto: command not found"** → Mosquitto not installed. Run the install command above.
- **"dotnet: command not found"** → .NET SDK not installed. Run the install command above.
- **"Permission denied"** → Run `chmod +x setup.sh` first

### Step 4: Launch Game and Test

**Start Mosquitto** (if not auto-running):
```bash
mosquitto -c config/mosquitto.conf
```

**Launch KSA:**
```bash
~/.local/share/Kitten\ Space\ Agency/StarMap
# Or wherever KSA is installed
```

**Verify in game logs:**
```
[KSA-Bridge] Found config at: ...
[KSA-Bridge] Connected to 127.0.0.1:1884
```

### Step 5: Start Web Console

Open a new terminal and run:
```bash
cd examples
python3 -m http.server 8088
```

Then open your browser to: **http://localhost:8088/hard-scifi/hardscifi-fdo-console.html**

---

## Docker (All Platforms)

**One-command setup for reproducible environments** (Windows/Linux/macOS with Docker installed)

### Step 1: Install Docker Desktop
- Download from [docker.com](https://www.docker.com/products/docker-desktop)
- Install and start Docker

### Step 2: Run Everything

```bash
docker-compose up
```

This starts:
- ✓ Mosquitto MQTT broker (port 1884, 9001)
- ✓ Python webserver (port 8088) serving the example consoles
- ✓ Network bridge connecting them

Then:
1. Launch KSA normally
2. Open browser to **http://localhost:8088/hard-scifi/hardscifi-fdo-console.html**
3. Watch live telemetry appear

**To stop:**
```bash
docker-compose down
```

---

## Verification Checklist

Before testing, verify all three components:

```powershell
# PowerShell (Windows)
Write-Host "Checking prerequisites..."
dotnet --version
Get-Process mosquitto -ErrorAction SilentlyContinue | Write-Host "Mosquitto: running" -ForegroundColor Green
Test-Path "$env:USERPROFILE\Documents\My Games\Kitten Space Agency\mods\KSA-Bridge\KSA-Bridge.dll" | Write-Host "Mod DLL: deployed" -ForegroundColor Green
```

```bash
# bash (Linux/macOS)
echo "Checking prerequisites..."
dotnet --version
pgrep mosquitto && echo "Mosquitto: running" || echo "Mosquitto: NOT running"
ls ~/.local/share/"Kitten Space Agency"/mods/KSA-Bridge/KSA-Bridge.dll 2>/dev/null && echo "Mod DLL: deployed" || echo "Mod DLL: NOT deployed"
```

All three should show green/success.

---

## Common Issues & Solutions

### "Port 1884 already in use"
Another application is using port 1884. Either:
- Kill the existing process: `taskkill /IM mosquitto.exe /F` (Windows)
- Change the port in `config/mosquitto.conf` (advanced)
- Stop Docker: `docker-compose down`

### "Game loads but no telemetry appears"
1. Verify Mosquitto is running (see checklist above)
2. Check game log: `[KSA-Bridge] Connected to...` should appear
3. Try reconnecting: in-game, press **Ctrl+B** and click **[Reload Config]**
4. Check firewall: Windows Defender may block Mosquitto on first run

### "Mod can't connect: NotAuthorized error"
**Error in game logs:** `[KSA-Bridge] Connect failed: NotAuthorized`

This happens when Mosquitto requires authentication. The KSA-Bridge repository includes a config that enables anonymous access (safe for local dev). Verify:

1. Check `config/mosquitto.conf` contains:
   ```
   listener 1884
   protocol mqtt
   allow_anonymous true
   ```

2. If not, add those lines and restart Mosquitto using `scripts/restart-mosquitto.bat`

3. For production/networked setups, see "Using Mosquitto with Authentication" in the docs (future)

### "Python webserver won't start"
```bash
# Port 8088 may be in use. Try:
python -m http.server 8089  # Use port 8089 instead
# Then visit http://localhost:8089/hard-scifi/...
```

### "Game doesn't find the mod"
Verify the DLL is in the correct location:
- **Windows**: `Documents\My Games\Kitten Space Agency\mods\KSA-Bridge\KSA-Bridge.dll`
- **Linux**: `~/.local/share/Kitten Space Agency/mods/KSA-Bridge/KSA-Bridge.dll`
- **macOS**: `~/Library/Application Support/Kitten Space Agency/mods/KSA-Bridge/KSA-Bridge.dll`

Run `setup.bat` or `setup.sh` again to re-deploy.

---

## Next Steps

Once telemetry is flowing:

1. **Explore the consoles** in `examples/`:
   - Hard Sci-Fi FDO (3D orbit visualization)
   - Apollo Mission Control (1960s-style terminal)

2. **Build your own console** using the MQTT topics list in `README.md`

3. **Join the community**: Questions? Share your data visualizations? Post in the KSA community forums.

---

## Need Help?

Check these in order:
1. **This guide** → Most common issues are here
2. **Game logs** → `Documents\My Games\Kitten Space Agency\logs\`
3. **MQTT diagnostics** → Use an MQTT client like [MQTT Explorer](http://mqtt-explorer.com/) to see what's being published
4. **KSA forums** → The community is helpful

---

**Happy flying! 🚀**
