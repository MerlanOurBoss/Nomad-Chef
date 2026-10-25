"""The chef, built from separate parts so Unity can animate it in code (ChefAnimator).

All parts are exported flat under one root with their origin at the feet (the export path
that converts axes reliably). Unity's Player prefab adds the joints itself: pivots at the
hips, neck and shoulders, using the joint positions below. The rest pose has the arms down.

    LegL / LegR      trouser leg + boot            (pivot: hip)
    Robe             chapan body, "Cloth" only      (pivot: hips, tinted per player)
    RobeDetails      hem, belt, buckle, apron, collar
    Head             face, hair, kalpak hat         (pivot: neck)
    SleeveL / R      "Cloth" only                   (pivot: shoulder, tinted)
    HandL / R        hand + gold cuff               (pivot: shoulder)

"L" is the chef's own left. Tinted parts are single-material because Unity tints a whole renderer.
"""
import math
import os
import sys

sys.dont_write_bytecode = True

import bpy
from mathutils import Vector

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import nc_art as art

PREVIEW_DIR = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else art.SOURCE_DIR

HIP_Z = 0.45
NECK_Z = 1.16
SHOULDER = (0.25, 0.0, 1.08)


def limb(name, start, end, radius, mat_name, vertices=8):
    """Cylinder from start to end."""
    start, end = Vector(start), Vector(end)
    obj = art.cylinder(name, radius, (end - start).length, (start + end) / 2, mat_name, vertices=vertices)
    obj.rotation_euler = (end - start).to_track_quat("Z", "Y").to_euler()
    return obj


def robe_radius(z):
    # The robe is a cone from r=0.36 at z=0.35 to r=0.26 at z=1.15.
    return 0.36 - (z - 0.35) / 0.8 * 0.10


def part(name, objects, pivot=None):
    """Join objects into one mesh at the world origin. The pivot is documentation for Unity's joints."""
    joined = art.join(name, objects)
    joined.parent = root
    return joined


art.reset_scene()
root = bpy.data.objects.new("Chef", None)
bpy.context.scene.collection.objects.link(root)

# --- Legs ------------------------------------------------------------------
for sx, side in ((1, "L"), (-1, "R")):
    hip = (sx * 0.11, 0, HIP_Z)
    leg = part(f"Leg{side}", [
        art.cylinder(f"Trouser{side}", 0.075, 0.34, (sx * 0.11, 0, 0.29), "FeltBrown", vertices=8),
        art.box(f"Boot{side}", (0.13, 0.22, 0.16), (sx * 0.11, -0.03, 0.08), "Boots", bevel=0.02),
    ], hip)

# --- Torso -------------------------------------------------------------------
robe = part("Robe", [art.cone("RobeBody", 0.36, 0.26, 0.8, (0, 0, 0.75), "Cloth", vertices=14)], (0, 0, HIP_Z))

belt_z = 0.72
apron_top, apron_bottom = 0.7, 0.4
apron = art.box("Apron", (0.34, 0.025, apron_top - apron_bottom), (0, 0, 0), "Apron")
apron.location = (0, -(robe_radius((apron_top + apron_bottom) / 2) + 0.018), (apron_top + apron_bottom) / 2)
apron.rotation_euler = (math.atan(0.1 / 0.8), 0, 0)
details = part("RobeDetails", [
    art.cylinder("Hem", robe_radius(0.37) + 0.012, 0.05, (0, 0, 0.37), "Gold", vertices=14),
    art.cylinder("Belt", robe_radius(belt_z) + 0.015, 0.07, (0, 0, belt_z), "Gold", vertices=14),
    art.box("Buckle", (0.08, 0.02, 0.08), (0, -robe_radius(belt_z) - 0.025, belt_z), "Turquoise"),
    apron,
    art.torus("Collar", 0.17, 0.035, (0, 0, 1.16), "Gold", major_segments=14, minor_segments=5),
], (0, 0, HIP_Z))

# --- Head --------------------------------------------------------------------
head_z = 1.34
hat_base = head_z + 0.13
head_parts = [art.sphere("HeadBall", 0.19, (0, 0, head_z), "Skin", segments=14, rings=10, scale=(1, 0.95, 1.05))]
for sx in (-1, 1):
    head_parts.append(art.box(f"Eye{sx}", (0.035, 0.02, 0.06), (sx * 0.07, -0.178, head_z + 0.03), "Hair"))
    head_parts.append(art.sphere(f"Ear{sx}", 0.04, (sx * 0.19, 0, head_z), "Skin", segments=6, rings=4))
head_parts.append(art.box("Mustache", (0.14, 0.03, 0.03), (0, -0.183, head_z - 0.06), "Hair", bevel=0.01))
head_parts.append(art.box("HairBack", (0.34, 0.12, 0.14), (0, 0.09, head_z + 0.02), "Hair", bevel=0.03))
head_parts.append(art.cone("Kalpak", 0.2, 0.11, 0.34, (0, 0, hat_base + 0.17), "HatFelt", vertices=12))
head_parts.append(art.sphere("KalpakTop", 0.04, (0, 0, hat_base + 0.35), "Hair", segments=6, rings=4))
for sy in (-1, 1):
    brim = art.box(f"Brim{sy}", (0.34, 0.03, 0.13), (0, sy * 0.18, hat_base + 0.06), "Hair", bevel=0.01)
    brim.rotation_euler = (math.radians(-12 * sy), 0, 0)
    head_parts.append(brim)
head = part("Head", head_parts, (0, 0, NECK_Z))

# --- Arms (rest pose: hanging down, slightly forward) ------------------------
for sx, side in ((1, "L"), (-1, "R")):
    shoulder = (sx * SHOULDER[0], SHOULDER[1], SHOULDER[2])
    elbow = (sx * 0.29, -0.02, 0.87)
    wrist = (sx * 0.30, -0.05, 0.69)
    hand_c = (sx * 0.30, -0.06, 0.64)
    sleeve = part(f"Sleeve{side}", [
        art.sphere(f"ShoulderBall{side}", 0.09, shoulder, "Cloth", segments=8, rings=6),
        limb(f"Upper{side}", shoulder, elbow, 0.075, "Cloth"),
        limb(f"Fore{side}", elbow, wrist, 0.07, "Cloth"),
    ], shoulder)
    cuff = limb(f"Cuff{side}", Vector(wrist) + (Vector(elbow) - Vector(wrist)).normalized() * 0.02,
                Vector(wrist) - (Vector(elbow) - Vector(wrist)).normalized() * 0.03, 0.078, "Gold")
    hand = part(f"Hand{side}", [art.sphere(f"Palm{side}", 0.06, hand_c, "Skin", segments=8, rings=6), cuff], shoulder)

art.export_fbx("Chef", [root])
art.save_source("Chef")
art.render_preview(os.path.join(PREVIEW_DIR, "chef_front.png"), target=(0, 0, 0.9), distance=3.2, elevation_deg=12, azimuth_deg=-25)
print("NC_DONE")
