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

Mosquitto is a lightweight message broker. KSA-Bridge needs it running in the background, listening on **port 1884 (MQTT)** and **port 9001 (WebSocket)**.

**Option A: Installer (recommended for beginners)**
1. Download from [mosquitto.org/download](https://mosquitto.org/download/)
2. Choose "Windows" → download the `.exe` installer
3. Run installer, accept defaults (installs to `C:\Program Files\Mosquitto\`)
4. **Skip "Install as Windows Service"** if the option appears, OR install it but be aware of the service-vs-manual conflict described below.

**Option B: Quick check (already installed?)**
Open PowerShell:
```powershell
Get-Process mosquitto -ErrorAction SilentlyContinue
```
If you see `mosquitto` in the output, it's already running. Continue to the service note below before Step 3.

#### Important: Service-vs-manual conflict

If you installed Mosquitto with the "Install as Windows Service" option (or via Chocolatey, which does this automatically), Mosquitto will already be running as an auto-start service using its **default config**. The default config has no listener directives, so it falls back to **port 1883** — not the 1884 / 9001 that KSA-Bridge needs.

You'll see this as: `setup.bat` reports "Mosquitto is running" (true), but the broker still isn't on 1884 (also true), and KSA-Bridge can't connect.

**Fix (choose one):**

- **Easiest — disable the service, run manually:** open an **elevated** PowerShell and run:
  ```powershell
  Stop-Service mosquitto
  Set-Service mosquitto -StartupType Manual
  ```
  Then `setup.bat` and `scripts\restart-mosquitto.bat` will work as designed (they start mosquitto manually with `-c config\mosquitto.conf`, which sets up 1884 + 9001).

- **Alternative — let both run side by side:** start a second mosquitto manually with the KSA-Bridge config. The service stays on 1883 (harmless background), and the manual instance handles 1884 / 9001 for KSA-Bridge. Use `scripts\restart-mosquitto.bat` to (re)start the manual instance whenever needed.

- **Persistent — point the service at the KSA-Bridge config:** edit the service binPath to include `-c "C:\path\to\KSA-Bridge\config\mosquitto.conf"`. Service auto-starts on the right ports going forward, but you lose the default 1883 listener.

The first approach is cleanest for development. The second is fine if you have other things on 1883 you don't want to disturb.

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

**Launch KSA — important:** the launcher MUST be run from `C:\Program Files\StarMap\`, not from a copy elsewhere. `StarMap.Loader.exe` inherits its working directory from the launcher, and it expects that working directory to be its own install dir for mod loading and other relative-path operations to resolve correctly. If you run the launcher from another folder (or double-click the repo's `scripts\launch-starmap.bat`), StarMap still launches but starts in the wrong working directory, mods may not load, and **telemetry will not flow even though everything else looks fine**.

The repo ships a copy of `launch-starmap.bat` at `scripts\launch-starmap.bat` for reference, but the **canonical run location** is `C:\Program Files\StarMap\launch-starmap.bat`. The `setup.bat` script deploys the repo copy to that location during install (overwriting any existing one), so the StarMap-dir copy stays in sync with the repo.

Run it from there:
```
C:\Program Files\StarMap\launch-starmap.bat
```

Or double-click that file in Explorer. Either way works because the `start "" "C:\Program Files\StarMap\StarMap.Loader.exe"` line then inherits that directory as its CWD.

> **Note on StarMap 0.4.5+ entry points:** the bat invokes `StarMap.Loader.exe` directly, not `StarMap.exe`. In 0.4.5, `StarMap.exe` is a new "Launcher" stub that prints `Currently WIP, please use the standalone version or launch 'StarMap.Loader.exe'` and exits immediately. The Loader is the actual mod-loading process. If a future StarMap update changes the entry point again, update `scripts/launch-starmap.bat` accordingly and rerun `setup.bat` to redeploy.

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

Then open your browser to: **http://localhost:8088/hard-scifi/hardscifi-fdo-console-cdn.html**

You should see live telemetry from KSA appearing on the console. (This is the CDN variant — multi-body, recommended for actual mission use. The companion `hardscifi-fdo-console.html` is an Earth-only fully-offline single-file demo; see README for when to use which.)

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

Then open your browser to: **http://localhost:8088/hard-scifi/hardscifi-fdo-console-cdn.html**

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
2. Open browser to **http://localhost:8088/hard-scifi/hardscifi-fdo-console-cdn.html**
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

## Day-to-Day Workflow (After First Install)

Once you've completed the one-time install above and verified telemetry is flowing, the project ships a small set of canonical scripts for normal day-to-day use. **Use these scripts directly — don't re-implement their logic in ad-hoc commands.** They encode the working assumptions about paths, ports, working directories, and ordering.

### The canonical scripts

| Script | What it does | When to run |
|--------|--------------|-------------|
| `build-and-deploy.bat` (repo root) | Builds the mod (Debug config), rotates DLL backups (keeps 5 timestamped copies under `mods\KSA-Bridge\backups\`), and deploys to your KSA mods directory. | Every time you change the mod's C# code. |
| `setup.bat` (repo root) | First-time setup: verifies .NET + Mosquitto, builds **Release**, deploys. Use the first time, or after a clean checkout. | Initial install only. |
| `scripts\restart-mosquitto.bat` | `taskkill` the running mosquitto, then `start mosquitto.exe -c config\mosquitto.conf` so the broker comes up on 1884 + 9001 with the project's listeners. | Whenever you've changed `config\mosquitto.conf`, or if the broker has died, or after a reboot if you went with the "run manually" option above. |
| `scripts\serve-examples.bat` | `python -m http.server 8088` from `examples\`, so the FDO consoles are reachable at `http://localhost:8088/...`. | Once per session — leave it running. |
| `C:\Program Files\StarMap\launch-starmap.bat` | Pause + launch `StarMap.Loader.exe` (the actual loader; `StarMap.exe` is a WIP stub in 0.4.5+). **Must be run from the StarMap install dir** so the game inherits the correct working directory for mod loading. The repo's `scripts\launch-starmap.bat` is a reference copy; `setup.bat` deploys it into `C:\Program Files\StarMap\` for you. **Run the deployed copy, not the repo copy.** | Whenever you want the game up. |

### Typical iterative session

```text
1. (already done from initial setup) ─ scripts\restart-mosquitto.bat
2. (already done from initial setup) ─ scripts\serve-examples.bat
3. Edit C# code in KSA-Bridge\
4. build-and-deploy.bat                ← rebuild + deploy with backup
5. "C:\Program Files\StarMap\launch-starmap.bat"   ← run the game (NOT the repo copy)
6. Confirm "[KSA-Bridge] Connected to 127.0.0.1:1884" in the KSA log
7. Open or refresh the FDO console in your browser
8. Make next change → goto 4
```

### Where things live

- **Mod build output:** `KSA-Bridge\bin\Debug\net10.0\` (from `build-and-deploy.bat`) or `bin\Release\net10.0\` (from `setup.bat`)
- **Deployed mod:** `%USERPROFILE%\OneDrive\Documents\My Games\Kitten Space Agency\mods\KSA-Bridge\` on Windows 11 with OneDrive Documents redirection (default), or `%USERPROFILE%\Documents\My Games\Kitten Space Agency\mods\KSA-Bridge\` without OneDrive.
- **Deployed-DLL backups (rotated):** `<mods dir>\KSA-Bridge\backups\<timestamp>\`
- **Mosquitto config used by the project:** `config\mosquitto.conf` (NOT the default `C:\Program Files\Mosquitto\mosquitto.conf`)
- **KSA logs:** `Documents\My Games\Kitten Space Agency\logs\` — tail the most recent file to watch for `[KSA-Bridge]` lines.

### Stopping cleanly

- **Web server:** find the process (port 8088) and stop it: `Get-Process | Where-Object {$_.Path -like '*python*'}` then `Stop-Process -Id <pid>`. Or just close the cmd window if it's foregrounded.
- **Mosquitto (manual):** `taskkill /IM mosquitto.exe /F` (only kills the manual instance if you also stopped the service per the "service-vs-manual" note above; otherwise the service will still be on 1883).
- **KSA:** quit normally from the in-game menu.

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
