# Contributing to KSA-Bridge

Thanks for your interest. KSA-Bridge is a small, deliberately narrow project — a "dumb pipe" between Kitten Space Agency and an MQTT broker — and it's intended to stay that way. Most contributions land naturally in one of three areas: the C# mod itself, the example consoles, or the surface-data sets. This guide covers each.

## Getting up and running

Everything you need to build, run, and test the mod is in [`SETUP.md`](SETUP.md). Read it once end-to-end. Two things that bite people if they skip it:

1. **Mosquitto-as-Windows-Service vs manual launch.** The Mosquitto installer's "Install as Windows Service" option auto-starts the broker on port 1883 with the default config — which is *not* what KSA-Bridge needs (it wants 1884 + 9001 from `config/mosquitto.conf`). SETUP.md walks the three resolution paths.
2. **`launch-starmap.bat` must run from `C:\Program Files\StarMap\`**, and as of StarMap 0.4.5 it must invoke `StarMap.Loader.exe` (not `StarMap.exe`, which is a WIP launcher stub). `setup.bat` deploys the corrected launcher there for you.

For the day-to-day iterative workflow once first install is done, see [SETUP.md → Day-to-Day Workflow](SETUP.md#day-to-day-workflow-after-first-install).

## Code style

**Comments are documentation.** This project is also a learning resource for people coming to MQTT, modding, or 3D web visualization. Write verbose, educational comments — explain the *why*, not just the *what*. A comment that walks a first-time reader through a tricky bit of orbital mechanics or coordinate-system conversion is more valuable than terseness.

**C# (the mod):**
- .NET 10 SDK, nullable reference types enabled.
- Match the existing brace and field-naming style in `Bridge.cs` / `Publisher.cs`.
- Build clean: `dotnet build --configuration Release` should produce **0 errors and 0 warnings**.

**JavaScript (the consoles):**
- Vanilla JS, no build step, no React. Consoles are static HTML + script tags.
- The runtime libraries (`mqtt.min.js`, `topojson-client.min.js`, `three.min.js`) are vendored copies under `examples/<console>/lib/`. Do **not** edit them — see the vendoring workflow below.

**Python (the data-gen scripts):**
- Lives in `scripts/data-gen/`.
- Each script should be runnable standalone, document its dependencies inline, and write output into `examples/hard-scifi/data/`.

## Updating vendored JS libraries

The example consoles load minified bundles from `examples/<console>/lib/`, not from `node_modules/`. To bump a library:

```sh
# 1. Bump version constraint
$EDITOR package.json   # change e.g. "mqtt": "4.3.7" → "4.3.8"

# 2. Resolve and download
npm install            # updates node_modules/ and package-lock.json

# 3. Refresh the vendored bundles
npm run vendor         # copies node_modules/<pkg>/dist/<pkg>.min.js → both consoles' lib/

# 4. Update LICENSES.md if the major version changed
$EDITOR examples/hard-scifi/lib/LICENSES.md
$EDITOR examples/apollo-mission-control/lib/LICENSES.md

# 5. Verify the consoles still connect to the broker and render telemetry

# 6. Commit it all together: package.json, package-lock.json, the vendored
#    *.min.js files, and any LICENSES.md changes.
```

Note: `three.min.js` (r128) and `land-110m.json` are vendored manually — they are not currently part of the npm dependency set. If you decide to put either under npm, add an entry to `scripts/vendor.js`.

## Adding planetary surface data

If you have data for a body that isn't yet covered (or better data for one that is):

1. **Verify the license** — must be public domain or a permissive license that allows redistribution.
2. **Document the source** — add a section to [`DATA_SOURCES.md`](DATA_SOURCES.md) following the Mars / Jupiter pattern. Include source name, reference URLs, coordinate system, format, and a citation block.
3. **Simplify for web** — convert to GeoJSON or TopoJSON with reasonable file sizes (target under ~1 MB per file; the existing data sets range from a few KB to ~840 KB).
4. **Add a generation script** — drop it into `scripts/data-gen/` so the dataset can be regenerated from the original source. Document Python dependencies in a comment at the top.
5. **Test on a console** — verify the geometry renders correctly on the 3D globe under different parent-body contexts.

## Filing issues

- Bugs in the mod (build errors, telemetry not flowing, crashes): include OS, .NET SDK version, KSA build, StarMap version, and the relevant lines from `Documents\My Games\Kitten Space Agency\logs\`.
- Bugs in a console: include browser, what URL you opened, and any relevant `Console` panel output.
- Feature requests: scope-creep is real for a "dumb pipe" project — see [`VISION.md`](VISION.md) for the boundary. Things on the right side of that boundary (more telemetry topics, better consoles, more surface data, getting-started materials) are welcome. Things on the wrong side (the mod making decisions about the data, embedded UIs, opinionated downstream behavior) live in companion projects (KSA-PAO, etc.), not here.

## Pull requests

- Small, focused PRs are easier to review and merge.
- Include a one-line summary that would make sense in `CHANGELOG.md`.
- If you change MQTT topic structure, payload shape, or rates, document it explicitly — telemetry topics are a public contract.
- If you add a dependency (NuGet, npm, pip), update [`LICENSING.md`](LICENSING.md) with the license + compatibility note.

## Boring legal

The project is MIT licensed; by submitting a PR you're agreeing your contribution is also offered under MIT. See [`LICENSE`](LICENSE) and [`LICENSING.md`](LICENSING.md) for full text and dependency licensing.
