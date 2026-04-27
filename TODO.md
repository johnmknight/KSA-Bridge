# KSA-Bridge — TODO

_Last refresh: 2026-04-27. The previous version of this file described a much
larger pile of cleanup work than actually existed in the repo; this rewrite
reflects the real state of the tree._

## Recently Completed (April 2026)

- [x] Repo cleanup pass — moved `convert_mars.py` and `generate_jupiter.py`
      from root into `scripts/data-gen/` to match documented paths.
- [x] README — added Saturn to surface data table; tightened Venus row
      to reflect actual feature data (coronae, regiones, dorsa).
- [x] README — added Saturn rings/bands paragraph alongside Jupiter section.
- [x] README — expanded Related Projects section with KittenRemoteControl
      (upstream), KSA-PAO (downstream), EDMC-Telemetry (lineage), and an
      ecosystem diagram. Removed broken link to non-existent
      `docs/KSA-PAO-Design-v0.2.md`.
- [x] `.gitignore` — added voice models (`*.onnx`, `voices/`, `models/`),
      OS metadata (`.DS_Store`, `Thumbs.db`, `desktop.ini`), and
      environment/secret patterns.
- [x] Pre-cleanup full-mirror backup taken at
      `C:\Users\john_\dev\KSA-Bridge-backup-2026-04-27`.
- [x] **End-to-end test verified (2026-04-27)** — full stack: built mod,
      deployed DLL, ran mosquitto on 1884/9001 with project config, served
      examples on 8088, launched StarMap, telemetry confirmed flowing on
      the Hard Sci-Fi FDO console.
- [x] **Documented launcher gotcha**: `launch-starmap.bat` must be run
      from `C:\Program Files\StarMap\` (StarMap.exe inherits its CWD from
      the launcher and needs CWD = install dir for mods to load). Running
      the repo copy from the repo directory silently breaks mod loading.
      `setup.bat` now deploys the repo's launcher to `C:\Program Files\StarMap\`
      at install time. SETUP.md, INSTALLATION.md, and the project hub skill
      all updated to reflect this.
- [x] **Documented Mosquitto service-vs-manual conflict** — Mosquitto
      installed as a Windows service runs default config on port 1883;
      KSA-Bridge needs 1884 + 9001 from `config/mosquitto.conf`. SETUP.md
      now includes the service-disable / side-by-side / repoint resolution
      paths.
- [x] **Documented day-to-day workflow** — SETUP.md now has a canonical
      script reference table (`build-and-deploy.bat`, `restart-mosquitto.bat`,
      `serve-examples.bat`, deployed `launch-starmap.bat`) with a typical
      iterative-session walkthrough and a "where things live" reference.

## Current Blockers

(none open as of this update)

## Pre-release follow-ups (decide before tagging v0.2.0)

(none open as of this update)

## Resolved pre-release items

- **(Resolved 2026-04-27) MQTT.js version drift.** `package.json` had
  declared `mqtt: ^5.15.1` (5.x) while the vendored bundles at
  `examples/hard-scifi/lib/mqtt.min.js` and
  `examples/apollo-mission-control/lib/mqtt.min.js` were MQTT.js 4.3.7
  (SHA-1 `25547f1cb6a71d373edc632870e59cf7c1da4bdb`, byte-for-byte
  match against `https://unpkg.com/mqtt@4.3.7/dist/mqtt.min.js`).
  Resolution: pinned `package.json` to exact `"mqtt": "4.3.7"`. Ran
  `npm install` to regenerate `package-lock.json` and `node_modules`
  against the new constraint (added 18, removed 20, changed 10 transitive
  deps; 0 vulnerabilities). Verified all four sources of truth now agree:
  package.json, lockfile, node_modules/mqtt, and the vendored bundles are
  all 4.3.7 — `npm run vendor` is now a safe no-op refresh. A future
  deliberate 5.x bump can be its own scoped change: bump constraint, run
  `npm install && npm run vendor`, port any console code that hits the
  4.x→5.x API delta, verify telemetry flows, commit all four together.

## Resolved Blockers

- **(Resolved 2026-04-27) StarMap 0.4.3 incompatible with KSA r4184.**
  Original symptom: `HarmonyLib.HarmonyException: Undefined target method
  for patch method static System.Void
  StarMap.Core.Patches.ProgramPatcher::BeforeOnDrawUi(System.Double dt)`.
  StarMap 0.4.4 (2026-04-09) explicitly fixed this with the release note
  "Fix OnDrawUi harmony patch for KSA 2026.4.6.4036 (#71)". Upgraded to
  StarMap 0.4.5 (the latest, 2026-04-10) via the elevated
  `_dc_install_starmap_045.ps1` flow: backup -> download -> extract ->
  overlay -> redeploy `launch-starmap.bat` from repo -> verify versions.
  Backup retained at `C:\Program Files\StarMap.0.4.3.bak`.
- **(Resolved 2026-04-27) StarMap 0.4.5 Launcher stub crash.** After the
  upgrade, launching `C:\Program Files\StarMap\StarMap.exe` produced a
  window that disappeared instantly. Captured stdout revealed:
  `Currently WIP, please use the standalone version or launch
  'StarMap.Loader.exe'`. In 0.4.5 the zip ships a separate
  `StarMap.Launcher.dll` + `StarMap.Loader.exe` architecture; the new
  `StarMap.exe` is a WIP stub. Updated `scripts/launch-starmap.bat` to
  call `StarMap.Loader.exe` directly. Updated SETUP.md, setup.bat, and
  the project hub skill to reflect the new entry point.

