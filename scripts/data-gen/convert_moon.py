"""
Convert USGS Unified Geologic Map of the Moon (Fortezzo, Spudis,
Harrel; 2020; v2 GIS bundle) GeoContacts.shp into a simplified GeoJSON
for rendering on the FDO console globe.

Mirrors scripts/data-gen/convert_mars.py exactly so the Mars and Moon
pipelines stay parallel: read shapefile, reproject to lat/lon on the
body's reference sphere, filter classifications we don't want to draw,
simplify geometry, write GeoJSON to examples/hard-scifi/data/.

Source data: https://astrogeology.usgs.gov/search/map/unified_geologic_map_of_the_moon_1_5m_2020
Direct ZIP:  https://asc-astropedia.s3.us-west-2.amazonaws.com/Moon/Geology/Unified_Geologic_Map_of_the_Moon_GIS_v2.zip
Place the extracted Lunar_GIS/Shapefiles/ tree under
examples/hard-scifi/data/usgs_raw/moon/extracted/Unified_Geologic_Map_of_the_Moon_GIS/.
"""
import os
import sys

try:
    import geopandas as gpd
except ImportError as e:
    print(f"ERR: geopandas required (pip install geopandas): {e}")
    sys.exit(1)

DATA_DIR = r"C:\Users\john_\dev\KSA-Bridge\examples\hard-scifi\data"
RAW_DIR  = os.path.join(DATA_DIR, "usgs_raw", "moon", "extracted",
                        "Unified_Geologic_Map_of_the_Moon_GIS",
                        "Lunar_GIS", "Shapefiles")
OUTPUT   = os.path.join(DATA_DIR, "moon_geologic.geojson")

# Moon mean radius (IAU 2015) for the geographic projection target.
# The shapefile is in Moon2000 EquidistantCylindrical (units = meters
# on a 1737400 m sphere); we want decimal-degree lat/lon on the same
# sphere for the FDO console renderer.
MOON_R_M = 1737400
TARGET_CRS = f"+proj=longlat +R={MOON_R_M} +no_defs"

# ContactTyp values to drop:
#   DND             — "do not display" markers, the data set's own hint
#                     that these are noise/internal-only edges
#   Map boundary    — the rectangular Mercator clip border, useless on
#                     a globe view
DROP_CONTACT_TYPES = {"DND", "Map boundary"}

# Aggressive minimum projected-length filter so we drop many small
# craterlet rims that contribute visual noise without information.
# Units are meters (projected). The lunar UGM is much higher density
# than the Mars contacts; 150 km is what it takes to land the output
# in the 1-2 MB range while keeping mare basins, basin-impact rings,
# and major named crater rims. Tune up further if the file is still
# too big for your bandwidth budget.
MIN_SHAPE_LENG_M = 150_000

# Final simplification tolerance, in degrees of lat/lon after reproj.
# Mars uses 1.0; the moon's vertex density is much higher so start
# tighter (0.5) and back off if file size is too large.
SIMPLIFY_TOL_DEG = 0.5

# Target output size cap (~1 MB) — escalate simplification if exceeded.
MAX_OUTPUT_BYTES = 1_500_000


