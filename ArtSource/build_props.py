"""Kitchen props: counter, selection rim, crate, cutting board + knife, hearth + kazan.

All props are 1x1 m, origin at the bottom centre, worktop at exactly 1.0 m so the
gameplay attach points in Unity stay where they are. The side facing the game
camera is +Y here (it becomes -Z in Unity).
"""
import math
import os
import sys

sys.dont_write_bytecode = True

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import nc_art as art

PREVIEW_DIR = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else art.SOURCE_DIR

SIDES = [(0, -1, 0.0), (0, 1, 0.0), (1, 0, 90.0), (-1, 0, 90.0)]  # (nx, ny, z-rotation)


def side_panels(prefix, kind):
    """Decoration on all four faces of a 1 m cabinet: felt ornament panels or crate slats."""
    parts = []
    for i, (nx, ny, rot) in enumerate(SIDES):
        off = 0.478
        rz = math.radians(rot)
        if kind == "ornament":
            parts.append(art.box(f"{prefix}Panel{i}", (0.58, 0.02, 0.52), (nx * off, ny * off, 0.5), "FeltRed", rotation=(0, 0, rz)))
            parts.append(art.box(f"{prefix}Border{i}", (0.64, 0.012, 0.58), (nx * (off - 0.004), ny * (off - 0.004), 0.5), "Gold", rotation=(0, 0, rz)))
            # Diamond medallion, a simplified 'koshkar muiz' centre.
            diamond_rot = (math.radians(90) if ny else 0, math.radians(90) if nx else 0, 0)
            d = art.box(f"{prefix}Diamond{i}", (0.16, 0.16, 0.02), (nx * (off + 0.012), ny * (off + 0.012), 0.5), "Gold")
            d.rotation_euler = (math.radians(90), math.radians(45), rz)
            parts.append(d)
            for sx in (-1, 1):
                horn = art.box(f"{prefix}Horn{i}{sx}", (0.1, 0.02, 0.05), (0, 0, 0), "Gold")
                lateral = 0.17 * sx
                horn.location = (nx * (off + 0.012) + (lateral if ny else 0), ny * (off + 0.012) + (lateral if nx else 0), 0.5)
                horn.rotation_euler = (0, 0, rz)
                parts.append(horn)
        else:  # crate slats
            for z in (0.22, 0.5, 0.78):
                parts.append(art.box(f"{prefix}Slat{i}{z}", (0.9, 0.03, 0.12), (nx * 0.478, ny * 0.478, z), "WoodDark", bevel=0.01, rotation=(0, 0, rz)))
    return parts


def cabinet(name, kind, body_mat="WoodLight"):
    parts = [
        art.box(name + "Plinth", (0.9, 0.9, 0.08), (0, 0, 0.04), "WoodDark"),
        art.box(name + "Body", (0.94, 0.94, 0.86), (0, 0, 0.5), body_mat, bevel=0.02),
        art.box(name + "Top", (1.0, 1.0, 0.08), (0, 0, 0.96), "WoodTop", bevel=0.015),
    ]
    for sx in (-1, 1):
        for sy in (-1, 1):
            parts.append(art.box(f"{name}Post{sx}{sy}", (0.08, 0.08, 0.86), (sx * 0.455, sy * 0.455, 0.5), "WoodDark", bevel=0.01))
    parts += side_panels(name, kind)
    return art.join(name, parts)


def selection_rim():
    w, h, outer = 0.05, 0.03, 1.04
    parts = []
    for i, (x, y, sx, sy) in enumerate([(0, outer / 2 - w / 2, outer, w), (0, -outer / 2 + w / 2, outer, w),
                                        (outer / 2 - w / 2, 0, w, outer), (-outer / 2 + w / 2, 0, w, outer)]):
        parts.append(art.box(f"Rim{i}", (sx, sy, h), (x, y, 1.0 + h / 2), "Highlight"))
    return art.join("SelectRim", parts)


def cutting_board():
    board = art.box("BoardSlab", (0.78, 0.58, 0.05), (0, 0, 1.025), "WoodLight", bevel=0.02)
    inlay = art.box("BoardInlay", (0.7, 0.03, 0.006), (0, 0.24, 1.051), "Gold")
    return art.join("CuttingBoard", [board, inlay])


