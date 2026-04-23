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

# Body parts (units roughly in meters, character ~1.8m tall)

# Torso
torso = add_cube("Torso", (0, 0, 1.1), (0.4, 0.2, 0.5))

# Head
head = add_sphere("Head", (0, 0, 1.65), 0.15)

# Upper arms
upper_arm_l = add_cube("UpperArm_L", (0.35, 0, 1.25), (0.12, 0.12, 0.25))
upper_arm_r = add_cube("UpperArm_R", (-0.35, 0, 1.25), (0.12, 0.12, 0.25))

# Lower arms
lower_arm_l = add_cube("LowerArm_L", (0.35, 0, 0.95), (0.1, 0.1, 0.22))
lower_arm_r = add_cube("LowerArm_R", (-0.35, 0, 0.95), (0.1, 0.1, 0.22))

# Hands
hand_l = add_sphere("Hand_L", (0.35, 0, 0.78), 0.06)
hand_r = add_sphere("Hand_R", (-0.35, 0, 0.78), 0.06)

# Hips
hips = add_cube("Hips", (0, 0, 0.75), (0.35, 0.18, 0.15))

# Upper legs
upper_leg_l = add_cube("UpperLeg_L", (0.12, 0, 0.5), (0.14, 0.14, 0.3))
upper_leg_r = add_cube("UpperLeg_R", (-0.12, 0, 0.5), (0.14, 0.14, 0.3))

# Lower legs
lower_leg_l = add_cube("LowerLeg_L", (0.12, 0, 0.2), (0.11, 0.11, 0.28))
lower_leg_r = add_cube("LowerLeg_R", (-0.12, 0, 0.2), (0.11, 0.11, 0.28))

# Feet
foot_l = add_cube("Foot_L", (0.12, 0.05, 0.04), (0.12, 0.2, 0.06))
foot_r = add_cube("Foot_R", (-0.12, 0.05, 0.04), (0.12, 0.2, 0.06))

# Apply all transforms
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)

# Join all into one mesh
bpy.ops.object.join()
bpy.context.active_object.name = "PlayerModel"

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
