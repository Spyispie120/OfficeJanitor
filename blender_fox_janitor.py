"""Generate a low-poly, Webfishing-inspired fox janitor for Blender 5.2+.

Run from Blender's Scripting workspace or with:
blender --background --python blender_fox_janitor.py
"""

import math

import bpy
from mathutils import Vector


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for datablocks in (bpy.data.meshes, bpy.data.curves, bpy.data.materials):
        for datablock in datablocks:
            if datablock.users == 0:
                datablocks.remove(datablock)


clear_scene()


def material(name, color, metallic=0.0, roughness=0.75):
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    mat.diffuse_color = (*color, 1.0)
    principled = mat.node_tree.nodes.get("Principled BSDF")
    principled.inputs["Roughness"].default_value = roughness
    principled.inputs["Metallic"].default_value = metallic
    # Constant color steps make the procedural texture read as chunky pixel patches.
    noise = mat.node_tree.nodes.new("ShaderNodeTexNoise")
    noise.noise_dimensions = "3D"
    noise.inputs["Scale"].default_value = 5.0
    noise.inputs["Detail"].default_value = 0.0
    ramp = mat.node_tree.nodes.new("ShaderNodeValToRGB")
    ramp.color_ramp.interpolation = "CONSTANT"
    ramp.color_ramp.elements[0].position = 0.46
    ramp.color_ramp.elements[0].color = (*(channel * 0.72 for channel in color), 1.0)
    ramp.color_ramp.elements[1].position = 0.54
    ramp.color_ramp.elements[1].color = (
        *(min(1.0, channel * 1.08) for channel in color), 1.0
    )
    mat.node_tree.links.new(noise.outputs["Fac"], ramp.inputs["Fac"])
    mat.node_tree.links.new(ramp.outputs["Color"], principled.inputs["Base Color"])
    return mat


FUR_ORANGE = material("Fox orange", (0.88, 0.24, 0.055))
FUR_DARK = material("Fox dark fur", (0.20, 0.065, 0.025))
FUR_CREAM = material("Fox cream", (0.95, 0.70, 0.39))
JUMPSUIT_BLUE = material("Janitor jumpsuit", (0.035, 0.29, 0.53))
JUMPSUIT_TRIM = material("Jumpsuit trim", (0.018, 0.11, 0.23))
EYE_BLACK = material("Eyes", (0.008, 0.006, 0.004))
BUCKET_GRAY = material("Bucket", (0.32, 0.40, 0.42), metallic=0.25)
WATER_BLUE = material("Bucket water", (0.05, 0.48, 0.66), metallic=0.15)
MOP_WOOD = material("Mop handle", (0.37, 0.16, 0.045))
MOP_CLOTH = material("Mop cloth", (0.87, 0.80, 0.59))


def finish(obj, name, mat):
    obj.name = name
    obj.data.materials.append(mat)
    for polygon in obj.data.polygons:
        polygon.use_smooth = False
    return obj


def fuse_jumpsuit(parts):
    """Voxel-remesh intersecting uniform pieces into one watertight volume."""
    uniform_parts = [
        part for part in parts
        if (
            part.type == "MESH"
            and part.data.materials[0] == JUMPSUIT_BLUE
            and "leg" not in part.name.lower()
        )
    ]
    bpy.ops.object.select_all(action="DESELECT")
    for part in uniform_parts:
        part.select_set(True)
    bpy.context.view_layer.objects.active = uniform_parts[0]
    bpy.ops.object.join()
    jumpsuit = bpy.context.object
    jumpsuit.name = "FoxJanitor_ContinuousJumpsuit"
    remesh = jumpsuit.modifiers.new("Fuse torso and arms", "REMESH")
    remesh.mode = "VOXEL"
    remesh.voxel_size = 0.075
    remesh.use_smooth_shade = False
    bpy.ops.object.modifier_apply(modifier=remesh.name)
    return [part for part in parts if part not in uniform_parts] + [jumpsuit]


def join_character(parts):
    """Join the continuous uniform and the remaining character geometry."""
    mesh_parts = [part for part in parts if part.type == "MESH"]
    bpy.ops.object.select_all(action="DESELECT")
    for part in mesh_parts:
        part.select_set(True)
    bpy.context.view_layer.objects.active = mesh_parts[0]
    bpy.ops.object.join()
    character = bpy.context.object
    character.name = "FoxJanitor_Character"
    return character


def cube(name, location, scale, mat, rotation=(0, 0, 0)):
    bpy.ops.mesh.primitive_cube_add(location=location, rotation=rotation)
    obj = bpy.context.object
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return finish(obj, name, mat)


