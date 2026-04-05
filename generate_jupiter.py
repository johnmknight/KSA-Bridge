"""
Generate Jupiter cloud band boundaries and features as GeoJSON.

Belt/zone boundary latitudes from Rogers (1995) "The Giant Planet Jupiter"
and Voyager/Cassini jet stream measurements. Latitudes are planetocentric.

Belts (dark) alternate with Zones (light) from equator to poles.
Belt interiors use halftone-style dots (small hexagons) for newsprint fill.
Storm interiors use denser dot patterns.
"""
import json, math, random

random.seed(42)

bands = [
    (-58, -51, "S3TB",   "belt"),
    (-51, -46, "S3TZ",   "zone"),
    (-46, -40, "SSTZ",   "zone"),
    (-40, -36, "SSTB",   "belt"),
    (-36, -33, "STZ",    "zone"),
    (-33, -27, "STB",    "belt"),
    (-27, -21, "STropZ", "zone"),
    (-21,  -7, "SEB",    "belt"),
    ( -7,   7, "EZ",     "zone"),
    (  7,  17, "NEB",    "belt"),
    ( 17,  24, "NTropZ", "zone"),
    ( 24,  31, "NTB",    "belt"),
    ( 31,  35, "NTZ",    "zone"),
    ( 35,  40, "NNTB",   "belt"),
    ( 40,  45, "NNTZ",   "zone"),
    ( 45,  50, "N3TB",   "belt"),
    ( 50,  58, "N3TZ",   "zone"),
]

def make_dot(lon, lat, radius_deg, n_sides=6):
    """Generate a small closed polygon (hexagon) as a dot."""
    coords = []
    for i in range(n_sides + 1):
        angle = (i / n_sides) * 2 * math.pi
        dx = radius_deg * math.cos(angle)
        # Correct longitude spacing for latitude (wider near equator)
        cos_lat = math.cos(math.radians(lat))
        if cos_lat < 0.1: cos_lat = 0.1
        dy = radius_deg * math.sin(angle) / cos_lat
        coords.append([round(lon + dy, 3), round(lat + dx, 3)])
    return coords

features = []

# Band boundary lines
drawn_lats = set()
for south, north, name, btype in bands:
    for lat in [south, north]:
        if lat not in drawn_lats and abs(lat) < 88:
            coords = [[lon, lat] for lon in range(-180, 181, 5)]
            features.append({
                "type": "Feature",
                "properties": {"name": f"boundary_{lat}", "type": "boundary"},
                "geometry": {"type": "LineString", "coordinates": coords}
            })
            drawn_lats.add(lat)

    # Belt interior: halftone dots
    if btype == "belt":
        span = north - south
        dot_radius = 0.35  # degrees - size of each dot
        # Density: one dot per ~10 sq-degrees
        n_dots = int((360 * span) / 10)
        for _ in range(n_dots):
            lon0 = random.uniform(-180, 180)
            lat0 = random.uniform(south + 0.5, north - 0.5)
            coords = make_dot(lon0, lat0, dot_radius)
            features.append({
                "type": "Feature",
                "properties": {"name": name, "type": "belt_hash"},
                "geometry": {"type": "Polygon", "coordinates": [coords]}
            })

# Equator
coords_eq = [[lon, 0] for lon in range(-180, 181, 5)]
features.append({
    "type": "Feature",
    "properties": {"name": "Equator", "type": "equator"},
    "geometry": {"type": "LineString", "coordinates": coords_eq}
})

# Great Red Spot - ellipse outline + interior dot fill
grs_lat, grs_lon = -22.5, 300
grs_dlat, grs_dlon = 2.5, 7.25

# GRS outline
grs_coords = []
for i in range(65):
    angle = (i / 64) * 2 * math.pi
    lon = grs_lon + grs_dlon * math.cos(angle)
    lat = grs_lat + grs_dlat * math.sin(angle)
    if lon > 180: lon -= 360
    grs_coords.append([round(lon, 2), round(lat, 2)])