def knife():
    """Origin is the knife pivot; the blade points along +Y (Unity -Z) like the gameplay prefab expects."""
    handle = art.box("KnifeHandle", (0.045, 0.13, 0.04), (0, -0.065, 0), "WoodDark", bevel=0.008)
    cap = art.box("KnifeCap", (0.05, 0.02, 0.045), (0, -0.13, 0), "Gold")
    bolster = art.box("KnifeBolster", (0.05, 0.02, 0.05), (0, 0.005, 0), "Gold")
    blade = art.box("KnifeBlade", (0.012, 0.27, 0.075), (0, 0.15, 0.008), "Metal")
    tip = art.box("KnifeTip", (0.012, 0.06, 0.06), (0, 0.28, 0.012), "Metal")
    tip.rotation_euler = (math.radians(30), 0, 0)
    return art.join("Knife", [handle, cap, bolster, blade, tip])


def hearth():
    parts = [
        art.box("HearthBody", (0.96, 0.96, 0.86), (0, 0, 0.43), "Stone", bevel=0.05),
        art.box("HearthTop", (1.0, 1.0, 0.1), (0, 0, 0.91), "Clay", bevel=0.03),
        art.box("HearthBand", (0.98, 0.98, 0.06), (0, 0, 0.12), "Clay"),
        # Fire opening on the camera side (+Y) and the back.
    ]
    for ny in (1, -1):
        parts.append(art.box(f"Opening{ny}", (0.42, 0.04, 0.3), (0, ny * 0.47, 0.42), "Iron"))
        parts.append(art.box(f"Embers{ny}", (0.34, 0.03, 0.08), (0, ny * 0.48, 0.32), "Ember"))
        parts.append(art.box(f"Arch{ny}", (0.5, 0.05, 0.05), (0, ny * 0.475, 0.6), "Clay"))
    return art.join("Hearth", parts)


def kazan():
    bowl = art.cone("KazanBowl", 0.22, 0.4, 0.3, (0, 0, 1.11), "Iron", vertices=16)
    rim = art.torus("KazanRim", 0.4, 0.03, (0, 0, 1.26), "Iron", major_segments=20)
    parts = [bowl, rim]
    for sx in (-1, 1):
        parts.append(art.torus(f"KazanHandle{sx}", 0.07, 0.018, (sx * 0.45, 0, 1.22), "Iron",
                               major_segments=10, minor_segments=4, rotation=(math.radians(90), 0, 0)))
    return art.join("Kazan", parts)


def pan():
    """Frying pan for kuyrdak, sitting on the hearth top (z=0.96); handle points to the side."""
    parts = [
        art.cone("PanBowl", 0.3, 0.36, 0.07, (0, 0, 0.995), "Iron", vertices=18),
        art.torus("PanRim", 0.36, 0.018, (0, 0, 1.03), "Iron", major_segments=20, minor_segments=4),
        art.cylinder("PanNeck", 0.025, 0.16, (0.44, 0, 1.03), "Iron", vertices=6, rotation=(0, math.radians(90), 0)),
        art.cylinder("PanGrip", 0.035, 0.2, (0.6, 0, 1.03), "WoodDark", vertices=8, rotation=(0, math.radians(90), 0)),
    ]
    return art.join("Pan", parts)


def tabaq(name, dirty):
    """Wooden tabaq platter, the traditional dish for beshbarmak. Sits on z=0, surface at ~0.025."""
    parts = [
        art.cylinder(name + "Base", 0.31, 0.025, (0, 0, 0.0125), "WoodLight", vertices=20),
        art.torus(name + "Rim", 0.295, 0.022, (0, 0, 0.03), "WoodDark", major_segments=20, minor_segments=5),
        art.torus(name + "Carving", 0.2, 0.006, (0, 0, 0.026), "Gold", major_segments=18, minor_segments=3),
    ]
    if dirty:
        for i, (x, y, r) in enumerate([(0.07, 0.04, 0.09), (-0.1, -0.05, 0.07), (0.02, -0.12, 0.05)]):
            smear = art.cylinder(f"{name}Smear{i}", r, 0.004, (x, y, 0.027), "Grime", vertices=9)
            smear.scale = (1.0, 0.7, 1.0)
            parts.append(smear)
        for i in range(4):
            parts.append(art.box(f"{name}Crumb{i}", (0.025, 0.02, 0.015), (0.12 - i * 0.07, 0.1 - i * 0.04, 0.032), "Grime"))
    return art.join(name, parts)


def sink_basin():
    bowl = art.cone("Basin", 0.27, 0.38, 0.14, (0, 0, 1.07), "Copper", vertices=20)
    rim = art.torus("BasinRim", 0.38, 0.022, (0, 0, 1.14), "Brass", major_segments=22, minor_segments=4)
    return art.join("SinkBasin", [bowl, rim])