def low_poly_sphere(name, location, scale, mat):
    bpy.ops.mesh.primitive_uv_sphere_add(
        segments=8, ring_count=4, location=location
    )
    obj = bpy.context.object
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return finish(obj, name, mat)


def cone(name, location, radius1, radius2, depth, mat, rotation=(0, 0, 0), vertices=6):
    bpy.ops.mesh.primitive_cone_add(
        vertices=vertices,
        radius1=radius1,
        radius2=radius2,
        depth=depth,
        location=location,
        rotation=rotation,
    )
    return finish(bpy.context.object, name, mat)


def rod(name, start, end, radius, mat, vertices=8):
    start, end = Vector(start), Vector(end)
    direction = end - start
    midpoint = (start + end) / 2
    bpy.ops.mesh.primitive_cylinder_add(
        vertices=vertices, radius=radius, depth=direction.length, location=midpoint
    )
    obj = bpy.context.object
    obj.rotation_mode = "QUATERNION"
    obj.rotation_quaternion = Vector((0, 0, 1)).rotation_difference(direction)
    obj.rotation_mode = "XYZ"
    return finish(obj, name, mat)


def tapered_rod(name, start, end, radius_start, radius_end, mat, vertices=7):
    """Create a cone whose wide root is embedded at start and narrows toward end."""
    start, end = Vector(start), Vector(end)
    direction = end - start
    bpy.ops.mesh.primitive_cone_add(
        vertices=vertices,
        radius1=radius_start,
        radius2=radius_end,
        depth=direction.length,
        location=(start + end) / 2,
    )
    obj = bpy.context.object
    obj.rotation_mode = "QUATERNION"
    obj.rotation_quaternion = Vector((0, 0, 1)).rotation_difference(direction)
    obj.rotation_mode = "XYZ"
    return finish(obj, name, mat)


def create_fox_janitor():
    # Blender coordinates: the character looks toward negative Y.
    before = list(bpy.context.scene.objects)
    low_poly_sphere("Body", (0, 0, 2.30), (0.64, 0.43, 0.90), JUMPSUIT_BLUE)
    low_poly_sphere("Left shoulder", (-0.55, 0, 2.55), (0.24, 0.25, 0.28), JUMPSUIT_BLUE)
    low_poly_sphere("Right shoulder", (0.55, 0, 2.55), (0.24, 0.25, 0.28), JUMPSUIT_BLUE)
    cube("Jumpsuit collar", (0, -0.40, 2.82), (0.46, 0.09, 0.12), JUMPSUIT_TRIM)
    cube("Belt", (0, 0, 2.00), (0.64, 0.44, 0.09), JUMPSUIT_TRIM)
    cube("Left pocket", (-0.33, -0.42, 2.34), (0.15, 0.05, 0.16), JUMPSUIT_TRIM)
    cube("Right pocket", (0.33, -0.42, 2.34), (0.15, 0.05, 0.16), JUMPSUIT_TRIM)

    # Splayed legs keep a clear gap between the feet and reach the ground,
    # with their tops buried in the torso so nothing floats.
    cube("Left jumpsuit leg", (-0.34, 0, 1.15), (0.20, 0.29, 0.75), JUMPSUIT_BLUE)
    cube("Right jumpsuit leg", (0.34, 0, 1.15), (0.20, 0.29, 0.75), JUMPSUIT_BLUE)
    cube("Left boot", (-0.37, -0.12, 0.20), (0.25, 0.40, 0.20), FUR_DARK)
    cube("Right boot", (0.37, -0.12, 0.20), (0.25, 0.40, 0.20), FUR_DARK)

    # Angled arms and mitten hands.
    rod("Left sleeve", (-0.54, 0, 2.55), (-1.16, -0.20, 1.98), 0.14, JUMPSUIT_BLUE, 6)
    rod("Right sleeve", (0.54, 0, 2.55), (1.16, -0.12, 2.02), 0.14, JUMPSUIT_BLUE, 6)
    low_poly_sphere("Left hand", (-1.19, -0.21, 1.94), (0.21, 0.17, 0.21), FUR_ORANGE)
    low_poly_sphere("Right hand", (1.20, -0.13, 1.98), (0.21, 0.17, 0.21), FUR_ORANGE)

    # Oversized chibi head, cream muzzle, and simple large pixel-like eyes.
    low_poly_sphere("Head", (0, -0.03, 3.84), (1.00, 0.76, 0.86), FUR_ORANGE)
    cone("Muzzle", (0, -0.72, 3.58), 0.43, 0.25, 0.44, FUR_CREAM, rotation=(math.pi / 2, 0, 0))
    cone("Nose", (0, -0.97, 3.58), 0.12, 0.02, 0.16, EYE_BLACK, rotation=(math.pi / 2, 0, 0))
    cube("Left eye", (-0.36, -0.68, 3.96), (0.13, 0.055, 0.20), EYE_BLACK)
    cube("Right eye", (0.36, -0.68, 3.96), (0.13, 0.055, 0.20), EYE_BLACK)
    cube("Left eye shine", (-0.40, -0.74, 4.06), (0.04, 0.015, 0.05), FUR_CREAM)
    cube("Right eye shine", (0.32, -0.74, 4.06), (0.04, 0.015, 0.05), FUR_CREAM)

    # Wide-based ears are buried deep in the skull and tilted outward so they
    # read as growing out of the head rather than floating beside it.
    cone("Left ear", (-0.52, 0.02, 4.48), 0.44, 0.03, 1.05, FUR_ORANGE, rotation=(0, -0.22, 0), vertices=4)
    cone("Right ear", (0.52, 0.02, 4.48), 0.44, 0.03, 1.05, FUR_ORANGE, rotation=(0, 0.22, 0), vertices=4)
    cone("Left inner ear", (-0.50, -0.20, 4.50), 0.20, 0.01, 0.62, FUR_DARK, rotation=(0, -0.22, 0), vertices=4)
    cone("Right inner ear", (0.50, -0.20, 4.50), 0.20, 0.01, 0.62, FUR_DARK, rotation=(0, 0.22, 0), vertices=4)

    # The root is sunk fully inside the torso so only the curve of the tail
    # emerges from the lower back, with a cream tip like the reference fox.
    tapered_rod("Tail", (0, 0.05, 2.10), (0, 0.80, 1.32), 0.24, 0.13, FUR_ORANGE)
    tapered_rod("Tail tip", (0, 0.68, 1.46), (0, 0.95, 1.18), 0.15, 0.07, FUR_CREAM)
    return [obj for obj in bpy.context.scene.objects if obj not in before]


