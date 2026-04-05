# KSA-Bridge Licensing & Attribution

Complete licensing information for the KSA-Bridge project, all dependencies, and included datasets.

---

## KSA-Bridge Core Project

**License:** MIT (Massachusetts Institute of Technology)  
**Copyright:** (c) 2026 John M. Knight, Florida, USA  
**License File:** `LICENSE` (in repository root)

### What MIT License Allows
✅ **Commercial use** — Use in commercial products  
✅ **Modification** — Modify and extend the code  
✅ **Distribution** — Redistribute under the same or compatible license  
✅ **Private use** — Use privately without disclosure  
✅ **Educational use** — Use in classrooms and educational institutions  

### What MIT License Requires
- Include a copy of the license and copyright notice in any distribution
- No warranty or liability provided by the author

**Summary:** KSA-Bridge is completely free, open-source, and can be used for any purpose including commercial applications.

---

## Software Dependencies

All dependencies are listed in `package.json` and `KSA-Bridge.csproj`. This section documents their licenses.

### .NET / C# Dependencies

#### Direct Dependencies (KSA-Bridge.csproj):
- **MQTTnet** — MIT License
  - Nuget: `MQTTnet` version varies
  - Purpose: MQTT protocol client for broker communication
  - GitHub: https://github.com/dotnet/MQTTnet
  - License: MIT — ✅ Compatible

- **Tomlyn** — MIT License
  - Nuget: `Tomlyn`
  - Purpose: TOML configuration file parsing
  - GitHub: https://github.com/xoofx/Tomlyn
  - License: MIT — ✅ Compatible

### Node.js / JavaScript Dependencies (package.json)

#### Direct Dependencies:
- **mqtt** — MIT License
  - npm package for MQTT client in JavaScript
  - Used by: Example web consoles
  - Purpose: Subscribe to telemetry over WebSocket
  - License: MIT — ✅ Compatible

- **topojson-client** — BSD-3-Clause
  - npm: `topojson-client`
  - Used by: Hard Sci-Fi FDO console for geometry processing
  - Purpose: Convert TopoJSON to GeoJSON
  - License: BSD-3-Clause — ✅ Compatible (permissive)

- **d3** — ISC License
  - npm: `d3`
  - Used by: Data visualization in consoles (optional)
  - License: ISC (permissive, similar to MIT) — ✅ Compatible

### Browser / CDN Dependencies

#### Three.js
- **CDN:** https://cdnjs.cloudflare.com/ajax/libs/three.js/
- **License:** MIT
- **Purpose:** 3D globe rendering in Hard Sci-Fi console
- **GitHub:** https://github.com/mrdoob/three.js
- **Version:** Typically r128 or higher
- ✅ Compatible

#### Mosquitto MQTT Broker
- **License:** EPL 2.0 (Eclipse Public License)
- **Purpose:** MQTT message broker (external service)
- **GitHub:** https://github.com/eclipse/mosquitto
- **Note:** EPL 2.0 is permissive for use; redistribution requires compliance
- ✅ Compatible (used as a service, not bundled)

### Python Data Generation Scripts

#### Dependencies (`scripts/data-gen/`):
- **geopandas** — BSD-3-Clause
  - pip: `geopandas`
  - Used by: `convert_mars.py`
  - Purpose: Geospatial data processing
  - License: BSD-3-Clause — ✅ Compatible

- **fiona** — BSD-3-Clause
  - pip: `fiona`
  - Used by: `convert_mars.py`
  - Purpose: OGR/GDAL Python bindings for shapefiles
  - License: BSD-3-Clause — ✅ Compatible

- **shapely** — BSD-3-Clause
  - pip: `shapely`
  - Used by: `generate_jupiter.py`, `convert_mars.py`
  - Purpose: Geometric operations
  - License: BSD-3-Clause — ✅ Compatible

---

## Planetary Data Licensing

See **[DATA_SOURCES.md](DATA_SOURCES.md)** for complete source documentation.

### Summary Table

| Data | Source | License | Attribution | Commercial Use |
|------|--------|---------|-------------|-----------------|
| **Mars** | USGS SIM 3292 | Public Domain | Recommended | ✅ Yes |
| **Earth** | Natural Earth | Public Domain | Recommended | ✅ Yes |
| **Jupiter** | Rogers (1995) + NASA | Public Domain | Recommended | ✅ Yes |
| **Moon** | USGS/NASA | Public Domain | Recommended | ✅ Yes |
| **Mercury** | USGS/NASA | Public Domain | Recommended | ✅ Yes |
| **Venus** | USGS/NASA | Public Domain | Recommended | ✅ Yes |

**All planetary data is in the public domain.** Full citations and usage details in DATA_SOURCES.md.

---

## Documentation Licensing

All documentation files (.md, .docx, etc.) are covered under the same MIT license as the code.

