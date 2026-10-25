"""Shared helpers for the Nomad Chef art build scripts.

Run a build script with:
    blender --background --factory-startup --python ArtSource/build_<name>.py

Conventions
-----------
* 1 Blender unit = 1 Unity unit (metre). Z is up. Models face -Y in Blender,
  which becomes +Z (forward) in Unity with the export settings below.
* Every material name matches a Unity material "M_Art_<Name>" that the Unity
  importer remaps to, so colours are tuned in one place (the Unity palette).
"""

import math
import os

import bpy
import bmesh

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
MODELS_DIR = os.path.join(ROOT, "Assets", "Art", "Models")
SOURCE_DIR = os.path.join(ROOT, "ArtSource")

# Warm steppe palette (linear-ish sRGB values for the Blender preview only;
# the real colours live in the Unity materials).
PALETTE = {
    "WoodDark": (0.36, 0.20, 0.11),
    "WoodLight": (0.72, 0.50, 0.28),
    "WoodTop": (0.80, 0.60, 0.36),
    "FeltCream": (0.93, 0.87, 0.74),
    "FeltRed": (0.66, 0.16, 0.12),
    "FeltBrown": (0.48, 0.32, 0.20),
    "Gold": (0.88, 0.66, 0.20),
    "Turquoise": (0.16, 0.55, 0.60),
    "Iron": (0.10, 0.10, 0.11),
    "Stone": (0.56, 0.50, 0.45),
    "Clay": (0.70, 0.44, 0.30),
    "Ember": (1.00, 0.45, 0.10),
    "Metal": (0.75, 0.77, 0.80),
    "Skin": (0.86, 0.64, 0.48),
    "Hair": (0.10, 0.07, 0.05),
    "Cloth": (0.85, 0.85, 0.85),   # tinted per player in Unity
    "Apron": (0.96, 0.95, 0.92),
    "Boots": (0.16, 0.10, 0.07),
    "HatFelt": (0.97, 0.95, 0.90),
    "Grass": (0.55, 0.62, 0.33),
    "Highlight": (1.00, 0.92, 0.60),
    # Food
    "MeatRaw": (0.72, 0.16, 0.16),
    "MeatFat": (0.95, 0.86, 0.80),
    "Bone": (0.93, 0.89, 0.80),
    "MeatCooked": (0.62, 0.42, 0.32),
    "OnionSkin": (0.55, 0.25, 0.52),
    "OnionFlesh": (0.93, 0.86, 0.93),
    "Dough": (0.96, 0.88, 0.70),
    "DoughCooked": (0.99, 0.95, 0.84),
    "Char": (0.09, 0.08, 0.07),
    # Props
    "Copper": (0.78, 0.45, 0.25),
    "Brass": (0.85, 0.68, 0.30),
    "Grime": (0.42, 0.30, 0.18),
    "ClothServe": (0.18, 0.55, 0.60),
    "ExtRed": (0.80, 0.12, 0.10),
    "Label": (0.96, 0.94, 0.88),
    # Menu backdrop
    "Mountain": (0.45, 0.50, 0.62),
    "Snow": (0.97, 0.98, 1.00),
    "Hill": (0.48, 0.58, 0.30),
}


def reset_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)


def material(name):
    mat = bpy.data.materials.get(name)
    if mat is None:
        mat = bpy.data.materials.new(name)
        color = PALETTE[name]
        mat.diffuse_color = (*color, 1.0)
        bsdf = mat.node_tree.nodes.get("Principled BSDF") if mat.node_tree else None
        if bsdf is not None:
            bsdf.inputs["Base Color"].default_value = (*color, 1.0)
            bsdf.inputs["Roughness"].default_value = 0.8
    return mat


def _finish(obj, mat_name, bevel=0.0, segments=1):
    obj.data.materials.clear()
    obj.data.materials.append(material(mat_name))
    if bevel > 0:
        mod = obj.modifiers.new("Bevel", "BEVEL")
        mod.width = bevel
        mod.segments = segments
        mod.limit_method = "ANGLE"
    return obj


def box(name, size, location, mat_name, bevel=0.0, rotation=(0, 0, 0)):
    """Axis-aligned box. size=(x, y, z), location is the box centre."""
    bpy.ops.mesh.primitive_cube_add(size=1, location=location, rotation=rotation)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = size
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return _finish(obj, mat_name, bevel)


def cylinder(name, radius, depth, location, mat_name, vertices=12, bevel=0.0, rotation=(0, 0, 0)):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth,
                                        location=location, rotation=rotation)
    obj = bpy.context.active_object
    obj.name = name
    return _finish(obj, mat_name, bevel)


