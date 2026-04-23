import bpy
import sys

output_path = sys.argv[sys.argv.index("--") + 1]

# Clear default scene
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete()

def add_cube(name, location, scale):
    bpy.ops.mesh.primitive_cube_add(size=1, location=location)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = scale
    return obj

def add_sphere(name, location, radius):
    bpy.ops.mesh.primitive_uv_sphere_add(radius=radius, location=location, segments=16, ring_count=8)
    obj = bpy.context.active_object
    obj.name = name
    return obj

def add_cone(name, location, radius, depth):
    bpy.ops.mesh.primitive_cone_add(radius1=radius, radius2=0, depth=depth, location=location, vertices=8)
    obj = bpy.context.active_object
    obj.name = name
    return obj

# Agumon is a small stocky dinosaur, roughly 1m tall

# Body (round and chunky)
body = add_sphere("Body", (0, 0, 0.45), 0.25)
body.scale = (0.28, 0.22, 0.3)

# Head (large relative to body)
head = add_sphere("Head", (0, 0, 0.85), 0.22)

# Snout
snout = add_cube("Snout", (0, 0.15, 0.8), (0.12, 0.12, 0.08))

# Eyes
eye_l = add_sphere("Eye_L", (0.1, 0.15, 0.9), 0.04)
eye_r = add_sphere("Eye_R", (-0.1, 0.15, 0.9), 0.04)

# Head crest (three small horns/bumps on back of head)
crest1 = add_cone("Crest1", (0, -0.15, 1.05), 0.04, 0.1)
crest2 = add_cone("Crest2", (0.07, -0.12, 1.0), 0.03, 0.08)
crest3 = add_cone("Crest3", (-0.07, -0.12, 1.0), 0.03, 0.08)

# Arms (short and stubby)
arm_l = add_cube("Arm_L", (0.28, 0, 0.55), (0.1, 0.08, 0.18))
arm_r = add_cube("Arm_R", (-0.28, 0, 0.55), (0.1, 0.08, 0.18))

# Claws on hands (3 per hand)
for i, offset in enumerate([-0.03, 0, 0.03]):
    claw = add_cone(f"ClawL_{i}", (0.33, offset, 0.43), 0.015, 0.06)
    claw.rotation_euler.x = 3.14159
for i, offset in enumerate([-0.03, 0, 0.03]):
    claw = add_cone(f"ClawR_{i}", (-0.33, offset, 0.43), 0.015, 0.06)
    claw.rotation_euler.x = 3.14159

# Legs (thick and short)
leg_l = add_cube("Leg_L", (0.12, 0, 0.15), (0.12, 0.1, 0.2))
leg_r = add_cube("Leg_R", (-0.12, 0, 0.15), (0.12, 0.1, 0.2))

# Feet (big flat feet)
foot_l = add_cube("Foot_L", (0.12, 0.06, 0.04), (0.14, 0.2, 0.06))
foot_r = add_cube("Foot_R", (-0.12, 0.06, 0.04), (0.14, 0.2, 0.06))

# Toe claws
for i, x_off in enumerate([-0.04, 0.0, 0.04]):
    claw = add_cone(f"ToeClawL_{i}", (0.12 + x_off, 0.18, 0.04), 0.015, 0.05)
    claw.rotation_euler.x = 1.5708
for i, x_off in enumerate([-0.04, 0.0, 0.04]):
    claw = add_cone(f"ToeClawR_{i}", (-0.12 + x_off, 0.18, 0.04), 0.015, 0.05)
    claw.rotation_euler.x = 1.5708

# Tail (short and thick)
tail = add_cone("Tail", (0, -0.22, 0.3), 0.08, 0.25)
tail.rotation_euler.x = -0.6

# Apply all transforms
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)

# Join all into one mesh
bpy.ops.object.join()
bpy.context.active_object.name = "Agumon"

# Move mesh so feet are at Z=0 with origin at feet
obj = bpy.context.active_object
bbox = obj.bound_box
min_z = min(v[2] for v in bbox)
for v in obj.data.vertices:
    v.co.z -= min_z

# Place the object at world origin (origin = feet)
obj.location = (0, 0, 0)
bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)

# Export FBX
bpy.ops.export_scene.fbx(
    filepath=output_path,
    use_selection=True,
    apply_scale_options='FBX_SCALE_ALL',
    axis_forward='-Z',
    axis_up='Y',
    use_space_transform=True,
    bake_space_transform=True,
    mesh_smooth_type='OFF',
)

print(f"Exported to {output_path}")
