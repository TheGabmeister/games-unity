"""Generate Small Mario sprite sheet as SVG, then export to PNG via Inkscape."""
import xml.etree.ElementTree as ET

CELL = 64
COLS = 4
ROWS = 12
W = CELL * COLS
H = CELL * ROWS

ANIMATIONS = [
    "idle",       # 1 frame
    "walk",       # 3 frames
    "run",        # 3 frames
    "sprint",     # 3 frames
    "jump",       # 1 frame
    "spin_jump",  # 4 frames
    "crouch",     # 1 frame
    "slide",      # 1 frame
    "skid",       # 1 frame
    "turn",       # 1 frame
    "fall",       # 1 frame
    "look_up",    # 1 frame
]

# Colors
HAT    = "#E52521"  # Mario red
SKIN   = "#FBBE6F"  # skin tone
SHIRT  = HAT
OVERALLS = "#2038EC" # blue overalls
SHOE   = "#6B3A23"  # brown shoes
EYE    = "#000000"
WHITE  = "#FFFFFF"

svg = ET.Element("svg", {
    "xmlns": "http://www.w3.org/2000/svg",
    "width": str(W),
    "height": str(H),
    "viewBox": f"0 0 {W} {H}",
})

# transparent background
ET.SubElement(svg, "rect", {
    "width": str(W), "height": str(H),
    "fill": "none",
})

def px(val):
    return str(round(val, 1))