grs_coords.append(grs_coords[0])
features.append({
    "type": "Feature",
    "properties": {"name": "Great Red Spot", "type": "storm", "abbrev": "GRS"},
    "geometry": {"type": "LineString", "coordinates": grs_coords}
})

# GRS concentric rings (2 inner ellipses at 60% and 30% size)
for scale in [0.6, 0.3]:
    ring = []
    for i in range(49):
        angle = (i / 48) * 2 * math.pi
        lon = grs_lon + grs_dlon * scale * math.cos(angle)
        lat = grs_lat + grs_dlat * scale * math.sin(angle)
        if lon > 180: lon -= 360
        ring.append([round(lon, 2), round(lat, 2)])
    ring.append(ring[0])
    features.append({
        "type": "Feature",
        "properties": {"name": "GRS ring", "type": "storm_detail"},
        "geometry": {"type": "LineString", "coordinates": ring}
    })

# GRS interior halftone dots (denser than belts)
for _ in range(80):
    # Random point inside ellipse using rejection sampling
    while True:
        rx = random.uniform(-1, 1)
        ry = random.uniform(-1, 1)
        if rx*rx + ry*ry <= 0.85:  # inside ~92% of ellipse
            break
    lon0 = grs_lon + grs_dlon * rx
    lat0 = grs_lat + grs_dlat * ry
    if lon0 > 180: lon0 -= 360
    coords = make_dot(lon0, lat0, 0.25)
    features.append({
        "type": "Feature",
        "properties": {"name": "GRS fill", "type": "storm_detail"},
        "geometry": {"type": "Polygon", "coordinates": [coords]}
    })

# Oval BA - outline + small interior
ba_lat, ba_lon = -33.0, 310
ba_dlat, ba_dlon = 1.5, 3.0
ba_coords = []
for i in range(33):
    angle = (i / 32) * 2 * math.pi
    lon = ba_lon + ba_dlon * math.cos(angle)
    lat = ba_lat + ba_dlat * math.sin(angle)
    if lon > 180: lon -= 360
    ba_coords.append([round(lon, 2), round(lat, 2)])
ba_coords.append(ba_coords[0])
features.append({
    "type": "Feature",
    "properties": {"name": "Oval BA", "type": "storm", "abbrev": "BA"},
    "geometry": {"type": "LineString", "coordinates": ba_coords}
})

# Oval BA inner ring
ba_ring = []
for i in range(25):
    angle = (i / 24) * 2 * math.pi
    lon = ba_lon + ba_dlon * 0.5 * math.cos(angle)
    lat = ba_lat + ba_dlat * 0.5 * math.sin(angle)
    if lon > 180: lon -= 360
    ba_ring.append([round(lon, 2), round(lat, 2)])
ba_ring.append(ba_ring[0])
features.append({
    "type": "Feature",
    "properties": {"name": "BA ring", "type": "storm_detail"},
    "geometry": {"type": "LineString", "coordinates": ba_ring}
})

# Oval BA interior dots
for _ in range(20):
    while True:
        rx = random.uniform(-1, 1)
        ry = random.uniform(-1, 1)
        if rx*rx + ry*ry <= 0.85:
            break
    lon0 = ba_lon + ba_dlon * rx
    lat0 = ba_lat + ba_dlat * ry
    if lon0 > 180: lon0 -= 360
    coords = make_dot(lon0, lat0, 0.2)
    features.append({
        "type": "Feature",
        "properties": {"name": "BA fill", "type": "storm_detail"},
        "geometry": {"type": "Polygon", "coordinates": [coords]}
    })

geojson = {"type": "FeatureCollection", "name": "jupiter_bands", "features": features}

outpath = r"C:\Users\john_\dev\KSA-Bridge\examples\hard-scifi\data\jupiter_bands.geojson"
with open(outpath, 'w') as f:
    json.dump(geojson, f)

import os
size = os.path.getsize(outpath)
types = {}
for f in features:
    t = f['properties']['type']
    types[t] = types.get(t, 0) + 1
print(f"Generated {len(features)} features")
for t, n in sorted(types.items()):
    print(f"  {n} {t}")
print(f"Output: {size:,} bytes ({size/1024:.0f} KB)")