def cone(name, r1, r2, depth, location, mat_name, vertices=12, rotation=(0, 0, 0)):
    bpy.ops.mesh.primitive_cone_add(vertices=vertices, radius1=r1, radius2=r2, depth=depth,
                                    location=location, rotation=rotation)
    obj = bpy.context.active_object
    obj.name = name
    return _finish(obj, mat_name)


def sphere(name, radius, location, mat_name, segments=12, rings=8, scale=(1, 1, 1)):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, radius=radius, location=location)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return _finish(obj, mat_name)


def torus(name, major, minor, location, mat_name, major_segments=16, minor_segments=6, rotation=(0, 0, 0)):
    bpy.ops.mesh.primitive_torus_add(major_radius=major, minor_radius=minor, location=location, rotation=rotation,
                                     major_segments=major_segments, minor_segments=minor_segments)
    obj = bpy.context.active_object
    obj.name = name
    return _finish(obj, mat_name)


def join(name, objects):
    """Join several objects into one multi-material mesh. Modifiers are applied first."""
    for obj in objects:
        bpy.context.view_layer.objects.active = obj
        for mod in list(obj.modifiers):
            bpy.ops.object.modifier_apply(modifier=mod.name)
    bpy.ops.object.select_all(action="DESELECT")
    for obj in objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = objects[0]
    bpy.ops.object.join()
    joined = bpy.context.active_object
    joined.name = name
    joined.data.name = name
    # Bake the transform into the mesh so the object sits at the world origin with no rotation;
    # the FBX exporter mishandles rotated objects under a parent.
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    return joined


def set_origin_to_world_zero(obj):
    cursor = bpy.context.scene.cursor
    cursor.location = (0, 0, 0)
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR")


def export_fbx(filename, objects, bake_space_transform=True):
    """Export the given objects (and their children) to Assets/Art/Models/<filename>.fbx.

    Blender's bake_space_transform mishandles empties, so hierarchies with joint empties
    (the chef) export without it and Unity converts the axes instead ("Bake Axis Conversion",
    set by the Nomad Chef art tool).
    """
    os.makedirs(MODELS_DIR, exist_ok=True)
    bpy.ops.object.select_all(action="DESELECT")
    for obj in objects:
        obj.select_set(True)
        for child in obj.children_recursive:
            child.select_set(True)
    path = os.path.join(MODELS_DIR, filename + ".fbx")
    bpy.ops.export_scene.fbx(
        filepath=path,
        use_selection=True,
        object_types={"MESH", "EMPTY"},
        use_mesh_modifiers=True,
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
        bake_space_transform=bake_space_transform,
        add_leaf_bones=False,
        mesh_smooth_type="FACE",
    )
    return path


def save_source(filename):
    os.makedirs(SOURCE_DIR, exist_ok=True)
    bpy.context.preferences.filepaths.save_version = 0  # no .blend1 backups; the scripts are the source of truth
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(SOURCE_DIR, filename + ".blend"))


def render_preview(path, target=(0, 0, 0.6), distance=4.0, elevation_deg=30, azimuth_deg=-35, ortho_scale=None):
    """Quick Workbench render with material colours, used to check models without Unity."""
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_WORKBENCH"
    scene.display.shading.color_type = "MATERIAL"
    scene.display.shading.light = "STUDIO"
    scene.display.shading.show_cavity = True
    scene.render.resolution_x = 900
    scene.render.resolution_y = 650
    scene.render.film_transparent = False
    world = bpy.data.worlds.new("PreviewWorld")
    world.color = (0.62, 0.72, 0.80)
    scene.world = world

    cam_data = bpy.data.cameras.new("PreviewCam")
    if ortho_scale:
        cam_data.type = "ORTHO"
        cam_data.ortho_scale = ortho_scale
    cam = bpy.data.objects.new("PreviewCam", cam_data)
    scene.collection.objects.link(cam)
    el = math.radians(elevation_deg)
    az = math.radians(azimuth_deg)
    cam.location = (
        target[0] + distance * math.cos(el) * math.sin(az),
        target[1] - distance * math.cos(el) * math.cos(az),
        target[2] + distance * math.sin(el),
    )
    direction = (target[0] - cam.location[0], target[1] - cam.location[1], target[2] - cam.location[2])
    from mathutils import Vector
    cam.rotation_euler = Vector(direction).to_track_quat("-Z", "Y").to_euler()
    scene.camera = cam
    scene.render.filepath = path
    bpy.ops.render.render(write_still=True)
    bpy.data.objects.remove(cam)