| File | License | Purpose |
|------|---------|---------|
| README.md | MIT | Project overview and architecture |
| SETUP.md | MIT | User setup guides for all platforms |
| DATA_SOURCES.md | MIT | Planetary data attribution and licensing |
| INSTALLATION.md | MIT | Detailed installation for developers |
| VISION.md | MIT | Project vision and design philosophy |
| docs/ | MIT | UI style guides and reference materials |

---

## Example Consoles

### Hard Sci-Fi FDO Console
- **License:** MIT (same as KSA-Bridge)
- **Location:** `examples/hard-scifi/hardscifi-fdo-console.html`
- **Dependencies:** Three.js (MIT), mqtt.js (MIT), D3 (ISC), TopoJSON (BSD-3-Clause)
- **Data:** Planetary data (public domain) + NASA imagery (public domain)
- ✅ All dependencies compatible; freely distributable

### Apollo Mission Control Console
- **License:** MIT (same as KSA-Bridge)
- **Location:** `examples/apollo-mission-control/apollo-fdo-console.html`
- **Dependencies:** mqtt.js (MIT)
- **Data:** Planetary data (public domain)
- ✅ Freely distributable

---

## License Compatibility Matrix

All dependencies use permissive licenses. Compatibility verified:

```
KSA-Bridge (MIT)
    ├── MQTTnet (MIT) ✅
    ├── Tomlyn (MIT) ✅
    ├── mqtt.js (MIT) ✅
    ├── three.js (MIT) ✅
    ├── topojson-client (BSD-3-Clause) ✅
    ├── d3 (ISC) ✅
    ├── geopandas (BSD-3-Clause) ✅
    ├── fiona (BSD-3-Clause) ✅
    ├── shapely (BSD-3-Clause) ✅
    └── Mosquitto (EPL 2.0) ✅ (used as service)

All licenses are permissive and compatible with MIT.
No GPL or AGPL dependencies that would restrict commercial use.
```

---

## How to Cite KSA-Bridge

### Academic/Research Citation:
```
Knight, J. M. (2026). KSA-Bridge: Real-time MQTT telemetry bridge for 
Kitten Space Agency. Version 0.1.0. Available at: 
https://github.com/[your-org]/KSA-Bridge
```

### BibTeX:
```bibtex
@software{knight_2026_ksabridge,
  author = {Knight, John M.},
  title = {KSA-Bridge: Real-time {MQTT} telemetry bridge for Kitten Space Agency},
  version = {0.1.0},
  year = {2026},
  url = {https://github.com/[your-org]/KSA-Bridge},
  license = {MIT}
}
```

### In Code/Documentation:
```
KSA-Bridge — MIT License (c) 2026 John M. Knight
See LICENSE file for complete text
```

---

## For Educators Using KSA-Bridge

✅ **You are free to:**
- Use KSA-Bridge in classrooms and educational settings
- Modify examples for teaching purposes
- Redistribute to students
- Use with any number of students
- Create derivative works for educational use
- Commercial use (if teaching in a for-profit institution)

✅ **You must:**
- Include or reference the MIT license
- Provide attribution to John M. Knight

✅ **You cannot:**
- Claim authorship of the original work
- Hold the author liable for any issues

**No permission required to use in education.** Just follow the MIT license terms (include license + attribution).

---

## Dependency Update Policy

If a dependency needs updating, ensure:
1. New version maintains compatible license (permissive, not GPL/AGPL)
2. No breaking changes to API
3. Security patches applied promptly
4. Update this LICENSING.md file

---

## Contributing

When contributing code or data:
1. **Code:** Must be compatible with MIT license (don't add GPL dependencies)
2. **Data:** Must be public domain or under a permissive license
3. **Documentation:** Must be under MIT or compatible license

Submit pull requests with clear license information for any new dependencies or data sources.

---

## Questions About Licensing?

- **MIT License FAQ:** https://opensource.org/licenses/MIT
- **Open Source Licenses:** https://opensource.org/licenses
- **USGS Public Domain:** https://www.usgs.gov/faqs/what-public-domain
- **NASA Public Domain:** https://www.nasa.gov/about/highlights/HP_Privacy.html

---

## License Compliance Summary

| Component | License | Status |
|-----------|---------|--------|
| KSA-Bridge Core | MIT | ✅ Fully compliant |
| C# Dependencies | MIT, BSD-3-Clause | ✅ All compatible |
| JavaScript Dependencies | MIT, ISC, BSD-3-Clause | ✅ All compatible |
| Python Scripts | MIT, BSD-3-Clause | ✅ All compatible |
| Documentation | MIT | ✅ Fully compliant |
| Planetary Data | Public Domain | ✅ Fully compliant |
| Example Consoles | MIT | ✅ Fully compliant |

**Overall Status:** ✅ **All components properly licensed and compatible**

---

**Last Updated:** April 5, 2026  
**License:** MIT (same as KSA-Bridge)  
**Maintained by:** KSA-Bridge Project Contributors
