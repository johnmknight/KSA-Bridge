# KSA-Bridge Data Sources

Complete attribution and licensing information for all planetary surface data used in KSA-Bridge example consoles.

---

## Mars: Geologic Contact Boundaries

**Source:** USGS SIM 3292 - Global Geologic Map of Mars  
**Reference:** Tanaka, K. L., Skinner Jr., J. A., Dohm, J. M., Irwin III, R. P., Kolb, E. J., Fortezzo, C. M., Platz, T., Michael, G. G., & Hare, T. M. (2014).  
**Publication:** U.S. Geological Survey Scientific Investigations Map 3292  
**DOI:** https://doi.org/10.3133/sim3292  
**URL:** https://pubs.usgs.gov/sim/3292/  

**Description:**  
Global map showing geologic unit boundaries and contacts on Mars at 1:20,000,000 scale. Compiled from THEMIS, MOC, HRSC, CTX, and HiRISE imagery. Contacts classified as:
- **Certain**: Well-defined boundaries (amber in console)
- **Approximate**: Boundaries with lower confidence (rust in console)
- **Internal**: Boundaries within units (teal in console)
- **Border**: Unit boundaries at map edges (gold in console)

**Coordinate System:** GCS Mars 2000 Sphere (geographic lat/lon on Mars ellipsoid)  
**Projection:** Natural spherical coordinates  
**License:** Public Domain (U.S. Government work)  
**Data Format:** Original shapefiles (.shp, .dbf, .shx); converted to GeoJSON for web use  

**Regeneration:**
```bash
pip install geopandas fiona shapely
# Place SIM3292_Global_Contacts.shp in examples/hard-scifi/data/usgs_raw/
python scripts/data-gen/convert_mars.py
```
Output: `examples/hard-scifi/data/mars_contacts.geojson` (~842 KB, 3708 features, simplified to 1.0° tolerance)

**Educational Use:** ✅ Freely available for educational and research purposes  
**Citation:** Tanaka et al. (2014), USGS SIM 3292

---

## Earth: Coastlines and Land Boundaries

**Source:** Natural Earth Data - 110m Cultural Vectors  
**Maintained by:** Stramen Design, North American Cartographic Information Society  
**URL:** https://www.naturalearthdata.com/  
**Version:** 5.x series  

**Datasets Used:**
- `ne_110m_coastline.shp` — Global coastline boundaries
- `ne_110m_land.shp` — Land area polygons

**Description:**  
High-quality vector data representing Earth's coastlines and land masses at 1:110m scale. Derived from the Global Self-consistent, Hierarchical, High-resolution Shoreline (GSHHS) database and refined through manual editing.

**Coordinate System:** WGS84 (EPSG:4326)  
**License:** Public Domain  
**Data Format:** Shapefiles (original); served as TopoJSON from CDN  

**CDN Source:**  
The console loads Earth data directly from the Natural Earth CDN:  
```html
<script src="https://d3js.org/topojson.v2.min.js"></script>
<!-- Earth coastlines loaded from Natural Earth CDN -->
<link rel="prefetch" href="https://cdn.jsdelivr.net/npm/world-atlas@2/land-50m.json">
```

