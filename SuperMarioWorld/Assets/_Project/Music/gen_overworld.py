"""Generate a stub Overworld/ground BGM as MIDI, then render via fluidsynth."""
from midiutil import MIDIFile

midi = MIDIFile(1)
track = 0
channel = 0
tempo = 140
midi.addTempo(track, 0, tempo)

# Program 80 = Square Lead (retro/chiptune feel)
midi.addProgramChange(track, channel, 0, 80)

# Simple cheerful melody in C major — a loopable 8-bar phrase
# Inspired by platformer overworld themes: upbeat, major key, bouncy rhythm
melody = [
    # (pitch, time_in_beats, duration_in_beats, velocity)
    # Bar 1
    (72, 0.0, 0.5, 100),   # C5
    (74, 0.5, 0.5, 90),    # D5
    (76, 1.0, 0.5, 100),   # E5
    (79, 1.5, 0.5, 95),    # G5
    (76, 2.0, 0.5, 90),    # E5
    (72, 2.5, 0.5, 85),    # C5
    (74, 3.0, 1.0, 100),   # D5
    # Bar 2
    (76, 4.0, 0.5, 100),   # E5
    (79, 4.5, 0.5, 95),    # G5
    (81, 5.0, 0.75, 100),  # A5
    (79, 5.75, 0.25, 85),  # G5
    (76, 6.0, 0.5, 90),    # E5
    (74, 6.5, 0.5, 85),    # D5
    (72, 7.0, 1.0, 100),   # C5
    # Bar 3
    (79, 8.0, 0.5, 100),   # G5
    (81, 8.5, 0.5, 95),    # A5
    (83, 9.0, 0.75, 100),  # B5
    (84, 9.75, 0.25, 90),  # C6
    (83, 10.0, 0.5, 95),   # B5
    (81, 10.5, 0.5, 90),   # A5
    (79, 11.0, 1.0, 100),  # G5
    # Bar 4
    (84, 12.0, 0.5, 100),  # C6
    (81, 12.5, 0.5, 90),   # A5
    (79, 13.0, 0.5, 95),   # G5
    (76, 13.5, 0.5, 85),   # E5
    (74, 14.0, 0.5, 90),   # D5
    (72, 14.5, 0.5, 85),   # C5
    (74, 15.0, 1.0, 100),  # D5
    # Bar 5 - variation
    (72, 16.0, 0.5, 100),  # C5
    (76, 16.5, 0.5, 95),   # E5
    (79, 17.0, 0.5, 100),  # G5
    (84, 17.5, 0.5, 100),  # C6
    (83, 18.0, 0.5, 95),   # B5
    (81, 18.5, 0.5, 90),   # A5
    (79, 19.0, 1.0, 100),  # G5
    # Bar 6
    (81, 20.0, 0.5, 100),  # A5
    (79, 20.5, 0.5, 90),   # G5
    (76, 21.0, 0.5, 95),   # E5
    (74, 21.5, 0.5, 85),   # D5
    (72, 22.0, 1.0, 100),  # C5
    (74, 23.0, 0.5, 90),   # D5
    (76, 23.5, 0.5, 95),   # E5
    # Bar 7
    (79, 24.0, 0.75, 100), # G5
    (76, 24.75, 0.25, 85), # E5
    (74, 25.0, 0.5, 90),   # D5
    (72, 25.5, 0.5, 85),   # C5
    (67, 26.0, 0.5, 80),   # G4
    (69, 26.5, 0.5, 85),   # A4
    (71, 27.0, 1.0, 90),   # B4
    # Bar 8 - resolve
    (72, 28.0, 1.5, 100),  # C5
    (76, 29.5, 0.5, 90),   # E5
    (79, 30.0, 1.0, 95),   # G5
    (72, 31.0, 1.0, 100),  # C5 (resolve)
]

# Bass line on channel 1 — simple root notes
bass_ch = 1
midi.addProgramChange(track, bass_ch, 0, 38)  # Synth Bass 1

bass = [
    # (pitch, time, duration)
    (48, 0, 2), (43, 2, 2),      # C3, G2
    (48, 4, 2), (45, 6, 2),      # C3, A2
    (43, 8, 2), (45, 10, 2),     # G2, A2
    (48, 12, 2), (43, 14, 2),    # C3, G2
    (48, 16, 2), (45, 18, 2),    # C3, A2
    (45, 20, 2), (48, 22, 2),    # A2, C3
    (43, 24, 2), (45, 26, 2),    # G2, A2
    (48, 28, 2), (48, 30, 2),    # C3, C3
]

for pitch, t, dur in bass:
    midi.addNote(track, bass_ch, pitch, t, dur, 80)

# Percussion on channel 9
perc_ch = 9
for beat in range(32):
    # Kick on 1 and 3
    if beat % 4 == 0:
        midi.addNote(track, perc_ch, 36, beat, 0.25, 100)
    # Snare on 2 and 4
    if beat % 4 == 2:
        midi.addNote(track, perc_ch, 38, beat, 0.25, 90)
    # Hi-hat eighth notes
    midi.addNote(track, perc_ch, 42, beat, 0.25, 60)
    midi.addNote(track, perc_ch, 42, beat + 0.5, 0.25, 50)

for pitch, t, dur, vel in melody:
    midi.addNote(track, channel, pitch, t, dur, vel)

mid_path = r"c:\dev\games-unity\SuperMarioWorld\Assets\_Project\Audio\Music\Overworld.mid"
with open(mid_path, "wb") as f:
    midi.writeFile(f)

print(f"MIDI written to {mid_path}")