## Known Quirks (for future sessions)

- **`build-and-deploy.bat` does NOT run from Desktop Commander.** The bat
  uses bare `dotnet build`, and cmd.exe spawned by Desktop Commander does
  not inherit `dotnet` on PATH (even with explicit `set PATH=` augmentation
  via PowerShell or inline cmd). Build fails with `'dotnet' is not
  recognized`. Run it from John's interactive terminal instead, or call
  `dotnet build` directly via PowerShell `ProcessStartInfo` (which uses
  Windows App Paths registry resolution and finds dotnet correctly).
  This is a Desktop Commander environment quirk, not a script bug.
- **The bat's `pause` calls** also block unattended runs from any
  non-interactive shell. Auto-ack via stdin works for the pause but
  doesn't help with the PATH issue above.

## This Release

### Repository state — uncommitted work review (priority)

When the cleanup pass ran `git status` on 2026-04-27, a substantial pile
of pre-existing uncommitted work was already in the tree, separate from
anything the cleanup touched. Worth a dedicated commit pass before any
public push. Specifically:

**Modified but uncommitted (intentional changes that haven't been staged):**
- `KSA-Bridge/Bridge.cs`
- `KSA-Bridge/Config.cs`
- `KSA-Bridge/KSA-Bridge.csproj`
- `KSA-Bridge/Publisher.cs`
- `config/mosquitto.conf`
- `mosquitto.conf`
- `examples/hard-scifi/hardscifi-fdo-console-cdn.html`

**Deleted but not committed:**
- `docs/ksa_ui_style_guide_v4 (1).docx`

**Untracked — never added to git but referenced by README/docs:**
- `INSTALLATION.md`, `VISION.md`, `package.json`
- `docs/diagrams/` (referenced by VISION.md for architecture diagrams)
- `examples/apollo-mission-control/README.md`,
  `examples/apollo-mission-control/lib/`
- `examples/hard-scifi/data/mars_landmarks.geojson`,
  `saturn_bands.geojson`, `saturn_labels.json`, `saturn_rings.json`
- The entire `scripts/` directory — the README explicitly tells readers
  to run scripts inside it, but it has never been committed.

- [ ] **Commit pass** — review each of the above, decide intent
      (commit, discard, .gitignore, or leave staged), and either commit
      with descriptive messages or document why each is being held back.

### Documentation hygiene
- [ ] **Update `DATA_SOURCES.md`** — currently lists Moon, Mercury, Venus,
      and Saturn as "Outline documentation only (data TBD)" but data files
      exist for all of them in `examples/hard-scifi/data/`. Replace the
      "TBD" stubs with real source attributions. Add Saturn section
      mirroring the Jupiter pattern.
- [ ] Decide what to do with empty `scripts/deploy/`, `scripts/inspect/`,
      `scripts/test/` directories. Either populate them, remove them,
      or add `.gitkeep` placeholders with a one-line README explaining
      what each is for.
- [ ] Decide on `package-lock.json` — it is currently ignored
      (`.gitignore` line 18) but exists at repo root. Most JS projects
      commit the lockfile for reproducible installs. Either remove the
      ignore line and commit the lockfile, or leave it ignored and
      delete the working-tree copy to keep the state consistent.

### Reach goals before public sharing
- [ ] **Telemetry recording and replay tool** — record an MQTT session
      to a file, replay it to a broker. Enables development without the
      game running. Critical for lowering the barrier to entry — anyone
      can build a console without owning KSA. Consider Python CLI in
      `scripts/replay/` plus a documented JSONL on-disk format.
- [ ] **Getting-started guide** — minimal tutorial: install Mosquitto,
      build the mod, subscribe with five lines of Python, see live
      altitude print to console. Aimed at someone who has never used
      MQTT. Should pair naturally with the recorder above so readers
      don't need to launch the game.

## Next Release

- [ ] **KSA.dll decompilation pass** — validate that trigger events
      needed by downstream consumers (dynamic pressure, stage number,
      EVA state, parachute state, surface contact) are reachable in the
      game API and can be published by the bridge. Document findings
      in `docs/`.
- [ ] **Coordinate system guide** — standalone doc explaining
      CCI Z-up → Three.js Y-up conversion for consumer developers.
      The README has the rules; this would be the long-form explanation
      with diagrams.
- [ ] **Saturn surface data refinement** — the current
      `saturn_bands.geojson` is a synthetic latitude-band generation;
      future passes can pull real Voyager/Cassini band boundaries
      following the Jupiter pattern with proper attribution.

## Ongoing

- [ ] Keep README current with each release — new topics, new consoles,
      new surface data.
- [ ] Keep VISION.md aligned with project direction — review annually
      or on major scope changes.
- [ ] Document any new MQTT topics, payload changes, or rate changes.
- [ ] Sync TODO.md after each cleanup or feature pass so this file
      stops drifting from reality (the failure mode this rewrite was
      correcting).
