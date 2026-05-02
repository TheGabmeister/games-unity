"""Generate combat sound effects for Red Alert project."""
import struct
import wave
import math
import random
import os

SAMPLE_RATE = 44100
OUTPUT_DIR = os.path.join(os.path.dirname(__file__), "..", "Assets", "_Project", "Sounds", "Combat")

random.seed(42)


def write_wav(filename, samples):
    path = os.path.join(OUTPUT_DIR, filename)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with wave.open(path, "w") as f:
        f.setnchannels(1)
        f.setsampwidth(2)
        f.setframerate(SAMPLE_RATE)
        for s in samples:
            clamped = max(-1.0, min(1.0, s))
            f.writeframes(struct.pack("<h", int(clamped * 32767)))
    print(f"  {filename}")


def noise():
    return random.uniform(-1, 1)


def envelope(t, attack, decay, duration):
    if t < attack:
        return t / attack
    elapsed = t - attack
    remaining = duration - attack
    if remaining <= 0:
        return 0
    return max(0, 1.0 - elapsed / remaining) ** decay


def generate_rifle_fire():
    """Short, sharp crack — filtered noise burst."""
    duration = 0.08
    n = int(SAMPLE_RATE * duration)
    samples = []
    for i in range(n):
        t = i / SAMPLE_RATE
        env = envelope(t, 0.002, 2.0, duration)
        freq = 3000 - t * 20000
        tone = math.sin(2 * math.pi * freq * t) * 0.3
        n_val = noise() * 0.7
        samples.append((tone + n_val) * env * 0.8)
    write_wav("RifleFire.wav", samples)


def generate_cannon_fire():
    """Deep, boomy cannon blast."""
    duration = 0.25
    n = int(SAMPLE_RATE * duration)
    samples = []
    for i in range(n):
        t = i / SAMPLE_RATE
        env = envelope(t, 0.003, 1.5, duration)
        low_tone = math.sin(2 * math.pi * 80 * t) * 0.5
        mid_tone = math.sin(2 * math.pi * 200 * t) * 0.3
        n_val = noise() * 0.4
        click = math.sin(2 * math.pi * 1000 * t) * max(0, 1 - t * 40) * 0.3
        samples.append((low_tone + mid_tone + n_val + click) * env * 0.9)
    write_wav("CannonFire.wav", samples)


def generate_rocket_fire():
    """Whooshing rocket launch — rising frequency sweep with noise."""
    duration = 0.35
    n = int(SAMPLE_RATE * duration)
    samples = []
    for i in range(n):
        t = i / SAMPLE_RATE
        env = envelope(t, 0.01, 1.2, duration)
        freq = 200 + t * 2000
        sweep = math.sin(2 * math.pi * freq * t) * 0.4
        hiss = noise() * 0.5 * min(1, t * 10)
        rumble = math.sin(2 * math.pi * 60 * t) * 0.2
        samples.append((sweep + hiss + rumble) * env * 0.7)
    write_wav("RocketFire.wav", samples)


def generate_explosion_small():
    """Small explosion — quick noise burst with punch."""
    duration = 0.3
    n = int(SAMPLE_RATE * duration)
    samples = []
    for i in range(n):
        t = i / SAMPLE_RATE
        env = envelope(t, 0.005, 2.0, duration)
        thud = math.sin(2 * math.pi * 100 * t) * 0.4 * max(0, 1 - t * 8)
        crackle = noise() * 0.6
        samples.append((thud + crackle) * env * 0.8)
    write_wav("ExplosionSmall.wav", samples)


def generate_explosion_large():
    """Large explosion — deep rumble with sustained noise."""
    duration = 0.6
    n = int(SAMPLE_RATE * duration)
    samples = []
    for i in range(n):
        t = i / SAMPLE_RATE
        env = envelope(t, 0.005, 1.3, duration)
        rumble = math.sin(2 * math.pi * 50 * t) * 0.5
        mid = math.sin(2 * math.pi * 150 * t) * 0.3 * max(0, 1 - t * 4)
        crackle = noise() * 0.5
        click = math.sin(2 * math.pi * 800 * t) * max(0, 1 - t * 50) * 0.4
        samples.append((rumble + mid + crackle + click) * env * 0.9)
    write_wav("ExplosionLarge.wav", samples)


def generate_unit_death():
    """Unit destroyed — downward pitch sweep with crunch."""
    duration = 0.2
    n = int(SAMPLE_RATE * duration)
    samples = []
    for i in range(n):
        t = i / SAMPLE_RATE
        env = envelope(t, 0.003, 1.8, duration)
        freq = 600 - t * 2500
        if freq < 50:
            freq = 50
        tone = math.sin(2 * math.pi * freq * t) * 0.5
        crunch = noise() * 0.5
        samples.append((tone + crunch) * env * 0.8)
    write_wav("UnitDeath.wav", samples)


if __name__ == "__main__":
    print("Generating combat sounds...")
    generate_rifle_fire()
    generate_cannon_fire()
    generate_rocket_fire()
    generate_explosion_small()
    generate_explosion_large()
    generate_unit_death()
    print("Done!")
