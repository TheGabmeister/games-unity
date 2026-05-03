#!/usr/bin/env python3
"""Generate 8-direction sprite sheet SVGs for all Red Alert units.

Vehicle/Naval/Aircraft: 512x64 (1 row x 8 directions)
Infantry: 512x256 (4 rows x 8 directions: idle, walk1, walk2, fire)

Directions: N, NE, E, SE, S, SW, W, NW (clockwise from north)
All shapes are drawn facing north (up = -Y) and rotated per direction.
"""

import os

CELL = 64
DIRS = 8
SHEET_W = CELL * DIRS  # 512

TOOLS_DIR = os.path.dirname(os.path.abspath(__file__))
OUTPUT_DIR = os.path.join(TOOLS_DIR, "sprites", "units")

# --- SVG helpers ---

def write_svg(name, height, content):
    path = os.path.join(OUTPUT_DIR, f"{name}.svg")
    with open(path, "w") as f:
        f.write(f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {SHEET_W} {height}" width="{SHEET_W}" height="{height}">\n')
        f.write(content)
        f.write("</svg>\n")
    print(f"  {name} ({SHEET_W}x{height})")

def make_vehicle_sheet(name, elements):
    content = ""
    for d in range(DIRS):
        a = d * 45
        cx = d * CELL + CELL // 2
        cy = CELL // 2
        content += f'  <g transform="translate({cx},{cy}) rotate({a})">\n'
        for e in elements:
            content += f"    {e}\n"
        content += "  </g>\n"
    write_svg(name, CELL, content)

def make_infantry_sheet(name, row_funcs):
    """row_funcs: list of 4 element-lists [idle, walk1, walk2, fire]"""
    height = CELL * len(row_funcs)
    content = ""
    for row, elems in enumerate(row_funcs):
        for d in range(DIRS):
            a = d * 45
            cx = d * CELL + CELL // 2
            cy = row * CELL + CELL // 2
            content += f'  <g transform="translate({cx},{cy}) rotate({a})">\n'
            for e in elems:
                content += f"    {e}\n"
            content += "  </g>\n"
    write_svg(name, height, content)

# Shape primitives - coordinates relative to cell center (0,0), north = -Y
def R(cx, cy, w, h, fill, stroke="#707070", rx=0):
    """Rect centered at (cx, cy)."""
    return f'<rect x="{cx-w/2:.1f}" y="{cy-h/2:.1f}" width="{w}" height="{h}" rx="{rx}" fill="{fill}" stroke="{stroke}" stroke-width="1"/>'

def C(cx, cy, r, fill, stroke="#707070"):
    return f'<circle cx="{cx}" cy="{cy}" r="{r}" fill="{fill}" stroke="{stroke}" stroke-width="1"/>'

def E(cx, cy, rx, ry, fill, stroke="#707070"):
    return f'<ellipse cx="{cx}" cy="{cy}" rx="{rx}" ry="{ry}" fill="{fill}" stroke="{stroke}" stroke-width="1"/>'

def L(x1, y1, x2, y2, stroke, sw=1.5):
    return f'<line x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}" stroke="{stroke}" stroke-width="{sw}"/>'

def P(points, fill, stroke="#707070"):
    """Polygon."""
    pts = " ".join(f"{x},{y}" for x, y in points)
    return f'<polygon points="{pts}" fill="{fill}" stroke="{stroke}" stroke-width="1"/>'

# --- Tank template ---
def tank(body_w, body_h, turret_r, barrel_len, barrel_w=3, dual=False):
    tread_w = 7
    elems = [
        # Treads
        R(-body_w/2 - tread_w/2, 2, tread_w, body_h + 4, "#808080", "#606060", 2),
        R(body_w/2 + tread_w/2, 2, tread_w, body_h + 4, "#808080", "#606060", 2),
        # Body
        R(0, 2, body_w, body_h, "#c8c8c8", "#808080", 2),
        # Turret
        C(0, 4, turret_r, "#b0b0b0", "#808080"),
    ]
    if dual:
        elems += [
            R(-barrel_w, -barrel_len/2 - 2, barrel_w, barrel_len, "#909090", "#707070", 1),
            R(barrel_w, -barrel_len/2 - 2, barrel_w, barrel_len, "#909090", "#707070", 1),
        ]
    else:
        elems.append(R(0, -barrel_len/2 - 2, barrel_w, barrel_len, "#909090", "#707070", 1))
    elems.append(C(0, 4, turret_r * 0.35, "#a0a0a0", "#707070"))
    return elems

# --- Tracked vehicle template (no turret) ---
def tracked_body(body_w, body_h, fill="#c8c8c8"):
    tread_w = 7
    return [
        R(-body_w/2 - tread_w/2, 0, tread_w, body_h + 4, "#808080", "#606060", 2),
        R(body_w/2 + tread_w/2, 0, tread_w, body_h + 4, "#808080", "#606060", 2),
        R(0, 0, body_w, body_h, fill, "#808080", 2),
    ]

# --- Wheeled vehicle template ---
def wheeled_body(body_w, body_h, fill="#c8c8c8"):
    return [
        R(0, 0, body_w, body_h, fill, "#808080", 3),
        C(-body_w/2 + 3, -body_h/2 + 4, 3, "#505050", "#404040"),
        C(body_w/2 - 3, -body_h/2 + 4, 3, "#505050", "#404040"),
        C(-body_w/2 + 3, body_h/2 - 4, 3, "#505050", "#404040"),
        C(body_w/2 - 3, body_h/2 - 4, 3, "#505050", "#404040"),
    ]

# --- Ship template ---
def ship_hull(length, width, fill="#808090"):
    half_l = length / 2
    half_w = width / 2
    return [
        P([(0, -half_l), (half_w, -half_l + width), (half_w, half_l - 2),
           (-half_w, half_l - 2), (-half_w, -half_l + width)], fill, "#505060"),
    ]

# --- Infantry template ---
def infantry(row, head_r=6, body_rx=7, body_ry=9, extras=None):
    leg_offset = 0
    if row == 1:
        leg_offset = 3
    elif row == 2:
        leg_offset = -3

    elems = []
    # Legs
    if row == 1:
        elems += [
            R(-4, 13 - 3, 4, 10, "#808080", "#606060", 1),
            R(4, 13 + 3, 4, 10, "#808080", "#606060", 1),
        ]
    elif row == 2:
        elems += [
            R(-4, 13 + 3, 4, 10, "#808080", "#606060", 1),
            R(4, 13 - 3, 4, 10, "#808080", "#606060", 1),
        ]
    else:
        elems += [
            R(-4, 13, 4, 10, "#808080", "#606060", 1),
            R(4, 13, 4, 10, "#808080", "#606060", 1),
        ]
    # Body
    elems.append(E(0, 4, body_rx, body_ry, "#c0c0c0", "#808080"))
    # Head
    elems.append(C(0, -10, head_r, "#d0d0d0", "#808080"))
    # Arms
    if row == 3:  # fire pose - arms forward
        elems += [
            R(-8, -4, 4, 8, "#b0b0b0", "#808080", 1),
            R(8, -4, 4, 8, "#b0b0b0", "#808080", 1),
        ]
    else:
        elems += [
            R(-10, 2, 4, 8, "#b0b0b0", "#808080", 1),
            R(10, 2, 4, 8, "#b0b0b0", "#808080", 1),
        ]
    if extras:
        elems += extras[row] if isinstance(extras, dict) else extras
    return elems

# --- Helicopter template ---
def helicopter(body_len, body_w, rotor_r, tail_len=12):
    return [
        E(0, 2, body_w, body_len/2, "#808880", "#606860"),
        R(0, body_len/2 + tail_len/2, 3, tail_len, "#708070", "#506050"),
        L(-rotor_r, -2, rotor_r, -2, "#404040", 1.5),
        L(0, -2 - rotor_r * 0.7, 0, -2 + rotor_r * 0.7, "#404040", 1.5),
        C(0, -2, 2, "#909090", "#707070"),
        C(0, 0, body_w * 0.4, "#a0a8a0", "#707070"),
    ]

# --- Fixed-wing template ---
def jet(fuselage_len, wing_span, wing_offset=0):
    half_f = fuselage_len / 2
    return [
        R(0, 0, 6, fuselage_len, "#909090", "#707070", 2),
        P([(-wing_span/2, wing_offset), (0, wing_offset - 6), (wing_span/2, wing_offset)],
          "#808080", "#606060"),
        P([(-6, half_f - 2), (0, half_f - 8), (6, half_f - 2)],
          "#707070", "#505050"),
        C(0, -half_f + 6, 3, "#b0b0b0", "#808080"),
    ]


# ========================================================================
# UNIT DEFINITIONS
# ========================================================================

def generate_all():
    os.makedirs(OUTPUT_DIR, exist_ok=True)
    print("Generating unit sprite sheets...")

    # --- INFANTRY ---
    # RifleInfantry
    rifle_extras = {
        0: [R(10, -8, 2, 16, "#606060", "#404040", 1)],
        1: [R(10, -8, 2, 16, "#606060", "#404040", 1)],
        2: [R(10, -8, 2, 16, "#606060", "#404040", 1)],
        3: [R(0, -18, 2, 14, "#606060", "#404040", 1)],
    }
    make_infantry_sheet("RifleInfantry",
        [infantry(r, extras=rifle_extras) for r in range(4)])

    # RocketSoldier
    rocket_extras = {
        0: [R(10, -4, 3, 14, "#556655", "#404040", 1)],
        1: [R(10, -4, 3, 14, "#556655", "#404040", 1)],
        2: [R(10, -4, 3, 14, "#556655", "#404040", 1)],
        3: [R(0, -18, 3, 14, "#556655", "#404040", 1), C(0, -26, 2, "#ffaa00", "#cc8800")],
    }
    make_infantry_sheet("RocketSoldier",
        [infantry(r, extras=rocket_extras) for r in range(4)])

    # Grenadier
    gren_extras = {
        0: [C(10, -2, 3, "#445544", "#334433")],
        1: [C(10, -2, 3, "#445544", "#334433")],
        2: [C(10, -2, 3, "#445544", "#334433")],
        3: [C(0, -22, 3, "#445544", "#334433")],
    }
    make_infantry_sheet("Grenadier",
        [infantry(r, extras=gren_extras) for r in range(4)])

    # Flamethrower
    flame_extras = {
        0: [R(10, -6, 3, 18, "#885533", "#664422", 1)],
        1: [R(10, -6, 3, 18, "#885533", "#664422", 1)],
        2: [R(10, -6, 3, 18, "#885533", "#664422", 1)],
        3: [R(0, -18, 3, 16, "#885533", "#664422", 1),
            E(0, -28, 4, 3, "#ff6600", "#cc4400")],
    }
    make_infantry_sheet("Flamethrower",
        [infantry(r, body_ry=10, extras=flame_extras) for r in range(4)])

    # ShockTrooper
    shock_extras = {
        0: [R(10, -4, 4, 14, "#5555aa", "#444488", 1),
            L(10, -12, 10, -16, "#88aaff", 2)],
        1: [R(10, -4, 4, 14, "#5555aa", "#444488", 1)],
        2: [R(10, -4, 4, 14, "#5555aa", "#444488", 1)],
        3: [R(0, -18, 4, 12, "#5555aa", "#444488", 1),
            L(-3, -26, 3, -26, "#88aaff", 2),
            L(0, -24, 0, -28, "#88aaff", 2)],
    }
    make_infantry_sheet("ShockTrooper",
        [infantry(r, head_r=7, body_rx=8, body_ry=10, extras=shock_extras) for r in range(4)])

    # Engineer
    eng_extras = {
        0: [R(12, 2, 6, 8, "#ccaa44", "#aa8833", 1)],
        1: [R(12, 2, 6, 8, "#ccaa44", "#aa8833", 1)],
        2: [R(12, 2, 6, 8, "#ccaa44", "#aa8833", 1)],
        3: [R(12, 2, 6, 8, "#ccaa44", "#aa8833", 1)],
    }
    make_infantry_sheet("Engineer",
        [infantry(r, extras=eng_extras) for r in range(4)])

    # Spy
    spy_extras = {
        0: [R(0, -10, 16, 4, "#444444", "#333333", 1)],  # hat brim
        1: [R(0, -10, 16, 4, "#444444", "#333333", 1)],
        2: [R(0, -10, 16, 4, "#444444", "#333333", 1)],
        3: [R(10, -6, 2, 10, "#555555", "#404040", 1)],  # pistol
    }
    make_infantry_sheet("Spy",
        [infantry(r, body_rx=8, extras=spy_extras) for r in range(4)])

    # Tanya
    tanya_extras = {
        0: [R(-10, -6, 2, 10, "#606060", "#404040", 1),
            R(10, -6, 2, 10, "#606060", "#404040", 1)],
        1: [R(-10, -6, 2, 10, "#606060", "#404040", 1),
            R(10, -6, 2, 10, "#606060", "#404040", 1)],
        2: [R(-10, -6, 2, 10, "#606060", "#404040", 1),
            R(10, -6, 2, 10, "#606060", "#404040", 1)],
        3: [R(-4, -20, 2, 12, "#606060", "#404040", 1),
            R(4, -20, 2, 12, "#606060", "#404040", 1)],
    }
    make_infantry_sheet("Tanya",
        [infantry(r, head_r=5, body_rx=6, extras=tanya_extras) for r in range(4)])

    # Medic
    medic_extras = {
        0: [L(-3, 2, 3, 2, "#ff0000", 2.5), L(0, -1, 0, 5, "#ff0000", 2.5),
            R(12, 2, 5, 8, "#ffffff", "#cccccc", 1)],
        1: [L(-3, 2, 3, 2, "#ff0000", 2.5), L(0, -1, 0, 5, "#ff0000", 2.5),
            R(12, 2, 5, 8, "#ffffff", "#cccccc", 1)],
        2: [L(-3, 2, 3, 2, "#ff0000", 2.5), L(0, -1, 0, 5, "#ff0000", 2.5),
            R(12, 2, 5, 8, "#ffffff", "#cccccc", 1)],
        3: [L(-3, 2, 3, 2, "#ff0000", 2.5), L(0, -1, 0, 5, "#ff0000", 2.5),
            R(6, -14, 5, 8, "#ffffff", "#cccccc", 1)],
    }
    make_infantry_sheet("Medic",
        [infantry(r, extras=medic_extras) for r in range(4)])

    # AttackDog (quadruped - custom shape, not humanoid)
    def dog(row):
        # Horizontal body facing north (up)
        dy = 0
        if row == 1: dy = -2
        elif row == 2: dy = 2
        elems = [
            E(0, 2 + dy, 6, 12, "#8B6914", "#6B4914"),  # body
            C(0, -12 + dy, 5, "#9B7924", "#6B4914"),  # head
            # Legs
            R(-5, 10 + dy, 3, 6, "#7B5914", "#5B3914", 1),
            R(5, 10 + dy, 3, 6, "#7B5914", "#5B3914", 1),
            R(-5, -4 + dy, 3, 6, "#7B5914", "#5B3914", 1),
            R(5, -4 + dy, 3, 6, "#7B5914", "#5B3914", 1),
        ]
        if row == 3:  # attack
            elems += [
                R(0, -18, 4, 3, "#cc3333", "#aa2222"),  # open jaw
            ]
        else:
            # Ears
            elems += [
                R(-4, -16, 2, 3, "#9B7924", "#6B4914"),
                R(4, -16, 2, 3, "#9B7924", "#6B4914"),
            ]
        return elems
    make_infantry_sheet("AttackDog", [dog(r) for r in range(4)])

    # --- VEHICLES (tracked tanks) ---
    make_vehicle_sheet("LightTank", tank(24, 20, 8, 16, 3))
    make_vehicle_sheet("HeavyTank", tank(28, 22, 9, 18, 4))
    make_vehicle_sheet("MammothTank", tank(32, 26, 11, 20, 3, dual=True))
    make_vehicle_sheet("TeslaTank",
        tracked_body(26, 22) + [
            C(0, 2, 8, "#5555bb", "#444488"),
            C(0, 2, 4, "#7777dd", "#5555aa"),
            L(0, -6, 0, -14, "#88aaff", 2),
            L(-3, -4, 3, -8, "#88aaff", 1.5),
            L(3, -4, -3, -8, "#88aaff", 1.5),
        ])
    make_vehicle_sheet("ChronoTank",
        tank(24, 20, 8, 14, 3) + [
            C(0, 4, 12, "none", "#44aaff"),
            C(0, 4, 14, "none", "#2288dd"),
        ])

    # --- VEHICLES (tracked, no turret) ---
    make_vehicle_sheet("APC",
        tracked_body(22, 28) + [
            R(0, -2, 16, 20, "#b8b8b8", "#808080", 2),
            R(0, -14, 3, 8, "#707070", "#505050", 1),  # small gun
        ])

    make_vehicle_sheet("Artillery",
        tracked_body(22, 22) + [
            R(0, -14, 3, 20, "#808080", "#606060", 1),  # long barrel
            R(0, 2, 16, 14, "#b8b8b8", "#808080", 2),
        ])

    make_vehicle_sheet("V2Launcher",
        tracked_body(24, 24) + [
            R(0, -2, 18, 18, "#b0b0b0", "#808080", 2),
            R(0, -14, 4, 18, "#aa4444", "#883333", 1),  # missile
            P([(0, -24), (-2, -20), (2, -20)], "#cc5555", "#aa4444"),  # nosecone
        ])

    make_vehicle_sheet("MineLayer",
        tracked_body(22, 22) + [
            R(0, 0, 16, 16, "#b0b0b0", "#808080", 2),
            C(0, 6, 4, "#445544", "#334433"),  # mine hatch
            R(0, -8, 10, 4, "#888888", "#707070", 1),
        ])

    make_vehicle_sheet("MCV",
        tracked_body(30, 30, "#b0a898") + [
            R(0, 0, 24, 24, "#c8b8a8", "#908070", 3),
            R(0, -4, 18, 8, "#a09888", "#807060", 2),
            R(0, 6, 18, 8, "#a09888", "#807060", 2),
        ])

    make_vehicle_sheet("OreTruck",
        tracked_body(24, 28, "#c8b870") + [
            R(0, -4, 18, 12, "#a8a060", "#808040", 2),  # cab
            R(0, 8, 18, 10, "#b0a858", "#908838", 2),   # bed
        ])

    make_vehicle_sheet("PhaseTransport",
        tracked_body(22, 24) + [
            R(0, 0, 16, 18, "#9090b0", "#7070a0", 2),
            C(0, 0, 6, "#a0a0cc", "#8080aa"),
            C(0, 0, 10, "none", "#6666aa"),  # cloak shimmer ring
        ])

    # --- VEHICLES (wheeled) ---
    make_vehicle_sheet("Ranger",
        wheeled_body(20, 22) + [
            R(0, -2, 14, 14, "#b8b8b8", "#808080", 2),
            R(0, -10, 2, 10, "#707070", "#505050", 1),
        ])

    make_vehicle_sheet("RadarJammer",
        wheeled_body(22, 24) + [
            R(0, 2, 16, 16, "#b0b0b0", "#808080", 2),
            C(0, -2, 6, "#557755", "#446644"),  # dish
            L(0, -2, 0, -10, "#446644", 2),  # antenna
        ])

    make_vehicle_sheet("DemoTruck",
        wheeled_body(22, 24, "#cc9944") + [
            R(0, 0, 16, 16, "#ddaa55", "#bb8833", 2),
            L(-4, -2, 4, -2, "#880000", 2),  # hazard X
            L(-4, 2, 4, 2, "#880000", 2),
            C(0, 0, 3, "#ff4444", "#cc2222"),  # warning
        ])

    # --- NAVAL ---
    make_vehicle_sheet("Destroyer",
        ship_hull(28, 10) + [
            R(0, 4, 8, 10, "#707880", "#505860", 2),  # superstructure
            R(0, -6, 3, 10, "#606870", "#404850", 1),  # gun
            C(0, 10, 3, "#555555", "#444444"),  # depth charges
        ])

    make_vehicle_sheet("Cruiser",
        ship_hull(32, 12, "#707888") + [
            R(0, 0, 10, 14, "#808890", "#606870", 2),
            R(-3, -10, 3, 10, "#606870", "#404850", 1),  # left gun
            R(3, -10, 3, 10, "#606870", "#404850", 1),   # right gun
        ])

    make_vehicle_sheet("Submarine",
        [
            E(0, 0, 5, 16, "#505868", "#404858"),
            R(0, -4, 3, 6, "#606870", "#505060"),  # conning tower
            C(0, -8, 2, "#404850", "#303840"),  # periscope
        ])

    make_vehicle_sheet("Gunboat",
        ship_hull(22, 8, "#707878") + [
            R(0, 2, 6, 6, "#606870", "#505060", 1),
            R(0, -6, 2, 8, "#505060", "#404050", 1),
        ])

    make_vehicle_sheet("TransportShip",
        ship_hull(30, 14, "#808078") + [
            R(0, 0, 12, 18, "#909088", "#707068", 2),
            R(0, -2, 8, 10, "#a0a098", "#808078", 1),
        ])

    make_vehicle_sheet("MissileSub",
        [
            E(0, 0, 6, 18, "#505868", "#404858"),
            R(0, -4, 4, 8, "#606870", "#505060"),
            R(-2, 4, 2, 6, "#aa4444", "#883333", 1),  # missile tube
            R(2, 4, 2, 6, "#aa4444", "#883333", 1),
        ])

    # --- AIRCRAFT ---
    make_vehicle_sheet("Longbow", helicopter(14, 7, 14))
    make_vehicle_sheet("Hind", helicopter(16, 8, 13))
    make_vehicle_sheet("Chinook", [
        E(0, 2, 7, 14, "#808870", "#606850"),
        L(-10, -10, 10, -10, "#404040", 1.5),  # front rotor
        L(-10, 14, 10, 14, "#404040", 1.5),    # rear rotor
        C(0, -10, 2, "#909090", "#707070"),
        C(0, 14, 2, "#909090", "#707070"),
        R(0, 2, 10, 20, "#909880", "#707860", 2),
    ])

    make_vehicle_sheet("MiG", jet(24, 22, 2))
    make_vehicle_sheet("Yak", jet(20, 24, 0))

    print("Done!")


if __name__ == "__main__":
    generate_all()