def draw_small_mario(parent, cx, cy, pose="idle", frame=0):
    """Draw a Small Mario placeholder at center (cx, cy) within a 64x64 cell.

    Small Mario is about 40px tall (head + body + legs).
    Offset downward so feet sit near bottom of cell and hat isn't clipped.
    """
    g = ET.SubElement(parent, "g")

    # Shift center down so the character fits: hat top ~15px above center, feet ~25px below
    cy = cy + 5

    # Base positions relative to center
    # Small Mario proportions: ~16px head, ~14px body, ~10px legs
    head_y = cy - 16
    body_y = cy - 2
    legs_y = cy + 10

    leg_spread = 0
    arm_angle = 0
    body_tilt = 0
    squash_y = 0
    head_offset_x = 0
    show_both_legs = True
    crouch_amount = 0
    look_up_amount = 0
    facing = 1  # 1 = right, -1 = left

    if pose == "idle":
        pass

    elif pose == "walk":
        leg_spread = [6, 0, -6][frame % 3]
        arm_angle = [-10, 0, 10][frame % 3]

    elif pose == "run":
        leg_spread = [10, 0, -10][frame % 3]
        arm_angle = [-20, 0, 20][frame % 3]

    elif pose == "sprint":
        leg_spread = [12, 0, -12][frame % 3]
        arm_angle = [-35, -10, -35][frame % 3]

    elif pose == "jump":
        squash_y = -4
        arm_angle = -30

    elif pose == "spin_jump":
        rotation = [0, 90, 180, 270][frame % 4]
        # Draw a spinning mario - simplified as rotation
        ET.SubElement(g, "g", {
            "transform": f"rotate({rotation},{px(cx)},{px(cy)})"
        })
        # Simplified spinning figure
        # Head
        ET.SubElement(g, "circle", {
            "cx": px(cx), "cy": px(head_y - 2),
            "r": "9",
            "fill": SKIN,
        })
        # Hat
        ET.SubElement(g, "rect", {
            "x": px(cx - 11), "y": px(head_y - 12),
            "width": "22", "height": "7",
            "rx": "2",
            "fill": HAT,
        })
        # Body
        ET.SubElement(g, "rect", {
            "x": px(cx - 8), "y": px(body_y - 2),
            "width": "16", "height": "14",
            "rx": "3",
            "fill": OVERALLS,
        })
        # Limbs spread out for spin
        for angle_offset in [45, 135, 225, 315]:
            a = rotation + angle_offset
            import math
            rad = math.radians(a)
            lx = cx + 16 * math.cos(rad)
            ly = cy + 16 * math.sin(rad)
            ET.SubElement(g, "line", {
                "x1": px(cx), "y1": px(cy),
                "x2": px(lx), "y2": px(ly),
                "stroke": SKIN,
                "stroke-width": "4",
                "stroke-linecap": "round",
            })
        return

    elif pose == "crouch":
        crouch_amount = 8

    elif pose == "slide":
        crouch_amount = 10
        body_tilt = 15

    elif pose == "skid":
        facing = -1
        leg_spread = 8
        body_tilt = -10

    elif pose == "turn":
        facing = -1

    elif pose == "fall":
        squash_y = 3
        arm_angle = 25
        leg_spread = 4

    elif pose == "look_up":
        look_up_amount = -5

    # Apply crouch: lower the head, compress body
    eff_head_y = head_y + crouch_amount + squash_y + look_up_amount
    eff_body_y = body_y + crouch_amount // 2 + squash_y
    eff_legs_y = legs_y + squash_y

    # Transform group for tilt
    transform = ""
    if body_tilt != 0:
        transform = f"rotate({body_tilt},{px(cx)},{px(cy)})"
        g.set("transform", transform)

    # === SHOES ===
    shoe_w, shoe_h = 10, 6
    if show_both_legs:
        # Left shoe
        ET.SubElement(g, "rect", {
            "x": px(cx - 6 - shoe_w//2 - leg_spread * facing),
            "y": px(eff_legs_y + 4),
            "width": str(shoe_w), "height": str(shoe_h),
            "rx": "2",
            "fill": SHOE,
        })
        # Right shoe
        ET.SubElement(g, "rect", {
            "x": px(cx + 6 - shoe_w//2 + leg_spread * facing),
            "y": px(eff_legs_y + 4),
            "width": str(shoe_w), "height": str(shoe_h),
            "rx": "2",
            "fill": SHOE,
        })

    # === LEGS (overalls) ===
    leg_w, leg_h = 8, 12 - crouch_amount // 2
    if leg_h > 2:
        ET.SubElement(g, "rect", {
            "x": px(cx - 6 - leg_w//2 - leg_spread * facing),
            "y": px(eff_legs_y + 4 - leg_h),
            "width": str(leg_w), "height": str(max(leg_h, 2)),
            "fill": OVERALLS,
        })
        ET.SubElement(g, "rect", {
            "x": px(cx + 6 - leg_w//2 + leg_spread * facing),
            "y": px(eff_legs_y + 4 - leg_h),
            "width": str(leg_w), "height": str(max(leg_h, 2)),
            "fill": OVERALLS,
        })

    # === BODY (shirt area) ===
    body_h = 14 - crouch_amount // 2
    ET.SubElement(g, "rect", {
        "x": px(cx - 9), "y": px(eff_body_y - 2),
        "width": "18", "height": str(max(body_h, 4)),
        "rx": "3",
        "fill": SHIRT,
    })
    # Overalls bib
    bib_h = max(body_h - 4, 2)
    ET.SubElement(g, "rect", {
        "x": px(cx - 7), "y": px(eff_body_y + 2),
        "width": "14", "height": str(bib_h),
        "rx": "2",
        "fill": OVERALLS,
    })
    # Overall buttons
    if bib_h > 3:
        for bx in [-3, 3]:
            ET.SubElement(g, "circle", {
                "cx": px(cx + bx), "cy": px(eff_body_y + 4),
                "r": "1.5",
                "fill": "#FFD700",
            })

    # === ARMS ===
    import math
    arm_len = 12
    for side in [-1, 1]:
        a = arm_angle * side
        if pose == "sprint":
            a = arm_angle  # both arms back
        rad = math.radians(a - 90)
        ax = cx + side * 10
        ay = eff_body_y + 2
        ex = ax + arm_len * math.sin(rad) * side
        ey = ay + arm_len * math.cos(rad)
        ET.SubElement(g, "line", {
            "x1": px(ax), "y1": px(ay),
            "x2": px(ex), "y2": px(ey),
            "stroke": SKIN,
            "stroke-width": "5",
            "stroke-linecap": "round",
        })

    # === HEAD ===
    # Face circle
    ET.SubElement(g, "circle", {
        "cx": px(cx + head_offset_x), "cy": px(eff_head_y),
        "r": "10",
        "fill": SKIN,
    })

    # Hat
    # Brim
    ET.SubElement(g, "rect", {
        "x": px(cx + head_offset_x - 13), "y": px(eff_head_y - 9),
        "width": "26", "height": "5",
        "rx": "2",
        "fill": HAT,
    })
    # Cap top
    ET.SubElement(g, "rect", {
        "x": px(cx + head_offset_x - 9), "y": px(eff_head_y - 15),
        "width": "18", "height": "8",
        "rx": "3",
        "fill": HAT,
    })
    # Hat "M" emblem - simplified as a white circle
    ET.SubElement(g, "circle", {
        "cx": px(cx + head_offset_x + 4 * facing), "cy": px(eff_head_y - 11),
        "r": "3",
        "fill": WHITE,
    })

    # Eyes
    eye_x = cx + head_offset_x + 3 * facing
    ET.SubElement(g, "ellipse", {
        "cx": px(eye_x), "cy": px(eff_head_y - 1),
        "rx": "2.5", "ry": "3",
        "fill": WHITE,
    })
    ET.SubElement(g, "ellipse", {
        "cx": px(eye_x + 1 * facing), "cy": px(eff_head_y - 0.5),
        "rx": "1.5", "ry": "2",
        "fill": EYE,
    })

    # Nose
    ET.SubElement(g, "ellipse", {
        "cx": px(cx + head_offset_x + 7 * facing), "cy": px(eff_head_y + 1),
        "rx": "3", "ry": "2.5",
        "fill": "#D4874E",
    })

    # Mustache
    ET.SubElement(g, "ellipse", {
        "cx": px(cx + head_offset_x + 4 * facing), "cy": px(eff_head_y + 4),
        "rx": "7", "ry": "2.5",
        "fill": "#3B1E08",
    })


# Frame counts per animation
FRAME_COUNTS = [1, 3, 3, 3, 1, 4, 1, 1, 1, 1, 1, 1]

for row, (anim, fcount) in enumerate(zip(ANIMATIONS, FRAME_COUNTS)):
    for col in range(fcount):
        cx = col * CELL + CELL // 2
        cy = row * CELL + CELL // 2
        draw_small_mario(svg, cx, cy, pose=anim, frame=col)

tree = ET.ElementTree(svg)
ET.indent(tree, space="  ")

out_path = r"c:\dev\games-unity\SuperMarioWorld\Assets\_Project\Art\Sprites\SmallMario.svg"
tree.write(out_path, xml_declaration=True, encoding="utf-8")
print(f"SVG written to {out_path}")
