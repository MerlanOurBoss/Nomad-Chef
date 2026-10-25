"""Yurt kitchen environment, built as a cut-away so the top-down game camera can see in.

* Kerege: lattice wall on the back and sides of a circle (open toward the camera, +Y),
  with a felt cover outside, a red ornament band (baskur) on top and a brown skirt.
* Uyk: red roof poles rising from the back wall toward the (unseen) shanyrak.
* Tekemet: round felt rug with a red border and a shanyrak symbol at its centre.
* Steppe ground around it.
Everything sits on y=0 in Unity; the rug is a few millimetres high so it never z-fights.
"""
import math
import os
import sys

sys.dont_write_bytecode = True

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import nc_art as art

PREVIEW_DIR = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else art.SOURCE_DIR

RADIUS = 7.2
WALL_HEIGHT = 2.1
ARC_START, ARC_END = math.radians(160), math.radians(380)   # back is -Y (270 deg)
SEGMENTS = 44


def polar(r, a, z):
    return (r * math.cos(a), r * math.sin(a), z)


def arc_segments():
    step = (ARC_END - ARC_START) / SEGMENTS
    for i in range(SEGMENTS):
        a = ARC_START + step * (i + 0.5)
        yield a, step * RADIUS * 1.02


def build_wall():
    parts = []
    for i, (a, width) in enumerate(arc_segments()):
        rot = (0, 0, a + math.pi / 2)
        parts.append(art.box(f"Felt{i}", (width, 0.08, WALL_HEIGHT), polar(RADIUS + 0.06, a, WALL_HEIGHT / 2), "FeltCream", rotation=rot))
        parts.append(art.box(f"Skirt{i}", (width, 0.1, 0.3), polar(RADIUS - 0.02, a, 0.15), "FeltBrown", rotation=rot))
        parts.append(art.box(f"Band{i}", (width, 0.06, 0.32), polar(RADIUS - 0.04, a, WALL_HEIGHT - 0.16), "FeltRed", rotation=rot))
        if i % 2 == 0:
            d = art.box(f"BandDiamond{i}", (0.16, 0.16, 0.02), polar(RADIUS - 0.08, a, WALL_HEIGHT - 0.16), "Gold")
            d.rotation_euler = (math.radians(90), math.radians(45), a + math.pi / 2)
            parts.append(d)

    # Kerege lattice: crossed slats on the inside of the felt.
    slat_len = (WALL_HEIGHT - 0.3) / math.cos(math.radians(40))
    count = int((ARC_END - ARC_START) * RADIUS / 0.42)
    for i in range(count + 1):
        a = ARC_START + (ARC_END - ARC_START) * i / count
        for lean in (-40, 40):
            s = art.box(f"Slat{i}{lean}", (0.035, 0.035, slat_len), polar(RADIUS - 0.03, a, 0.3 + (WALL_HEIGHT - 0.62) / 2), "WoodLight")
            # XYZ order: lean around local Y first (within the wall plane), then turn to face the centre.
            s.rotation_euler = (0, math.radians(lean), a + math.pi / 2)
            parts.append(s)
    return art.join("YurtWall", parts)


def build_poles():
    parts = []
    inner_r, inner_z = 4.6, 3.4
    count = 13
    back_start, back_end = math.radians(205), math.radians(335)
    for i in range(count):
        a = back_start + (back_end - back_start) * i / (count - 1)
        start = polar(RADIUS - 0.05, a, WALL_HEIGHT)
        end = polar(inner_r, a, inner_z)
        mid = tuple((s + e) / 2 for s, e in zip(start, end))
        length = math.dist(start, end)
        p = art.cylinder(f"Uyk{i}", 0.04, length, mid, "FeltRed", vertices=6)
        from mathutils import Vector
        p.rotation_euler = (Vector(end) - Vector(start)).to_track_quat("Z", "Y").to_euler()
        parts.append(p)
        parts.append(art.sphere(f"UykTip{i}", 0.05, end, "Gold", segments=6, rings=4))
    return art.join("YurtPoles", parts)


def build_rug():
    parts = [
        art.cylinder("RugBorder", RADIUS - 0.25, 0.01, (0, 0, 0.005), "FeltRed", vertices=48),
        art.cylinder("RugBorderGold", RADIUS - 0.55, 0.012, (0, 0, 0.006), "Gold", vertices=48),
        art.cylinder("RugField", RADIUS - 0.65, 0.014, (0, 0, 0.007), "FeltCream", vertices=48),
    ]
    # Ram-horn-ish diamonds around the border.
    for i in range(24):
        a = 2 * math.pi * i / 24
        d = art.box(f"RugDiamond{i}", (0.22, 0.22, 0.016), polar(RADIUS - 0.4, a, 0.008), "FeltCream")
        d.rotation_euler = (0, 0, a + math.pi / 4)
        parts.append(d)
    # Shanyrak symbol in the centre: a ring with two sets of three crossed arches (kuldyrysh).
    parts.append(art.torus("ShanyrakRing", 1.5, 0.09, (0, 0, 0.016), "FeltRed", major_segments=32, minor_segments=4))
    parts.append(art.cylinder("ShanyrakDisc", 1.42, 0.012, (0, 0, 0.009), "Turquoise", vertices=32))
    for rot in (math.radians(45), math.radians(-45)):
        for offset in (-0.45, 0.0, 0.45):
            bar = art.box(f"Kuldyrysh{rot}{offset}", (2.8, 0.1, 0.01), (0, 0, 0.018), "Gold")
            bar.rotation_euler = (0, 0, rot)
            bar.location = (offset * math.cos(rot + math.pi / 2), offset * math.sin(rot + math.pi / 2), 0.018)
            parts.append(bar)
    return art.join("Rug", parts)


def build_ground():
    return art.box("Steppe", (60, 60, 0.1), (0, 0, -0.05), "Grass")


art.reset_scene()
wall = build_wall()
poles = build_poles()
rug = build_rug()
ground = build_ground()
root = art.bpy.data.objects.new("YurtKitchen", None)
art.bpy.context.scene.collection.objects.link(root)
for obj in (wall, poles, rug, ground):
    obj.parent = root
art.export_fbx("YurtKitchen", [root])
art.save_source("YurtKitchen")
art.render_preview(os.path.join(PREVIEW_DIR, "yurt_preview.png"), target=(0, -3.0, 1.0), distance=13, elevation_deg=25, azimuth_deg=180)
print("NC_DONE")