def kumgan():
    """Traditional washing jug, standing in the back corner of the sink counter."""
    x, y, z = 0.36, -0.36, 1.0
    parts = [
        art.sphere("KumganBody", 0.085, (x, y, z + 0.1), "Copper", segments=10, rings=8, scale=(1, 1, 1.25)),
        art.cylinder("KumganNeck", 0.035, 0.08, (x, y, z + 0.24), "Copper", vertices=8),
        art.cone("KumganLid", 0.05, 0.01, 0.06, (x, y, z + 0.31), "Brass", vertices=8),
        art.cylinder("KumganFoot", 0.06, 0.02, (x, y, z + 0.01), "Brass", vertices=10),
    ]
    spout = art.cylinder("KumganSpout", 0.014, 0.16, (x - 0.1, y + 0.02, z + 0.16), "Copper", vertices=6)
    spout.rotation_euler = (0, math.radians(-55), 0)
    parts.append(spout)
    parts.append(art.torus("KumganHandle", 0.06, 0.012, (x + 0.09, y, z + 0.17), "Brass", major_segments=10, minor_segments=3,
                           rotation=(math.radians(90), 0, 0)))
    return art.join("Kumgan", parts)


def dastarkhan():
    """Serving cloth. Single material so the serving window can flash it green/red."""
    return art.join("Dastarkhan", [art.box("Cloth", (0.94, 0.94, 0.016), (0, 0, 1.008), "ClothServe")])


def delivery_decor():
    parts = []
    w = 0.05
    for i, (x, y, sx, sy) in enumerate([(0, 0.42, 0.94, w), (0, -0.42, 0.94, w), (0.42, 0, w, 0.94), (-0.42, 0, w, 0.94)]):
        parts.append(art.box(f"Border{i}", (sx, sy, 0.006), (x, y, 1.019), "Gold"))
    for sx in (-1, 1):
        for sy in (-1, 1):
            d = art.box(f"Corner{sx}{sy}", (0.1, 0.1, 0.008), (sx * 0.42, sy * 0.42, 1.021), "FeltRed")
            d.rotation_euler = (0, 0, math.radians(45))
            parts.append(d)
    # A round loaf of nan and a brass bell to call the waiters.
    parts.append(art.cylinder("Nan", 0.11, 0.035, (-0.3, -0.3, 1.035), "Dough", vertices=14))
    parts.append(art.torus("NanRim", 0.1, 0.018, (-0.3, -0.3, 1.05), "WoodTop", major_segments=14, minor_segments=4))
    parts.append(art.cone("Bell", 0.07, 0.03, 0.09, (0.32, -0.32, 1.06), "Brass", vertices=12))
    parts.append(art.cylinder("BellBase", 0.075, 0.012, (0.32, -0.32, 1.02), "WoodDark", vertices=12))
    parts.append(art.sphere("BellKnob", 0.022, (0.32, -0.32, 1.115), "Brass", segments=6, rings=4))
    return art.join("DeliveryDecor", parts)


def plate_rack():
    parts = []
    for sx in (-1, 1):
        for sy in (-1, 1):
            parts.append(art.box(f"Post{sx}{sy}", (0.05, 0.05, 0.32), (sx * 0.37, sy * 0.37, 1.16), "WoodDark", bevel=0.01))
        for z in (1.1, 1.27):
            parts.append(art.box(f"Rail{sx}{z}", (0.04, 0.74, 0.035), (sx * 0.37, 0, z), "WoodLight"))
    parts.append(art.box("RackBase", (0.82, 0.82, 0.02), (0, 0, 1.01), "WoodLight"))
    return art.join("PlateRack", parts)


def return_tray():
    parts = [art.box("TrayBase", (0.8, 0.7, 0.03), (0, 0, 1.015), "WoodLight", bevel=0.01)]
    for i, (x, y, sx, sy) in enumerate([(0, 0.34, 0.8, 0.03), (0, -0.34, 0.8, 0.03), (0.39, 0, 0.03, 0.7), (-0.39, 0, 0.03, 0.7)]):
        parts.append(art.box(f"TrayRim{i}", (sx, sy, 0.05), (x, y, 1.045), "WoodDark"))
    for sx in (-1, 1):
        parts.append(art.torus(f"TrayHandle{sx}", 0.05, 0.01, (sx * 0.43, 0, 1.05), "Brass", major_segments=8, minor_segments=3,
                               rotation=(math.radians(90), 0, math.radians(90))))
    return art.join("ReturnTray", parts)