**Educational Use:** ✅ Freely available, designed specifically for education  
**Citation:** Natural Earth Data (https://www.naturalearthdata.com/)

---

## Jupiter: Cloud Bands and Atmospheric Features

**Source:** Established atmospheric dynamics literature  
**Primary References:**
- Rogers, J. H. (1995). "The Giant Planet Jupiter." *Practical Astronomy Series*, Springer-Verlag.
- Voyager 1/2 imaging data (NASA, 1979)
- Cassini imaging and spectroscopy (NASA, 2000–2017)
- Juno imaging data (NASA, 2016–present)

**Description:**  
Atmospheric circulation bands derived from:
- **Cloud belt/zone latitudes** from Rogers (1995) and refined by modern imaging
- **Great Red Spot** position and structure from Cassini and Juno
- **Oval BA** (South Polar Storm) from post-2000 observations
- **Small storm features** mapped from recent Juno high-resolution imagery

**Band Classification:**
- Equatorial Zone (EZ) — bright, fast westerly winds
- Tropical Regions (NTZ, STZ) — pale zones
- Temperate Belts (NEB, SEB, etc.) — dark, slow easterly winds
- Polar Regions — complex circulation patterns

**Coordinate System:** Cylindrical (latitude, longitude on 10-bar pressure level)  
**Data Format:** GeoJSON with feature attributes (belt/zone ID, latitude range, color coding)  
**License:** Public Domain (NASA data + published literature)  

**Generation:**
```bash
python scripts/data-gen/generate_jupiter.py
```
Output: `examples/hard-scifi/data/jupiter_bands.geojson`

**Educational Use:** ✅ All NASA data is public domain; Rogers book widely available in academic libraries  
**Citation:** Rogers (1995); NASA Voyager, Cassini, and Juno mission data

---

## Moon: Maria and Major Features

**Current Status:** Outline documentation only (data TBD)

**Potential Sources:**
- **USGS Astrogeology**: https://astrogeology.usgs.gov/
  - Lunar Mare Cataloging Project
  - Lunar Impact Crater Database
  - LRO Digital Elevation Model and orthophotos

- **International Astronomical Union**: Lunar Feature Nomenclature
  - IAU Gazetteer of Planetary Nomenclature
  - https://planetarynames.wr.usgs.gov/

- **NASA Lunar Mapping and Modeling**: 
  - Lunar Orbiter Laser Altimeter (LOLA)
  - Lunar Reconnaissance Orbiter (LRO) imagery

**Planned Implementation:**  
- Major mare boundaries (Maria Imbrium, Tranquillitatis, Serenitatis, etc.)
- Crater rings for prominent features (Tycho, Copernicus, Clavius)
- Elevation data from LRO LOLA

---

## Mercury: Crater Rings and Terrain Features

**Current Status:** Outline documentation only (data TBD)

**Potential Sources:**
- **USGS Astrogeology**: Mercury Crater Database
- **NASA MESSENGER Mission**: Global imaging and topography (2011–2015)
- **BepiColombo Mission**: Current high-resolution mapping

**Data to Include:**
- Major crater rim boundaries
- Ancient basin structures
- Terrain type classification (smooth plains, intercrater terrain, etc.)

---

## Venus: Surface Features and Topography

**Current Status:** Outline documentation only (data TBD)

**Potential Sources:**
- **NASA Magellan Mission**: SAR imagery and altimetry (1990–1994)
- **USGS Astrogeology**: Venus crater and feature databases
- **Akatsuki (Venus Climate Orbiter)**: Modern atmospheric and surface data

**Data to Include:**
- Major volcanic features (coronae, calderas)
- Highland/lowland topography
- Impact crater locations

---

## Data Processing Pipeline

### Mars Geologic Contacts (Implemented)

```mermaid
USGS SIM 3292 Shapefiles (GCS Mars 2000)
    ↓
  [geopandas reads .shp]
    ↓
  [Geometry simplification (1.0° tolerance)]
    ↓
  [Remove null/invalid geometries]
    ↓
  [Project to WGS84 equivalent for Mars]
    ↓
  [Color-code by contact type]
    ↓
  mars_contacts.geojson (~842 KB)
    ↓
  [Three.js loads and renders on 3D globe]
```

**Script:** `scripts/data-gen/convert_mars.py`  
**Dependencies:** geopandas, fiona, shapely

### Jupiter Cloud Bands (Implemented)

```mermaid
Rogers (1995) + NASA imagery
    ↓
  [Define belt/zone latitudes and widths]
    ↓
  [Generate band geometry (polygons/polylines)]
    ↓
  [Add great storm features (GRS, Oval BA)]
    ↓
  [Assign color codes and metadata]
    ↓
  jupiter_bands.geojson
    ↓
  [Three.js renders with fill + outline patterns]
```

**Script:** `scripts/data-gen/generate_jupiter.py`  
**Dependencies:** shapely, geojson

---

## Licensing Summary

| Body | License | Attribution Required? | Commercial Use? | Modifications? |
|------|---------|----------------------|-----------------|---------------|
| Mars | Public Domain (USGS) | Recommended | ✅ Yes | ✅ Yes |
| Earth | Public Domain (Natural Earth) | Recommended | ✅ Yes | ✅ Yes |
| Jupiter | Public Domain (NASA + Literature) | Recommended | ✅ Yes | ✅ Yes |
| Moon | Public Domain (USGS/NASA) | TBD | ✅ Yes | ✅ Yes |
| Mercury | Public Domain (USGS/NASA) | TBD | ✅ Yes | ✅ Yes |
| Venus | Public Domain (USGS/NASA) | TBD | ✅ Yes | ✅ Yes |

**All data is freely available for educational, research, and commercial use with proper attribution.**

---

## How to Cite KSA-Bridge Data

### Mars:
> Mars geologic data derived from the USGS SIM 3292 Global Geologic Map (Tanaka et al., 2014). Simplified for visualization in KSA-Bridge telemetry console.

### Earth:
> Coastline data from Natural Earth (https://www.naturalearthdata.com/). Served via 110m vector dataset.

### Jupiter:
> Atmospheric band data derived from established cloud circulation patterns (Rogers, 1995) and refined with imagery from NASA Voyager, Cassini, and Juno missions.

### Complete Citation (All Data):
> Planetary surface data visualized in KSA-Bridge includes:
> - Mars: USGS SIM 3292 (Tanaka et al., 2014)
> - Earth: Natural Earth Data
> - Jupiter: Atmospheric dynamics literature (Rogers, 1995) + NASA mission imagery

---

## For Educators

This data is **specifically chosen for educational use**:
- ✅ All sources are publicly available and freely licensed
- ✅ Original scientific publications are accessible
- ✅ NASA data is public domain by law
- ✅ No restrictive copyrights or paywalls
- ✅ Suitable for classroom use, assignments, and research projects

**Questions about data use in educational settings?**  
Contact the data providers directly:
- USGS Astrogeology: https://astrogeology.usgs.gov/
- Natural Earth: https://www.naturalearthdata.com/
- NASA: https://science.nasa.gov/

---

## Contributing New Datasets

If you'd like to add data for other bodies or improve existing datasets:

1. **Verify the license** — ensure it permits educational and potentially commercial use
2. **Cite the source** — add an entry to this file with complete attribution
3. **Simplify for web** — convert large datasets to GeoJSON/TopoJSON with reasonable file sizes
4. **Add a generation script** — document how to regenerate the data from the original source
5. **Test on the console** — verify the geometry renders correctly on the 3D globe
6. **Submit a PR** — include the data source documentation and generation script

---

**Last Updated:** April 5, 2026  
**Maintained by:** KSA-Bridge Project Contributors  
**For Questions:** See README.md and SETUP.md