def create_cleaning_tools():
    rod("Mop handle", (-1.19, -0.21, 1.94), (-2.10, -1.10, 0.42), 0.055, MOP_WOOD, 8)
    cube("Mop head", (-2.10, -1.10, 0.28), (0.45, 0.20, 0.10), MOP_CLOTH)
    for index, offset in enumerate((-0.33, -0.16, 0.0, 0.16, 0.33)):
        cube(
            f"Mop strand {index + 1}",
            (-2.10 + offset, -1.13, 0.12),
            (0.07, 0.16, 0.22),
            MOP_CLOTH,
            rotation=(0, 0.16 * offset, 0),
        )

    cone("Bucket", (1.48, -0.43, 0.63), 0.48, 0.40, 0.72, BUCKET_GRAY, vertices=10)
    cone("Bucket water", (1.48, -0.43, 0.99), 0.37, 0.37, 0.025, WATER_BLUE, vertices=10)
    bpy.ops.mesh.primitive_torus_add(
        major_radius=0.53,
        minor_radius=0.035,
        major_segments=8,
        minor_segments=4,
        location=(1.48, -0.43, 1.15),
        rotation=(math.pi / 2, 0, 0),
    )
    finish(bpy.context.object, "Bucket handle", BUCKET_GRAY)


def create_presentation():
    cube("Display base", (0, 0, -0.12), (3.4, 3.0, 0.12), material("Base", (0.08, 0.10, 0.12)))
    bpy.ops.object.light_add(type="AREA", location=(2.5, -4.0, 6.5))
    bpy.context.object.data.energy = 800
    bpy.context.object.data.shape = "DISK"
    bpy.context.object.data.size = 5
    bpy.ops.object.light_add(type="AREA", location=(-4, -1, 3.5))
    bpy.context.object.data.energy = 400
    bpy.context.object.data.size = 3


character_parts = create_fox_janitor()
character_parts = fuse_jumpsuit(character_parts)
join_character(character_parts)
create_cleaning_tools()
create_presentation()

bpy.context.scene.world.color = (0.04, 0.05, 0.07)
# EEVEE Next replaced the old EEVEE enum in Blender 4.2; fall back as needed.
try:
    bpy.context.scene.render.engine = "BLENDER_EEVEE_NEXT"
except TypeError:
    bpy.context.scene.render.engine = "BLENDER_EEVEE"
try:
    bpy.context.scene.view_settings.look = "AgX - Medium High Contrast"
except TypeError:
    pass
for area in bpy.context.screen.areas:
    if area.type == "VIEW_3D":
        area.spaces.active.shading.type = "MATERIAL"
bpy.context.scene["model_description"] = "Low-poly fox janitor with mop and bucket"
print("Created low-poly fox janitor model.")
