# KSA-Bridge — Changelog

All notable changes to this project. Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) conventions; versions follow [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.2.1] — 2026-08-14

Compatibility pass for the August 2026 game builds.

### Compatibility

- **KSA:** version 2026.8.19.5261 — mod loads, heartbeats, and unloads cleanly; in-flight
  telemetry verification on this build still pending (only menu-level run so far).
- **StarMap loader:** 0.4.6 — the launcher/loader split is gone; a single `StarMap.exe`
  now lives in `C:\Program Files\StarMap\`. The attribute-based mod API is unchanged, so
  no entry-point changes were needed. NOTE: `scripts\launch-starmap.bat` references the
  removed `StarMap.Loader.exe` and needs updating.

### Changed

- **KSA 2026.8.x API migration** in `TelemetryPublisher.cs` (replacements verified by
  reflecting over the new `KSA.dll`, not guessed):
  - `NavBallData.DeltaVInVacuum` → `NavBallData.DeltaV`
  - `Vehicle.LastKinematicStates.Situation` → `Vehicle.Situation`
  - atmosphere topic now reads `Vehicle.PhysicsEnvironment` (density/pressure/ocean/terrain)
    and `Vehicle.Props` (surface area, volume, draft) — the old `LastKinematicStates`
    struct was split by the game.
  - **BREAKING (topic schema):** `ksa/telemetry/dynamics` field `propellantMassFlowRate`
    replaced by `propellantMass` (total kg aboard). The game removed the vehicle-level
    flow-rate aggregate (now per-nozzle on `ActiveNozzle`); no known consumer used the
    old field.

## [0.2.0] — 2026-04-27

First substantive release after the initial 0.1.0 scaffolding. Targets KSA r4184 (game version 2026.4.17.4184) and StarMap 0.4.5.

### Compatibility

- **KSA:** version 2026.4.17.4184 (r4184) — verified telemetry end-to-end on this build.
- **StarMap loader:** 0.4.5 — required for r4184. Earlier StarMap 0.4.x versions hit a Harmony patching exception (`Undefined target method for patch method ProgramPatcher::BeforeOnDrawUi`) against r4184.
- **.NET SDK:** 10.0.x.
- **Mosquitto MQTT broker:** 2.x with WebSocket support.

### Added

- **Apollo Mission Control FDO console** at `examples/apollo-mission-control/apollo-fdo-console.html`. Phosphor-green terminal aesthetic inspired by 1960s NASA mission control. Subscribes to `ksa/telemetry/#`, renders an event log and live state panels.
- **Saturn surface data** — `saturn_bands.geojson` (atmospheric band boundaries), `saturn_rings.json` (D, C, B, A, F, G ring inner/outer radii in Saturn radii, with Cassini and Encke gaps), `saturn_labels.json`. Renders accurate ringed Saturn with cloud bands when the active vehicle is in Saturn's SOI.
- **Mercury surface data** — crater rings and named features as TopoJSON (`mercury_craters.topojson`, `mercury_labels.json`).
- **Venus surface data** — coronae, regiones, dorsa, named impact features as TopoJSON (`venus_craters.topojson`, `venus_labels.json`).
- **Moon surface data** — mare boundaries and craters (`moon_mare.topojson`, `moon_craters.topojson`, `moon_labels.json`).
- **Mars landmarks** — `mars_landmarks.geojson` complementing the existing USGS SIM 3292 contacts data.
- **Setup automation** — `setup.bat` step `[6/6]` deploys `scripts\launch-starmap.bat` to `C:\Program Files\StarMap\` so the canonical run location stays in sync with the repo.
- **`scripts/vendor.js`** — cross-platform Node.js vendoring helper. `npm run vendor` copies `mqtt.min.js` and `topojson-client.min.js` from `node_modules/` into both consoles' `lib/` directories with version logging.
- **`CONTRIBUTING.md`** — minimum contributor guide covering build/run, comment style, vendoring workflow, and where to file issues.
- **`CHANGELOG.md`** — this file.
- **README "Related Projects / Ecosystem" section** with KittenRemoteControl (upstream, REST), KSA-PAO (downstream consumer), EDMC-Telemetry (lineage / prior art), and an ASCII ecosystem diagram.

### Changed

- **Hard Sci-Fi FDO console — CDN variant is now the recommended default.** `hardscifi-fdo-console-cdn.html` (multi-body, runtime per-body data fetches) is the variant the docs point at first. The companion `hardscifi-fdo-console.html` is now explicitly labeled as the Earth-only fully-offline single-file demo / learning-reference variant. Both ship; you pick based on use case. Fix for the bug where users on Mars, Saturn, etc. saw an Earth globe — the embed variant is Earth-only by design (its `embed_coastlines.js` build step pre-extracts only Earth coastlines), and the CDN variant is the one that switches surface data per `parent_body` telemetry. README, SETUP.md, INSTALLATION.md, docs/CONSOLES.md, samples/README.md, and LICENSING.md all updated accordingly.

- **`README.md`** — added Saturn row to surface data table, sharpened the Venus row to reflect actual feature data, added Saturn rings/bands paragraph alongside Jupiter, replaced broken Related Projects link with the full ecosystem section. Added clear "Hard Sci-Fi FDO Console (recommended)" section pointing at the CDN variant, plus an "Earth-only embed variant" subsection explaining when to use which.
- **`SETUP.md`** — full Day-to-Day Workflow section with canonical script reference table, typical iterative session walkthrough, "where things live" reference; explicit "service vs manual" Mosquitto conflict documentation; explicit note that the launcher must be invoked from the StarMap install dir and must call `StarMap.Loader.exe` (not `StarMap.exe`, which is a WIP stub in 0.4.5+).
- **`INSTALLATION.md`** — recommended path now points to `setup.bat` / `build-and-deploy.bat`; manual install path retained as advanced fallback. Mosquitto section points to the project's existing `config/mosquitto.conf` rather than instructing users to write a new one.
- **`scripts/launch-starmap.bat`** — now invokes `StarMap.Loader.exe` instead of `StarMap.exe`. As of StarMap 0.4.5, the new `StarMap.exe` is a WIP "Launcher" stub that prints `Currently WIP, please use the standalone version or launch 'StarMap.Loader.exe'` and exits. Header comment expanded with REFERENCE COPY ONLY warning.
- **`.gitignore`** — added voice models (`*.onnx`, `voices/`, `models/`), OS metadata (`.DS_Store`, `Thumbs.db`, `desktop.ini`), env/secrets (`.env`, `.env.*`, `*.secret`), and AI assistant scratch patterns (`_dc_*`, `__cleanup*`). Removed `package-lock.json` from ignore list — lockfile is now committed for reproducible installs (standard JS practice).
- **`package.json`** — added `name`, `private: true`, `description`, `scripts.vendor`. Pinned `mqtt` to exact `4.3.7` to match the actually-vendored bundle (was `^5.15.1`, which would have caused the next `npm run vendor` to overwrite the working 4.x bundle with an untested 5.x build).
- **`examples/*/lib/LICENSES.md`** — added 6-step vendoring workflow note at the top so contributors don't edit the minified bundles directly.
- **`scripts/data-gen/`** — `convert_mars.py` and `generate_jupiter.py` moved here from repo root to match what README and DATA_SOURCES.md already documented.
- **`Bridge.cs`** — eliminated CS8625 nullable warning by refactoring config-path search to use a local nullable variable, with the field staying non-nullable.
- **`mod.toml`** — version bumped from `0.1.0` to `0.2.0`.

### Fixed

- **Hard Sci-Fi CDN console: Luna upgraded to USGS Unified Geologic Map (Fortezzo / Spudis / Harrel, 2020).** Replaces the earlier `moon_mare.topojson` MultiPolygon + `moon_craters.topojson` polygons with the seamless 1:5M-scale lunar geologic-unit contact lines from the USGS Astropedia v2 GIS bundle — the same data class as Mars's USGS SIM 3292 contacts, just for the Moon. New `scripts/data-gen/convert_moon.py` mirrors the Mars pipeline: read the source shapefile, reproject from `Moon2000_EquidistantCylindrical_clon0` (meters) to `+proj=longlat +R=1737400` (lat/lon on the lunar sphere), drop the source's `DND` and `Map boundary` rows, filter to contacts at least 150 km long, simplify at 0.5° tolerance, rename `ContactTyp → ConType` for parity with Mars, and emit `moon_geologic.geojson` (~925 KB, 2,877 features). Console gains a new `moonContactMats` material set keyed by ConType (Certain / Approx / Internal / Buried / Inferred) in cool tones, parallel to the warm-toned `marsContactMats` — the renderer's existing classify-and-color path picks the right palette by `bodyName`. The IAU labels overlay (top 15 named features as marker dots + text sprites) is preserved on top of the contacts so iconic landmarks (Mare Imbrium, Tranquillitatis, Apollo, Korolev, Hertzsprung) are named. New theme entries `moonCertain`/`moonApprox`/`moonInternal`/`moonBuried`/`moonInferred` in dark and light scenes. Mars unchanged. Removed the now-obsolete `lunaMats` scaffolding from prior iterations. `DATA_SOURCES.md` now carries full UGM 2020 attribution + regeneration instructions.
- **Hard Sci-Fi CDN console: Luna body name mismatch.** KSA reports Earth's moon as `"Luna"` in the `parent_body` telemetry payload, but the console's `bodyDataConfig` and `bodyAtmoColors` dictionaries were keyed by `"Moon"`. The case-sensitive direct lookup returned `undefined`, the warning fired silently in DevTools (`[FDO] No surface data configured for body: Luna`), and the previously-loaded body's surface data stayed rendered — visible as "Earth globe at the moon" while the BODY header correctly read `Luna`. Renamed both dictionary keys from `'Moon'` to `'Luna'` to match what the bridge actually emits. Data files keep their `moon_*` names (USGS/IAU English convention, honest attribution to the data source). Same pattern applies to any future body whose KSA name diverges from the file-naming convention — fix at the dict-key level, not via a translation layer.
- **CS8625 nullable warning** in `Bridge.cs:81` (`Cannot convert null literal to non-nullable reference type`). Build now produces 0 warnings, 0 errors.
- **Broken cross-doc link** — `README.md`'s "Related Projects" section formerly pointed at non-existent `docs/KSA-PAO-Design-v0.2.md`. Replaced with the in-repo VISION.md lineage discussion.
- **Broken screenshot links** in `docs/CONSOLES.md` — pointed at `/docs/screenshots/apollo-console.png` and `/docs/screenshots/hardscifi-console.png`, neither of which existed and both used absolute filesystem paths. Replaced with HTML comments noting the live-view URL until screenshots are captured.
- **Stale `LICENSING.md`** — removed false D3 attribution (D3 is not actually a project dependency); fixed citation block (version `0.2.0`, real GitHub URL).
- **Stale `TODO.md`** — full rewrite to match real repo state. Old TODO claimed roughly 25 phantom session-status documents and 20 stray Python scripts to delete; in reality only 2 Python scripts existed at root and were moved into `scripts/data-gen/`.
- **Mosquitto Windows-service vs manual-launch conflict** — documented in SETUP.md and the project hub. Mosquitto installed via the Windows installer's "Install as Windows Service" option auto-starts on default port 1883 with the default config; KSA-Bridge requires ports 1884 (MQTT) and 9001 (WebSocket) from `config/mosquitto.conf`. Three resolution paths documented: stop+disable service, run side-by-side, or repoint service binPath at the KSA-Bridge config.

### Telemetry topics — unchanged

The 13-topic publish layout (`ksa/telemetry/{vehicle, orbit, state_vectors, attitude, navigation, dynamics, resources, performance, situation, atmosphere, maneuver, encounter, parent_body}` plus `ksa/bridge/status`) is **stable** between 0.1.0 and 0.2.0. Existing consumers continue to work without changes.

### Known issues

- **`StarMap.exe` (the new 0.4.5 Launcher) is WIP** — running it directly prints a "use Loader.exe" message and exits. The `launch-starmap.bat` we ship invokes `StarMap.Loader.exe` to avoid this. Revisit if a future StarMap update makes `StarMap.exe` functional or removes `StarMap.Loader.exe`.
- **DATA_SOURCES.md** lists Moon, Mercury, Venus, and Saturn under "outline documentation only (data TBD)" sections, but the data is actually present in the repo. Best-effort attribution rewrite is part of this release; corrections welcome via PR.

## [0.1.0] — Earlier 2026

Initial scaffolding. C# StarMap mod publishing 13 telemetry topics over MQTT, hard-sci-fi FDO console with Three.js globe and Mars/Jupiter/Earth surface data, MIT license, basic README/SETUP/INSTALLATION/VISION docs.

[0.2.0]: https://github.com/johnmknight/KSA-Bridge/releases/tag/0.2.0
[0.1.0]: https://github.com/johnmknight/KSA-Bridge/releases/tag/0.1.0