def barrel():
    parts = [
        art.cone("BarrelLow", 0.40, 0.46, 0.5, (0, 0, 0.25), "WoodLight", vertices=16),
        art.cone("BarrelHigh", 0.46, 0.40, 0.48, (0, 0, 0.74), "WoodLight", vertices=16),
        art.cylinder("BarrelHole", 0.37, 0.01, (0, 0, 0.985), "Char", vertices=16),
        art.torus("BarrelLip", 0.395, 0.025, (0, 0, 0.98), "WoodDark", major_segments=16, minor_segments=4),
    ]
    for z, r in ((0.12, 0.425), (0.5, 0.465), (0.86, 0.425)):
        parts.append(art.torus(f"Hoop{z}", r, 0.02, (0, 0, z), "Iron", major_segments=16, minor_segments=3))
    for i in range(16):
        a = 2 * math.pi * (i + 0.5) / 16
        s = art.box(f"Stave{i}", (0.012, 0.012, 0.96), (0.455 * math.cos(a), 0.455 * math.sin(a), 0.49), "WoodDark")
        parts.append(s)
    return art.join("Barrel", parts)


def extinguisher():
    """Origin at the bottom; the nozzle points along -Y (Unity +Z) at the spray point (0, 0.45, 0.22) in Unity."""
    parts = [
        art.cylinder("ExtBody", 0.1, 0.36, (0, 0, 0.19), "ExtRed", vertices=14),
        art.sphere("ExtDome", 0.1, (0, 0, 0.37), "ExtRed", segments=14, rings=8, scale=(1, 1, 0.6)),
        art.cylinder("ExtLabel", 0.102, 0.1, (0, 0, 0.2), "Label", vertices=14),
        art.cylinder("ExtBase", 0.105, 0.03, (0, 0, 0.015), "Iron", vertices=14),
        art.cylinder("ExtValve", 0.035, 0.07, (0, 0, 0.46), "Brass", vertices=8),
        art.box("ExtLever", (0.04, 0.16, 0.02), (0, 0.04, 0.51), "Iron"),
    ]
    nozzle = art.cylinder("ExtNozzle", 0.022, 0.18, (0, -0.11, 0.45), "Iron", vertices=8, rotation=(math.radians(90), 0, 0))
    tip = art.cylinder("ExtTip", 0.03, 0.04, (0, -0.21, 0.45), "Brass", vertices=8, rotation=(math.radians(90), 0, 0))
    parts += [nozzle, tip]
    return art.join("Extinguisher", parts)


art.reset_scene()
models = {
    "Counter": cabinet("Counter", "ornament"),
    "Crate": cabinet("Crate", "slats", body_mat="WoodLight"),
    "SelectRim": selection_rim(),
    "CuttingBoard": cutting_board(),
    "Knife": knife(),
    "Hearth": hearth(),
    "Kazan": kazan(),
    "Pan": pan(),
    "Tabaq": tabaq("Tabaq", dirty=False),
    "TabaqDirty": tabaq("TabaqDirty", dirty=True),
    "SinkBasin": sink_basin(),
    "Kumgan": kumgan(),
    "Dastarkhan": dastarkhan(),
    "DeliveryDecor": delivery_decor(),
    "PlateRack": plate_rack(),
    "ReturnTray": return_tray(),
    "Barrel": barrel(),
    "Extinguisher": extinguisher(),
}
for name, obj in models.items():
    art.export_fbx(name, [obj])

# Preview: a grid of the props. Counter-top dressings get a counter underneath.
counter_tops = {"CuttingBoard", "Knife", "Kazan", "SelectRim", "SinkBasin", "Kumgan", "Dastarkhan", "DeliveryDecor",
                "PlateRack", "ReturnTray"}
slots = {"Counter": 0, "Crate": 1, "Hearth": 2, "Kazan": 2, "CuttingBoard": 3, "Knife": 3, "SelectRim": 3,
         "SinkBasin": 4, "Kumgan": 4, "Dastarkhan": 5, "DeliveryDecor": 5, "PlateRack": 6, "ReturnTray": 7,
         "Barrel": 8, "Tabaq": 9, "TabaqDirty": 10, "Extinguisher": 11, "Pan": 12}
for name, obj in models.items():
    slot = slots[name]
    obj.location.x += (slot % 6) * 1.3 - 3.25
    obj.location.y += (slot // 6) * 1.6
models["Knife"].location.z += 1.08
for slot in (4, 5, 6, 7):
    base = cabinet(f"PreviewCounter{slot}", "ornament")
    base.location = ((slot % 6) * 1.3 - 3.25, (slot // 6) * 1.6, 0)
for name in ("Tabaq", "TabaqDirty", "Extinguisher"):
    models[name].location.z += 0.0
art.save_source("Props")
art.render_preview(os.path.join(PREVIEW_DIR, "props_preview.png"), target=(0.3, 1.0, 0.5), distance=12.5, elevation_deg=38, azimuth_deg=0)
print("NC_DONE")
