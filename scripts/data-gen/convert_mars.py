"""
Convert USGS SIM3292 Mars Global Contacts shapefile to simplified GeoJSON
for rendering on the FDO console globe.
"""
import geopandas as gpd
import json
import os

DATA_DIR = r"C:\Users\john_\dev\KSA-Bridge\examples\hard-scifi\data"
RAW_DIR = os.path.join(DATA_DIR, "usgs_raw")
OUTPUT = os.path.join(DATA_DIR, "mars_contacts.geojson")

print("Reading Global_Contacts shapefile...")
contacts = gpd.read_file(os.path.join(RAW_DIR, "SIM3292_Global_Contacts.shp"))
print(f"  {len(contacts)} features")
print(f"  CRS: {contacts.crs}")
print(f"  Columns: {list(contacts.columns)}")
print(f"  ConType values: {contacts['ConType'].unique() if 'ConType' in contacts.columns else 'N/A'}")

# Check projection - USGS Mars data uses Robinson or Mars 2000
# We need lat/lon (EPSG:4326 equivalent for Mars)
print(f"\n  Bounds: {contacts.total_bounds}")

# Read the PRJ file to understand the projection
with open(os.path.join(RAW_DIR, "SIM3292_Global_Contacts.prj"), "r") as f:
    print(f"  PRJ: {f.read()[:200]}")

# Simplify geometry - reduce vertex count
# tolerance in the units of the CRS (likely degrees or meters depending on projection)
print("\nSimplifying geometry...")
contacts_simple = contacts.copy()
contacts_simple['geometry'] = contacts_simple['geometry'].simplify(tolerance=0.5, preserve_topology=True)

# Drop empty geometries
contacts_simple = contacts_simple[~contacts_simple.geometry.is_empty]
print(f"  {len(contacts_simple)} features after simplification")

# Keep only useful columns
keep_cols = ['geometry']
if 'ConType' in contacts_simple.columns:
    keep_cols.append('ConType')
if 'Unit' in contacts_simple.columns:
    keep_cols.append('Unit')
contacts_simple = contacts_simple[keep_cols]

# Write to GeoJSON
print(f"\nWriting to {OUTPUT}...")
contacts_simple.to_file(OUTPUT, driver="GeoJSON")
file_size = os.path.getsize(OUTPUT)
print(f"  Output size: {file_size:,} bytes ({file_size/1024:.0f} KB)")

# If too large, simplify more aggressively
if file_size > 1_000_000:
    print("\n  Still over 1MB, simplifying more aggressively...")
    contacts_simple['geometry'] = contacts['geometry'].simplify(tolerance=1.0, preserve_topology=True)
    contacts_simple = contacts_simple[~contacts_simple.geometry.is_empty]
    contacts_simple.to_file(OUTPUT, driver="GeoJSON")
    file_size = os.path.getsize(OUTPUT)
    print(f"  Output size: {file_size:,} bytes ({file_size/1024:.0f} KB)")

if file_size > 1_000_000:
    print("\n  Still over 1MB, trying tolerance=2.0...")
    contacts_simple['geometry'] = contacts['geometry'].simplify(tolerance=2.0, preserve_topology=True)
    contacts_simple = contacts_simple[~contacts_simple.geometry.is_empty]
    contacts_simple.to_file(OUTPUT, driver="GeoJSON")
    file_size = os.path.getsize(OUTPUT)
    print(f"  Output size: {file_size:,} bytes ({file_size/1024:.0f} KB)")

print("\nAlso processing Global_Geology for reference...")
geology = gpd.read_file(os.path.join(RAW_DIR, "SIM3292_Global_Geology.shp"))
print(f"  {len(geology)} features")
print(f"  Columns: {list(geology.columns)}")
if 'UnitSymbol' in geology.columns:
    print(f"  Units: {sorted(geology['UnitSymbol'].unique())}")
elif 'Unit' in geology.columns:
    print(f"  Units: {sorted(geology['Unit'].unique())}")

print("\nDone!")
