"""Main menu backdrop: a yurt on the steppe with a kazan over a camp fire and the Alatau behind.

The yurt stands at the origin with its door facing +Y (toward the menu camera, Unity -Z).
Mountains are far away along -Y (Unity +Z).
"""
import math
import os
import random
import sys

sys.dont_write_bytecode = True
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import nc_art as art
from mathutils import Vector

PREVIEW_DIR = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else art.SOURCE_DIR
rng = random.Random(3)


def stick(name, start, end, radius, mat, vertices=6):
    start, end = Vector(start), Vector(end)
    obj = art.cylinder(name, radius, (end - start).length, (start + end) / 2, mat, vertices=vertices)
    obj.rotation_euler = (end - start).to_track_quat("Z", "Y").to_euler()
    return obj


def yurt():
    r, wall_h = 2.4, 1.5
    parts = [
        art.cylinder("Wall", r, wall_h, (0, 0, wall_h / 2), "FeltCream", vertices=28),
        art.cylinder("Skirt", r + 0.02, 0.25, (0, 0, 0.125), "FeltBrown", vertices=28),
        art.cylinder("Band", r + 0.03, 0.26, (0, 0, wall_h - 0.15), "FeltRed", vertices=28),
        art.cone("Roof", r + 0.15, 0.55, 1.15, (0, 0, wall_h + 0.575), "FeltCream", vertices=28),
        art.torus("Shanyrak", 0.55, 0.07, (0, 0, wall_h + 1.17), "WoodDark", major_segments=20, minor_segments=5),
    ]
    for rot in (math.radians(45), math.radians(-45)):
        for off in (-0.2, 0.0, 0.2):
            bar = art.box(f"Kuldyrysh{rot}{off}", (1.0, 0.05, 0.05), (0, 0, wall_h + 1.2), "FeltRed")
            bar.rotation_euler = (0, 0, rot)
            bar.location = (off * math.cos(rot + math.pi / 2), off * math.sin(rot + math.pi / 2), wall_h + 1.2)
            parts.append(bar)
    for z in (0.55, 0.95):
        parts.append(art.torus(f"Rope{z}", r + 0.02, 0.025, (0, 0, z), "FeltBrown", major_segments=28, minor_segments=3))
    for i in range(24):
        a = 2 * math.pi * i / 24
        d = art.box(f"BandDiamond{i}", (0.14, 0.14, 0.02), (0, 0, 0), "Gold")
        d.location = ((r + 0.05) * math.cos(a), (r + 0.05) * math.sin(a), wall_h - 0.15)
        d.rotation_euler = (math.radians(90), math.radians(45), a + math.pi / 2)
        parts.append(d)
    # Roof ribbons running from the band up to the shanyrak.
    for i in range(8):
        a = 2 * math.pi * (i + 0.5) / 8
        parts.append(stick(f"Ribbon{i}", ((r + 0.12) * math.cos(a), (r + 0.12) * math.sin(a), wall_h + 0.02),
                           (0.6 * math.cos(a), 0.6 * math.sin(a), wall_h + 1.15), 0.035, "FeltRed", vertices=4))
    # Painted wooden door facing +Y.
    parts.append(art.box("DoorFrame", (0.86, 0.12, 1.32), (0, r - 0.01, 0.66), "WoodDark"))
    parts.append(art.box("Door", (0.72, 0.1, 1.2), (0, r + 0.02, 0.62), "FeltRed"))
    parts.append(art.box("DoorBorder", (0.6, 0.1, 1.0), (0, r + 0.03, 0.64), "Gold"))
    parts.append(art.box("DoorPanel", (0.52, 0.1, 0.92), (0, r + 0.04, 0.64), "FeltRed"))
    d = art.box("DoorDiamond", (0.2, 0.2, 0.02), (0, r + 0.1, 0.66), "Gold")
    d.rotation_euler = (math.radians(90), math.radians(45), 0)
    parts.append(d)
    return art.join("Yurt", parts)


def camp_fire(x, y):
    parts = []
    top = (x, y, 1.55)
    for i in range(3):
        a = 2 * math.pi * i / 3 + 0.3
        parts.append(stick(f"Tripod{i}", (x + 0.75 * math.cos(a), y + 0.75 * math.sin(a), 0), top, 0.035, "WoodDark"))
    parts.append(stick("Chain", top, (x, y, 0.95), 0.012, "Iron", vertices=4))
    parts.append(art.cone("FireKazan", 0.18, 0.32, 0.28, (x, y, 0.78), "Iron", vertices=14))
    parts.append(art.torus("FireKazanRim", 0.32, 0.025, (x, y, 0.92), "Iron", major_segments=16, minor_segments=4))
    for i in range(5):
        a = 2 * math.pi * i / 5
        log = stick(f"Log{i}", (x + 0.35 * math.cos(a), y + 0.35 * math.sin(a), 0.06), (x + 0.05 * math.cos(a), y + 0.05 * math.sin(a), 0.2),
                    0.05, "WoodLight", vertices=6)
        parts.append(log)
    parts.append(art.cone("Flame", 0.18, 0.0, 0.42, (x, y, 0.38), "Ember", vertices=7))
    parts.append(art.cone("FlameCore", 0.09, 0.0, 0.26, (x, y, 0.32), "Highlight", vertices=6))
    for i in range(8):
        a = 2 * math.pi * i / 8
        parts.append(art.sphere(f"Stone{i}", 0.08, (x + 0.42 * math.cos(a), y + 0.42 * math.sin(a), 0.04), "Stone", segments=6, rings=4))
    return art.join("CampFire", parts)


def landscape():
    parts = [art.cylinder("Ground", 90, 0.1, (0, 0, -0.05), "Grass", vertices=32)]
    for i in range(9):
        x = rng.uniform(-45, 45)
        y = rng.uniform(-38, -14)
        s = rng.uniform(6, 12)
        parts.append(art.sphere(f"Hill{i}", s, (x, y, -s * 0.72), "Hill", segments=12, rings=8, scale=(1.6, 1, 1)))
    for i in range(7):
        x = -60 + i * 20 + rng.uniform(-6, 6)
        h = rng.uniform(16, 26)
        y = rng.uniform(-75, -62)
        parts.append(art.cone(f"Peak{i}", h * 0.75, 0.0, h, (x, y, h / 2 - 2), "Mountain", vertices=7))
        parts.append(art.cone(f"Snow{i}", h * 0.75 * 0.3, 0.0, h * 0.3, (x, y, h - 2 - h * 0.15 + 0.05), "Snow", vertices=7))
    for i in range(40):
        a = rng.uniform(0, 2 * math.pi)
        d = rng.uniform(3.2, 12)
        parts.append(art.cone(f"Tuft{i}", 0.12, 0.0, 0.25, (d * math.cos(a), d * math.sin(a), 0.12), "Hill", vertices=4))
    return art.join("Landscape", parts)


art.reset_scene()
root = art.bpy.data.objects.new("MenuBackdrop", None)
art.bpy.context.scene.collection.objects.link(root)
for obj in (yurt(), camp_fire(-3.6, 1.4), landscape()):
    obj.parent = root
art.export_fbx("MenuBackdrop", [root])
art.save_source("MenuBackdrop")
art.render_preview(os.path.join(PREVIEW_DIR, "menu_preview.png"), target=(-1.2, 0, 1.3), distance=9, elevation_deg=8, azimuth_deg=200)
print("NC_DONE")