def main():
    contacts_shp = os.path.join(RAW_DIR, "GeoContacts.shp")
    if not os.path.exists(contacts_shp):
        print(f"ERR: shapefile not found at {contacts_shp}")
        print("Did you download and extract the UGM v2 GIS ZIP into"
              " examples/hard-scifi/data/usgs_raw/moon/?")
        sys.exit(1)

    print(f"Reading {contacts_shp} ...")
    gdf = gpd.read_file(contacts_shp)
    print(f"  initial rows: {len(gdf)}")
    print(f"  source crs:   {gdf.crs.to_string() if gdf.crs else '(none)'}")

    # 1. Reproject to lat/lon on the Moon 2000 sphere
    print(f"\nReprojecting to {TARGET_CRS} ...")
    gdf = gdf.to_crs(TARGET_CRS)
    print(f"  bounds (lon/lat): {gdf.total_bounds}")

    # 2. Filter out unwanted ContactTyp values
    if "ContactTyp" in gdf.columns:
        before = len(gdf)
        gdf = gdf[~gdf["ContactTyp"].isin(DROP_CONTACT_TYPES)]
        print(f"\nDropped {before - len(gdf)} rows with ContactTyp in {sorted(DROP_CONTACT_TYPES)}")
        print(f"  remaining: {len(gdf)}")

    # 3. Drop small features by projected length (Shape_Leng is in meters
    #    in the source CRS, regardless of the reprojection above)
    if "Shape_Leng" in gdf.columns:
        before = len(gdf)
        gdf = gdf[gdf["Shape_Leng"] >= MIN_SHAPE_LENG_M]
        print(f"\nDropped {before - len(gdf)} rows shorter than"
              f" {MIN_SHAPE_LENG_M} m source-projected length")
        print(f"  remaining: {len(gdf)}")

    # 4. Simplify geometry
    def simplify(df, tol):
        df = df.copy()
        df["geometry"] = df["geometry"].simplify(tolerance=tol, preserve_topology=True)
        df = df[~df.geometry.is_empty]
        return df

    print(f"\nSimplifying at tolerance={SIMPLIFY_TOL_DEG} degrees ...")
    out = simplify(gdf, SIMPLIFY_TOL_DEG)
    print(f"  rows after simplify: {len(out)}")

    # 5. Keep just the columns we actually use downstream. Rename
    #    ContactTyp -> ConType so the moon data uses the same field
    #    name as Mars's contacts; the FDO console picks the right
    #    body-specific material set by bodyName, but the column
    #    convention is shared.
    keep = ["geometry"]
    if "ContactTyp" in out.columns:
        out = out.rename(columns={"ContactTyp": "ConType"})
        keep.append("ConType")
    out = out[keep]

    # 6. Normalise ConType casing so JS dict lookups stay consistent
    #    with the values we'll wire into bodyDataConfig: title-case the
    #    classification labels (certain -> Certain, approximate -> Approx).
    canon = {
        "certain":     "Certain",
        "approximate": "Approx",
        "Internal":    "Internal",
        "inferred":    "Inferred",
        "buried":      "Buried",
    }
    if "ConType" in out.columns:
        out["ConType"] = out["ConType"].map(lambda v: canon.get(v, v))
        print("\nConType histogram after normalisation:")
        for v, n in out["ConType"].value_counts().items():
            print(f"  {n:6d}  {v}")

    # 7. Write GeoJSON, escalate simplification if too large
    print(f"\nWriting {OUTPUT} ...")
    if os.path.exists(OUTPUT):
        os.remove(OUTPUT)
    out.to_file(OUTPUT, driver="GeoJSON")
    size = os.path.getsize(OUTPUT)
    print(f"  size: {size:,} bytes ({size/1024:.0f} KB)")

    next_tols = [1.0, 1.5, 2.0]
    for tol in next_tols:
        if size <= MAX_OUTPUT_BYTES:
            break
        print(f"\nOver target ({MAX_OUTPUT_BYTES:,} B); simplifying at tol={tol} ...")
        out = simplify(gdf, tol)
        # Re-apply column rename + normalisation after re-simplify
        if "ContactTyp" in out.columns:
            out = out.rename(columns={"ContactTyp": "ConType"})
        keep = ["geometry"] + [c for c in ["ConType"] if c in out.columns]
        out = out[keep]
        if "ConType" in out.columns:
            out["ConType"] = out["ConType"].map(lambda v: canon.get(v, v))
        os.remove(OUTPUT)
        out.to_file(OUTPUT, driver="GeoJSON")
        size = os.path.getsize(OUTPUT)
        print(f"  size: {size:,} bytes ({size/1024:.0f} KB)")

    print("\nDone.")


if __name__ == "__main__":
    main()
