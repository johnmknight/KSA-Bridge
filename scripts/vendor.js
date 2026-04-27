#!/usr/bin/env node
//
// vendor.js  -  refresh the minified libs vendored into examples/*/lib/ from
//               the freshly-installed copies in node_modules/.
//
// This script is the canonical refresh path for the JS libraries the example
// consoles load locally. The runtime consoles (hardscifi-fdo-console.html,
// apollo-fdo-console.html, etc.) load `lib/mqtt.min.js`, `lib/topojson-
// client.min.js`, and `lib/three.min.js` -- NOT from node_modules. node_modules
// exists only as the source-of-truth for vendoring.
//
// Workflow:
//   1. Bump a version in package.json (e.g. mqtt: ^5.x.x -> mqtt: ^6.0.0)
//   2. Run `npm install` -- updates node_modules + package-lock.json
//   3. Run `npm run vendor` -- copies node_modules/<pkg>/dist/<file>.min.js
//      into both examples/<console>/lib/ folders
//   4. Verify the consoles still work in a browser (load + connect to MQTT)
//   5. Update the version banner in examples/*/lib/LICENSES.md if the major
//      version changed
//   6. Commit package.json, package-lock.json, both vendored *.min.js files,
//      and both LICENSES.md changes together
//
// Cross-platform: pure Node, no shell-specific commands. Works on Windows,
// macOS, Linux.
//
// Note about three.min.js: Three.js is ALSO vendored under examples/*/lib/
// but is NOT currently a package.json dependency. It was downloaded once
// (r128) and is maintained manually. If you decide to put it under npm,
// add the appropriate VENDOR_TARGETS entry below.

'use strict';

const fs   = require('fs');
const path = require('path');

// Where this script lives, so we can resolve repo root regardless of cwd.
const SCRIPT_DIR = __dirname;
const REPO_ROOT  = path.resolve(SCRIPT_DIR, '..');

// Each entry: source path inside node_modules, list of vendor destinations.
// All destinations are relative to REPO_ROOT.
const VENDOR_TARGETS = [
    {
        pkg: 'mqtt',
        src: 'node_modules/mqtt/dist/mqtt.min.js',
        dests: [
            'examples/hard-scifi/lib/mqtt.min.js',
            'examples/apollo-mission-control/lib/mqtt.min.js',
        ],
    },
    {
        pkg: 'topojson-client',
        src: 'node_modules/topojson-client/dist/topojson-client.min.js',
        dests: [
            'examples/hard-scifi/lib/topojson-client.min.js',
            'examples/apollo-mission-control/lib/topojson-client.min.js',
        ],
    },
];

function readVersion(pkgName) {
    const pkgJson = path.join(REPO_ROOT, 'node_modules', pkgName, 'package.json');
    if (!fs.existsSync(pkgJson)) return null;
    try {
        return JSON.parse(fs.readFileSync(pkgJson, 'utf8')).version;
    } catch {
        return null;
    }
}

function fileSize(p) {
    try { return fs.statSync(p).size; } catch { return null; }
}

function copyOne(srcRel, destRel) {
    const src  = path.join(REPO_ROOT, srcRel);
    const dest = path.join(REPO_ROOT, destRel);
    if (!fs.existsSync(src)) {
        return { ok: false, reason: `source missing: ${srcRel}` };
    }
    fs.mkdirSync(path.dirname(dest), { recursive: true });
    fs.copyFileSync(src, dest);
    return { ok: true, srcSize: fileSize(src), destSize: fileSize(dest) };
}

function main() {
    console.log('=== KSA-Bridge vendor refresh ===');
    console.log(`repo root: ${REPO_ROOT}`);
    console.log('');

    let totalCopied = 0;
    let anyMissing  = false;

    for (const t of VENDOR_TARGETS) {
        const ver = readVersion(t.pkg);
        if (!ver) {
            console.error(`!! ${t.pkg}: not installed (run \`npm install\` first)`);
            anyMissing = true;
            continue;
        }
        console.log(`-- ${t.pkg} v${ver}`);
        console.log(`   src: ${t.src}`);
        for (const dest of t.dests) {
            const r = copyOne(t.src, dest);
            if (r.ok) {
                console.log(`   --> ${dest}  (${r.srcSize} bytes)`);
                totalCopied++;
            } else {
                console.error(`   !! ${r.reason}`);
                anyMissing = true;
            }
        }
        console.log('');
    }

    console.log('===================================');
    console.log(`copied ${totalCopied} files`);
    if (anyMissing) {
        console.error('one or more sources missing - run `npm install` and retry');
        process.exit(1);
    }

    console.log('');
    console.log('Reminder: if any major version above changed, update');
    console.log('  examples/hard-scifi/lib/LICENSES.md');
    console.log('  examples/apollo-mission-control/lib/LICENSES.md');
    console.log('to match before committing.');
}

main();
