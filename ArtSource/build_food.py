"""Food: raw, chopped and cooked ingredients plus burned food.

Each model sits on its origin (bottom centre) and fits roughly inside 0.45 x 0.45 m,
the size of the old placeholder shapes, so plates and counters keep working.
"""
import math
import os
import random
import sys

sys.dont_write_bytecode = True
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import nc_art as art

PREVIEW_DIR = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else art.SOURCE_DIR
rng = random.Random(7)


def meat_raw():
    slab = art.box("Slab", (0.40, 0.32, 0.16), (0, 0, 0.09), "MeatRaw", bevel=0.05)
    fat = art.box("Fat", (0.41, 0.06, 0.05), (0, 0.06, 0.15), "MeatFat", bevel=0.02)
    fat2 = art.box("Fat2", (0.30, 0.04, 0.04), (0.03, -0.08, 0.165), "MeatFat", bevel=0.015)
    shaft = art.cylinder("BoneShaft", 0.03, 0.2, (0.25, 0.0, 0.1), "Bone", vertices=8, rotation=(0, math.radians(90), 0))
    knob = art.sphere("BoneKnob", 0.05, (0.35, 0.0, 0.1), "Bone", segments=8, rings=6)
    return art.join("Meat", [slab, fat, fat2, shaft, knob])


def chunks(name, mat, fat_mat, count, size, spread):
    parts = []
    for i in range(count):
        a = 2 * math.pi * i / count + rng.uniform(-0.3, 0.3)
        r = spread * (0.35 + 0.65 * rng.random()) if i else 0
        s = size * rng.uniform(0.85, 1.15)
        c = art.box(f"Chunk{i}", (s, s * 0.9, s * 0.8), (r * math.cos(a), r * math.sin(a), s * 0.4), mat, bevel=s * 0.25)
        c.rotation_euler = (0, 0, rng.uniform(0, math.pi))
        parts.append(c)
        if fat_mat and i % 2 == 0:
            f = art.box(f"Fat{i}", (s * 1.02, s * 0.25, s * 0.2), (r * math.cos(a), r * math.sin(a), s * 0.75), fat_mat)
            f.rotation_euler = c.rotation_euler
            parts.append(f)
    return art.join(name, parts)


def onion():
    bulb = art.sphere("Bulb", 0.15, (0, 0, 0.14), "OnionSkin", segments=12, rings=8, scale=(1, 1, 0.92))
    tip = art.cone("Tip", 0.06, 0.0, 0.1, (0, 0, 0.3), "OnionSkin", vertices=8)
    root = art.cylinder("Root", 0.04, 0.02, (0, 0, 0.01), "Bone", vertices=8)
    stripes = []
    for i in range(4):
        s = art.torus(f"Stripe{i}", 0.148, 0.006, (0, 0, 0.14), "OnionFlesh", major_segments=16, minor_segments=3,
                      rotation=(math.radians(90), 0, math.radians(45 * i)))
        s.scale = (1, 1, 0.92)
        stripes.append(s)
    return art.join("Onion", [bulb, tip, root] + stripes)


def onion_sliced():
    parts = []
    for i in range(4):
        x = -0.12 + i * 0.08
        z = 0.012 + (i % 2) * 0.012
        parts.append(art.cylinder(f"Slice{i}", 0.11, 0.022, (x, rng.uniform(-0.03, 0.03), z), "OnionFlesh", vertices=14))
        parts.append(art.torus(f"Skin{i}", 0.11, 0.012, (x, parts[-1].location.y, z), "OnionSkin", major_segments=14, minor_segments=4))
        parts.append(art.torus(f"Ring{i}", 0.065, 0.006, (x, parts[-2].location.y, z + 0.011), "OnionSkin", major_segments=12, minor_segments=3))
    return art.join("OnionSliced", parts)


def dough():
    ball = art.sphere("Ball", 0.21, (0, 0, 0.09), "Dough", segments=14, rings=8, scale=(1, 0.95, 0.45))
    return art.join("Dough", [ball])


def dough_squares(name, mat, size, wobble):
    parts = []
    for x in range(-1, 2):
        for y in range(-1, 2):
            sq = art.box(f"Sq{x}{y}", (size, size, 0.02), (x * 0.135, y * 0.135, 0.012 + rng.uniform(0, wobble)), mat, bevel=0.006)
            sq.rotation_euler = (rng.uniform(-wobble, wobble) * 4, rng.uniform(-wobble, wobble) * 4, rng.uniform(-0.15, 0.15))
            parts.append(sq)
    return art.join(name, parts)


def burned():
    parts = []
    for i, (x, y, r) in enumerate([(-0.08, -0.04, 0.11), (0.08, 0.02, 0.09), (-0.01, 0.09, 0.08)]):
        lump = art.sphere(f"Lump{i}", r, (x, y, r * 0.6), "Char", segments=7, rings=5, scale=(1, 0.9, 0.7))
        parts.append(lump)
    for i in range(5):
        parts.append(art.box(f"Ember{i}", (0.025, 0.025, 0.02), (rng.uniform(-0.12, 0.12), rng.uniform(-0.1, 0.12), rng.uniform(0.09, 0.13)), "Ember"))
    return art.join("Burned", parts)


art.reset_scene()
models = {
    "Food_Meat": meat_raw(),
    "Food_MeatChopped": chunks("MeatChopped", "MeatRaw", None, 6, 0.11, 0.14),
    "Food_MeatBoiled": chunks("MeatBoiled", "MeatCooked", None, 6, 0.105, 0.14),
    "Food_Onion": onion(),
    "Food_OnionSliced": onion_sliced(),
    "Food_Dough": dough(),
    "Food_DoughCut": dough_squares("DoughCut", "Dough", 0.12, 0.0),
    "Food_DoughBoiled": dough_squares("DoughBoiled", "DoughCooked", 0.13, 0.02),
    "Food_Burned": burned(),
}
for name, obj in models.items():
    art.export_fbx(name, [obj])

for i, obj in enumerate(models.values()):
    obj.location.x += (i % 5) * 0.6 - 1.2
    obj.location.y += (i // 5) * 0.6
art.save_source("Food")
art.render_preview(os.path.join(PREVIEW_DIR, "food_preview.png"), target=(0, 0.3, 0.1), distance=3.0, elevation_deg=45, azimuth_deg=0)
print("NC_DONE")
