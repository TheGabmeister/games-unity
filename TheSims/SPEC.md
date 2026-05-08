# The Sims (2000) -- Gameplay Systems Spec

The Sims, PC, February 4 2000. Developed by Maxis, published by Electronic Arts. This spec covers the **base game only** unless explicitly noted; expansion-specific features are marked with their pack name (e.g. [Livin' Large]).

---

## 1. Core Loop & Simulation Model

The Sims is a real-time life simulation with no win condition. The player manages one household of up to **8 Sims** on a residential lot, balancing eight Needs (motives), building skills, advancing careers, forming relationships, and constructing/furnishing the home. The core tension is time: every activity that satisfies one need costs time that could satisfy another, and the day-night cycle imposes hard deadlines (work, sleep).

### 1.1 Autonomy & Object Advertising

When not under direct player command, Sims choose actions autonomously. Every object on the lot **advertises** to one or more motives. Each advertisement has:

| Property | Description |
|---|---|
| **Motive** | Which need the object claims to satisfy (Hunger, Fun, etc.) |
| **Strength** | How strongly it advertises (0--50 scale) |
| **Attenuation** | How far the signal reaches -- None/Low = lot-wide, Medium = same building, High = same room |

Sims evaluate all advertisements against their current motive levels, personality, and distance, then queue the highest-scoring action. Autonomy level is configurable 0--100 via options (0 = no autonomous actions, 100 = full free will).

**Priority order** (descending urgency): Hunger > Energy > Hygiene > Bladder > Comfort > Social > Fun > Room.

### 1.2 Action Queue

Players direct Sims by clicking objects or other Sims to open a radial **pie menu** of available interactions, then selecting one. Selected actions are added to a **queue** of up to 8 pending actions, displayed as icon tiles above the selected Sim's portrait. Actions execute in FIFO order. Players can:

- **Reorder** queued actions by dragging the icon tiles.
- **Cancel** individual actions by clicking the X on the tile, or cancel all by clicking the Sim's portrait.
- **Interrupt**: When a critical need crosses its exit threshold (§2.1), the Sim autonomously inserts an urgent action at the front of the queue, pushing player-queued actions back.

### 1.3 Create-a-Sim (CAS)

Before entering a lot, players create a household in Create-a-Sim:

1. **Household size**: 1--8 Sims. At least one adult is required.
2. **Age**: Adult or Child. No other life stages exist in the base game.
3. **Gender**: Male or Female. Affects available clothing and head options.
4. **Skin tone**: 3 options (light, medium, dark).
5. **Head**: ~30 pre-made head meshes per gender (no individual facial feature sliders).
6. **Body**: Adult Sims have a single body type per gender. Clothing is selected from ~20 outfits per gender.
7. **Personality**: Select a zodiac sign to preset the 5-trait allocation (§7.2), then manually redistribute the **25 personality points** across the 5 traits (0--10 each).
8. **Name**: First and last name (family shares last name by default).
9. **Bio**: Optional text description (no gameplay effect).

Children use a smaller set of head/clothing options and cannot hold careers.

---

## 2. Needs / Motives System

All eight needs are tracked on a **-100 to +100 scale** (200 total points). The UI displays them as green bars (positive) or red bars (negative). Overall mood is derived from a weighted average of all eight motives.

### 2.1 Need Descriptions & Consequences

| Need | Satisfiers | Critical Threshold | Consequence at Bottom |
|---|---|---|---|
| **Hunger** | Meals, snacks, pizza | -80 (exit trigger) | Death by starvation ~24 Sim-hours after bottoming out (§8.1) |
| **Comfort** | Chairs, sofas, beds, baths | -90 (exit trigger) | Severe mood penalty; Sim refuses most activities |
| **Hygiene** | Showers, baths, hand-washing, medicine cabinet | -- | Negative social interactions; other Sims react badly |
| **Bladder** | Toilets | -85 (exit trigger) | **Accident** -- Bladder refills instantly, Hygiene tanks to deep red, puddle appears on floor |
| **Energy** | Sleep (beds), naps (sofas/recliners), espresso | -80 (exit trigger) | Sim **passes out** on the spot |
| **Fun** | TVs, games, books, instruments, dancing, telescope | -- | Bad mood; children's Fun decays faster than adults |
| **Social** | Talking, phone calls, group activities | -- | Depression; career stalls (friend requirements unmet) |
| **Room** | Passive -- scans environment for decor, light, cleanliness | -- | Constant mood drag if environment is poor |

**Exit triggers**: When a motive falls below its exit threshold, a resident Sim will abandon the current activity and autonomously seek to fill that need. Visitors use higher (less negative) thresholds and will leave the lot rather than suffer.

| Motive | Resident Exit | Visitor Exit |
|---|---|---|
| Hunger | -80 | -- |
| Bladder | -85 | -80 |
| Energy | -80 | -70 |
| Comfort | -90 | -60 |

### 2.2 Hunger Satisfaction Values

Hunger restored from a full meal depends on fridge quality, appliance quality, AND Cooking skill. The formula is:

**Hunger = Fridge Base + Appliance Modifier + (Cooking Skill x Skill Multiplier)**

| Fridge | Price | Base Hunger |
|---|---|---|
| Llamark (cheapest) | S600 | 9 |
| Porcina Refrigerator | S1,200 | 12 |
| Freeze Secret Refrigerator | S2,500 | 16 |

| Appliance | Price | Hunger Modifier | Skill Multiplier |
|---|---|---|---|
| No appliance (snack from fridge) | -- | 0 | 0 |
| Toaster Oven (Brand Name) | S100 | +9 | x1 |
| Positive Potential Microwave | S250 | +16 | x1 |
| Dialectric Free-Standing Range | S400 | +32 | x1.5 |
| Pyrotorre Gas Range | S1,000 | +40 | x1.5 |

Snacks bypass the formula -- a Sim grabs food from the fridge without cooking, restoring Hunger equal to the fridge's base value only.

| Food Source | Hunger Points | Notes |
|---|---|---|
| Snack (from fridge) | 9--16 | Fridge quality only; no cooking |
| Full Meal (cooked) | 18--72 | Fridge + Appliance + Skill (see formula) |
| Pizza slice (delivery, S40 per pizza) | 33 | 6 slices per pizza |
| Candy (per piece) | 3 | 12 servings per box |
| Fruitcake (per slice) | 7 | 6 slices |

**Example**: Freeze Secret fridge (16) + Pyrotorre stove (40) + Cooking 6 (x1.5 = 9) = 65 Hunger points per serving.

### 2.3 Hygiene Satisfaction Values

| Object | Price | Hygiene Rating |
|---|---|---|
| Wash Hands (any sink) | S250--S500 | 2 |
| Medicine Cabinet | S125 | 4 |
| "Justa" Shower | S650 | 6 |
| Justa Bathtub | S800 | 6 |
| Sani-Queen Bathtub | S1,500 | 8 |
| Hydrothera Bathtub (best) | S3,200 | 10 |

### 2.4 Energy Satisfaction Values

Energy is restored primarily by sleeping. Bed quality determines restoration rate per Sim-hour of sleep:

| Object | Price | Energy Rating | Comfort Rating |
|---|---|---|---|
| Spartan Special (cheap single) | S300 | 7 | 2 |
| Tyke Nyte Bed (child) | S450 | 7 | 3 |
| Napoleon Sleigh Bed | S1,000 | 8 | 7 |
| Modern Mission Bed (double) | S3,000 | 9 | 8 |
| Colonial Ironworks Bed (best) | S3,200 | 10 | 9 |

Sims can also nap on sofas/recliners (lower Energy rate than beds) or drink espresso (§2.8).

### 2.5 Comfort Satisfaction Values

| Object | Price | Comfort Rating |
|---|---|---|
| Werkbunnst All-Purpose Chair | S80 | 2 |
| Posture Plus Office Chair | S100 | 3 |
| Country Class Armchair | S250 | 5 |
| "Back Slack" Recliner | S250 | 6 |
| Indoor-Outdoor Loveseat | S150 | 2 |
| Contempto Loveseat | S150 | 3 |
| Pinstripe Sofa | S400 | 5 |
| "Luxuriare" Loveseat | S875 | 8 |
| Deiter Dansen Sofa | S1,100 | 8 |
| All bathtubs | S800--S3,200 | 5--8 |

Comfort decays faster for Sims with low Active trait. Sims in high-Comfort seating satisfy this need passively while performing other seated activities (eating, watching TV, reading).

### 2.6 Bladder Satisfaction Values

| Object | Price | Bladder Rating |
|---|---|---|
| Flush Force Toilet (cheap) | S325 | 8 |
| Hygeia-O-Matic Toilet (best) | S1,200 | 10 |

There is no other way to relieve Bladder — if no usable toilet is accessible, the Sim will eventually have an accident (§2.1).

### 2.7 Fun Satisfaction Values

Fun activities fall into four types:

| Type | Behavior |
|---|---|
| **Extended** | Sim engages until fun is full or another need interrupts |
| **One-Time** | Single burst of fun points |
| **Timed** | Fixed-duration activity |
| **Endless** | Continuous fun (e.g. dancing) until manually stopped |

| Object | Price | Fun Rating | Other Motives |
|---|---|---|---|
| Monochrome TV (cheap) | S85 | 2 | -- |
| Trottco 27" TV (mid) | S500 | 4 | -- |
| Soma Plasma TV (best) | S3,500 | 7 | -- |
| Boom Box | S100 | 4 | +Social (dancing with others) |
| Strings Theory Stereo (best) | S2,550 | 7 | +Social (dancing with others) |
| Hot Tub | S6,500 | 5 | +Comfort, +Social, +Hygiene |
| Pinball Machine | S1,800 | 6 | -- |
| "Dish" Chess Set (§4.1) | S500 | 2 | +Social (with partner), trains Logic |
| "Star-Traek" Telescope (§4.1) | S2,100 | 3 | Trains Logic |

TV channel preferences are personality-driven:
- Playful Sims prefer Cartoons
- Grouchy (low Nice) Sims prefer Horror
- Active Sims prefer Action
- Outgoing Sims prefer Romance

### 2.8 Espresso (Special Case)

| Motive | Effect |
|---|---|
| Energy | +115 |
| Fun | +10 |
| Bladder | -10 |

### 2.9 Room Score

Room is a **passive** motive -- it continuously reflects the Sim's current environment. It is not "filled" by actions but by the state of the surroundings.

**Positive contributors:**

| Source | Room Score |
|---|---|
| Suit of Armor | +100 |
| Fireplace, Library Edition (lit) | +75 |
| Grandfather Clock | +50 |
| Piano | +30 |
| Train Set (small) | +25 |
| Aquarium (clean, fish alive) | +25 |
| Fireplace (unlit) | +20 |
| Adequate lighting | Variable (+ per lamp) |
| Expensive furniture/decor | Variable (higher price = higher Room) |

**Negative contributors:**

| Source | Room Score |
|---|---|
| Active fire | -100 |
| Aquarium (dirty/dead fish) | -50 |
| Full trash compactor | -25 |
| Flood/puddle | -25 |
| Trash pile | -20 |
| Dead plant | -10 to -20 |
| Dirty plates | -10 |
| Unmade bed | -10 |
| Clogged toilet | -10 |
| Unpainted walls, dim lighting, cramped rooms | Variable negative |

### 2.10 Personality Effects on Need Decay

| Trait | Effect on Decay |
|---|---|
| **Neat** (high) | Hygiene decays slower; Sim autonomously cleans |
| **Neat** (low) | Hygiene decays faster; Sim leaves messes |
| **Outgoing** (high) | Social decays faster (needs more socializing) |
| **Outgoing** (low) | Social decays slower |
| **Active** (low) | Comfort decays faster |
| Children | Fun decays faster than adults |

---

## 3. Mood System

### 3.1 Mood Calculation

Overall mood is a **weighted average** of all eight motives. Each motive has a weight that increases as the motive drops lower (critical needs pull mood down disproportionately).

**Weight hierarchy**: Hunger, Bladder, and Energy carry the heaviest weights. Social, Fun, and Room carry the lightest.

Mood is displayed as up to 5 green bars (positive) or 5 red bars (negative), representing a -100 to +100 scale in 20-point increments.

### 3.2 Mood Effects on Gameplay

| System | Effect |
|---|---|
| **Work performance** | Good mood when leaving for work improves job performance; bad mood hurts it |
| **Skill gain** | Sims in bad mood gain skills slower and may refuse to study |
| **Social interactions** | Bad mood increases chance of negative interaction outcomes |
| **Autonomous behavior** | Low mood causes Sims to prioritize critical needs over player-directed tasks |
| **Tragic Clown** | [Livin' Large] If household average mood drops very low and the Tragic Clown painting is owned, Sunny the Tragic Clown spawns and actively makes things worse |

### 3.3 Job Performance

Five performance tiers: **Dismal, Poor, Average, Good, Excellent**.

- Sims arriving at work in a good mood trend toward Good/Excellent performance.
- Sims arriving in a bad mood trend toward Poor/Dismal.
- **Good/Excellent** performance + meeting skill/friend requirements = promotion eligible.
- **Poor/Dismal** sustained over multiple days = demotion (one level down).
- Attendance and firing rules: see §5.1.

### 3.4 Chance Cards

In the base game, chance cards are **non-interactive** -- a dialog appears when the Sim returns from work describing a workplace event, and the game applies a fixed outcome (skill gain, skill loss, mood change, or career status change). The player makes no choice.

---

## 4. Skill System

Six skills, each on a **0--10 scale**. Skills are required for career promotion and affect gameplay quality.

### 4.1 Skill Training Methods

| Skill | Training Objects (Price) | Personality Accelerator |
|---|---|---|
| **Cooking** | Bookshelf > Study Cooking (S250--S900) | -- |
| **Mechanical** | Bookshelf > Study Mechanical (S250--S900); repair broken objects | -- |
| **Charisma** | Mirror > Practice Speech (S100); Medicine Cabinet (S125) | **Outgoing** (high = faster) |
| **Body** | Exerto Exercise Machine (S700); Swimming Pool (build cost varies) | **Active** (high = faster) |
| **Logic** | Chuck Matewell Chess Set (S500); "Star-Traek" Telescope (S2,100) | **Serious** (low Playful = faster) |
| **Creativity** | "Diamanche" Folding Easel (S250); Chimeway & Daughters Piano (S3,500) | **Playful** (high = faster) |

Exercise machine trains Body ~4x faster than swimming. The cheapest bookshelf (Pinegulcher, S250) works identically to expensive ones for skill training.

Chess also generates Social points when played with another Sim.

**Skill gain speed:**
- Each skill has 10 levels (integer skill points 0--10).
- Base time to gain one skill point: approximately **3--4 Sim-hours** of continuous practice.
- Higher skill levels take progressively longer to earn.
- Personality bonus at extremes (0 or 10 in the relevant trait) roughly **doubles** the gain speed compared to neutral (5).
- The bonus scales linearly: each point away from 5 increases the multiplier.
- Skills **do not decay** -- once earned, a skill point is permanent.
- Performing the activity does NOT train the skill (e.g., cooking meals does not raise Cooking -- Study Cooking from a bookshelf is required).

### 4.2 Skill Gameplay Effects

| Skill | Gameplay Effect |
|---|---|
| **Cooking** | Higher skill = more filling meals (see §2.2). Reduces fire risk (see §8.1). |
| **Mechanical** | Higher skill = faster/cheaper repairs. Reduces electrocution risk (see §8.1). |
| **Charisma** | Improves social interaction success rates. Required for many careers. |
| **Body** | Required for athletic/military/law careers. |
| **Logic** | Required for science/medicine/law careers. |
| **Creativity** | Paintings sell for more with higher skill. A Creativity 0 painting sells for ~S1--S5; Creativity 10 paintings sell for ~S100--S166. Time spent painting also affects value. Required for entertainment/science careers. |

### 4.3 Electrocution Risk by Mechanical Skill

See full table in §8.1. In summary: Mechanical 0 has a very high electrocution chance when repairing electronics, dropping steeply at Mechanical 1 and becoming negligible at 3+. Repairing in standing water increases risk regardless of skill. Light bulb changes carry ~1% risk at any skill level.

---

## 5. Career System

Ten career tracks in the base game, each with **10 job levels**. Careers require increasing combinations of skills and family friends for promotion.

### 5.1 General Career Rules

- **Carpool** arrives 1 hour before the shift starts. The Sim has that 1 hour to board. At shift start time, the carpool leaves with or without the Sim.
- Sims who miss the carpool miss work for the day.
- **Two consecutive missed days** = fired. Must restart from Level 1 if re-entering the same track.
- **Promotion** requires: meeting skill thresholds + friend count + Good/Excellent job performance.
- **Promotion bonus**: 2x the new level's daily salary, paid on the promotion day.
- **Motive decay during work**: Needs decay while at work. Different careers decay different motives at different rates (e.g., Pro Athlete decays Hygiene heavily; Military decays Energy mildly at -15).
- **No weekends**: Sims work every day.
- Bills arrive every 3 (sim) days, based on total lot value.

### 5.2 Career Track Tables

#### Business

| Lvl | Title | Salary | Hours | Cook | Mech | Char | Body | Logic | Creat | Friends |
|-----|-------|--------|-------|------|------|------|------|-------|-------|---------|
| 1 | Mailroom Clerk | 120 | 9--15 | -- | -- | -- | -- | -- | -- | 0 |
| 2 | Executive Assistant | 180 | 9--16 | -- | -- | -- | -- | -- | -- | 0 |
| 3 | Field Sales Rep | 250 | 9--16 | -- | 2 | -- | -- | -- | -- | 0 |
| 4 | Junior Executive | 320 | 9--16 | -- | 2 | 2 | -- | -- | -- | 1 |
| 5 | Executive | 400 | 9--16 | -- | 2 | 2 | -- | 2 | -- | 3 |
| 6 | Senior Manager | 520 | 9--16 | -- | 2 | 3 | -- | 3 | 2 | 6 |
| 7 | Vice President | 660 | 9--17 | -- | 2 | 4 | 2 | 4 | 2 | 8 |
| 8 | President | 800 | 9--17 | -- | 2 | 5 | 2 | 6 | 3 | 10 |
| 9 | CEO | 950 | 9--16 | -- | 2 | 6 | 2 | 7 | 5 | 12 |
| 10 | Business Tycoon | 1200 | 9--15 | -- | 2 | 8 | 2 | 9 | 6 | 14 |

#### Entertainment

| Lvl | Title | Salary | Hours | Cook | Mech | Char | Body | Logic | Creat | Friends |
|-----|-------|--------|-------|------|------|------|------|-------|-------|---------|
| 1 | Waiter/Waitress | 100 | 9--15 | -- | -- | -- | -- | -- | -- | 0 |
| 2 | Extra | 150 | 9--15 | -- | -- | -- | -- | -- | -- | 0 |
| 3 | Bit Player | 200 | 9--15 | -- | -- | 2 | -- | -- | -- | 0 |
| 4 | Stunt Double | 275 | 9--16 | -- | -- | 2 | 2 | -- | -- | 2 |
| 5 | B-Movie Star | 375 | 10--17 | -- | -- | 3 | 3 | -- | 1 | 4 |
| 6 | Supporting Player | 500 | 10--18 | -- | 1 | 4 | 4 | -- | 2 | 6 |
| 7 | TV Star | 650 | 10--18 | -- | 1 | 6 | 5 | -- | 3 | 8 |
| 8 | Feature Star | 900 | 17--01 | -- | 2 | 7 | 6 | -- | 4 | 10 |
| 9 | Broadway Star | 1100 | 10--17 | -- | 2 | 8 | 7 | -- | 7 | 12 |
| 10 | Superstar | 1400 | 10--17 | -- | 2 | 10 | 8 | -- | 10 | 14 |

#### Law Enforcement

| Lvl | Title | Salary | Hours | Cook | Mech | Char | Body | Logic | Creat | Friends |
|-----|-------|--------|-------|------|------|------|------|-------|-------|---------|
| 1 | Security Guard | 240 | 0--6 | -- | -- | -- | -- | -- | -- | 0 |
| 2 | Cadet | 320 | 9--15 | -- | -- | -- | -- | -- | -- | 0 |
| 3 | Patrol Officer | 380 | 17--01 | -- | -- | -- | 2 | -- | -- | 0 |
| 4 | Desk Sergeant | 440 | 9--15 | -- | 2 | -- | 2 | -- | -- | 1 |
| 5 | Vice Squad | 490 | 22--04 | -- | 3 | -- | 4 | -- | -- | 2 |
| 6 | Detective | 540 | 9--15 | 1 | 3 | 1 | 5 | 1 | -- | 4 |
| 7 | Lieutenant | 590 | 9--15 | 1 | 3 | 2 | 5 | 3 | 1 | 6 |
| 8 | SWAT Team Leader | 625 | 9--15 | 1 | 4 | 3 | 6 | 5 | 1 | 8 |
| 9 | Police Chief | 650 | 9--17 | 1 | 4 | 4 | 7 | 7 | 3 | 10 |
| 10 | Captain Hero | 700 | 10--16 | 1 | 4 | 6 | 7 | 10 | 5 | 12 |

#### Life of Crime

| Lvl | Title | Salary | Hours | Cook | Mech | Char | Body | Logic | Creat | Friends |
|-----|-------|--------|-------|------|------|------|------|-------|-------|---------|
| 1 | Pickpocket | 140 | 9--15 | -- | -- | -- | -- | -- | -- | 0 |
| 2 | Bagman | 200 | 23--07 | -- | -- | -- | -- | -- | -- | 0 |
| 3 | Bookie | 275 | 12--19 | -- | -- | -- | 2 | -- | -- | 0 |
| 4 | Con Artist | 350 | 9--15 | -- | -- | 1 | 2 | -- | 1 | 2 |
| 5 | Getaway Driver | 425 | 17--01 | -- | 2 | 1 | 2 | -- | 2 | 3 |
| 6 | Bank Robber | 530 | 15--23 | -- | 3 | 2 | 3 | 1 | 2 | 4 |
| 7 | Cat Burglar | 640 | 21--03 | 1 | 3 | 2 | 5 | 2 | 3 | 6 |
| 8 | Counterfeiter | 760 | 9--15 | 1 | 5 | 2 | 5 | 3 | 5 | 8 |
| 9 | Smuggler | 900 | 9--15 | 1 | 5 | 5 | 6 | 3 | 6 | 10 |
| 10 | Criminal Mastermind | 1100 | 18--00 | 2 | 5 | 7 | 6 | 4 | 8 | 12 |

#### Medicine

| Lvl | Title | Salary | Hours | Cook | Mech | Char | Body | Logic | Creat | Friends |
|-----|-------|--------|-------|------|------|------|------|-------|-------|---------|
| 1 | Medical Technician | 200 | 9--15 | -- | -- | -- | -- | -- | -- | 0 |
| 2 | Paramedic | 275 | 23--05 | -- | -- | -- | -- | -- | -- | 0 |
| 3 | Nurse | 340 | 9--15 | -- | 2 | -- | -- | -- | -- | 0 |
| 4 | Intern | 410 | 9--18 | -- | 2 | -- | 2 | -- | -- | 2 |
| 5 | Resident | 480 | 21--04 | -- | 3 | -- | 2 | 2 | -- | 3 |
| 6 | GP | 550 | 10--18 | -- | 3 | 1 | 3 | 4 | -- | 4 |
| 7 | Specialist | 625 | 10--16 | -- | 4 | 2 | 4 | 4 | 1 | 5 |
| 8 | Surgeon | 700 | 10--16 | -- | 4 | 3 | 5 | 6 | 2 | 7 |
| 9 | Medical Researcher | 775 | 9--16 | -- | 5 | 4 | 6 | 8 | 3 | 9 |
| 10 | Chief of Hospital Staff | 850 | 9--16 | -- | 6 | 6 | 7 | 9 | 4 | 11 |

#### Military

| Lvl | Title | Salary | Hours | Cook | Mech | Char | Body | Logic | Creat | Friends |
|-----|-------|--------|-------|------|------|------|------|-------|-------|---------|
| 1 | Recruit | 250 | 6--12 | -- | -- | -- | -- | -- | -- | 0 |
| 2 | Elite Forces | 325 | 7--13 | -- | -- | -- | -- | -- | -- | 0 |
| 3 | Drill Instructor | 400 | 8--14 | -- | -- | -- | 2 | -- | -- | 0 |
| 4 | Junior Officer | 450 | 9--15 | -- | 1 | 2 | 2 | -- | -- | 0 |
| 5 | Counter-Intelligence | 500 | 9--15 | 1 | 1 | 2 | 4 | -- | -- | 0 |
| 6 | Flight Officer | 550 | 9--15 | 1 | 2 | 4 | 4 | 1 | -- | 1 |
| 7 | Senior Officer | 580 | 9--15 | 1 | 4 | 4 | 5 | 3 | -- | 3 |
| 8 | Commander | 600 | 9--15 | 1 | 6 | 5 | 5 | 5 | -- | 5 |
| 9 | Astronaut | 625 | 9--15 | 1 | 9 | 5 | 8 | 6 | -- | 6 |
| 10 | General | 650 | 9--15 | 1 | 10 | 7 | 10 | 9 | -- | 8 |

#### Politics

| Lvl | Title | Salary | Hours | Cook | Mech | Char | Body | Logic | Creat | Friends |
|-----|-------|--------|-------|------|------|------|------|-------|-------|---------|
| 1 | Campaign Worker | 220 | 9--18 | -- | -- | -- | -- | -- | -- | 0 |
| 2 | Intern | 300 | 9--15 | -- | -- | -- | -- | -- | -- | 0 |
| 3 | Lobbyist | 360 | 9--15 | -- | -- | 2 | -- | -- | -- | 0 |
| 4 | Campaign Manager | 430 | 9--18 | -- | -- | 2 | -- | 1 | -- | 2 |
| 5 | City Council Member | 485 | 9--15 | -- | -- | 3 | 1 | 1 | -- | 4 |
| 6 | State Assemblyperson | 540 | 9--16 | -- | -- | 4 | 2 | 1 | 1 | 6 |
| 7 | Congressperson | 600 | 9--15 | -- | -- | 4 | 3 | 3 | 2 | 9 |
| 8 | Judge | 650 | 9--15 | -- | -- | 5 | 4 | 4 | 3 | 11 |
| 9 | Senator | 700 | 9--18 | -- | -- | 6 | 5 | 6 | 4 | 14 |
| 10 | Mayor | 750 | 9--15 | -- | -- | 9 | 5 | 7 | 5 | 17 |

#### Pro Athlete

| Lvl | Title | Salary | Hours | Cook | Mech | Char | Body | Logic | Creat | Friends |
|-----|-------|--------|-------|------|------|------|------|-------|-------|---------|
| 1 | Team Mascot | 110 | 12--18 | -- | -- | -- | -- | -- | -- | 0 |
| 2 | Minor Leaguer | 170 | 9--15 | -- | -- | -- | -- | -- | -- | 0 |
| 3 | Rookie | 230 | 9--15 | -- | -- | -- | 2 | -- | -- | 0 |
| 4 | Starter | 300 | 9--15 | -- | -- | -- | 5 | -- | -- | 1 |
| 5 | All-Star | 385 | 9--15 | -- | 1 | 1 | 6 | -- | -- | 3 |
| 6 | MVP | 510 | 9--15 | -- | 2 | 2 | 7 | -- | -- | 5 |
| 7 | Superstar | 680 | 9--16 | 1 | 2 | 3 | 8 | -- | -- | 7 |
| 8 | Assistant Coach | 850 | 9--14 | 2 | 2 | 4 | 9 | -- | 1 | 9 |
| 9 | Coach | 1000 | 9--15 | 3 | 2 | 6 | 10 | -- | 2 | 11 |
| 10 | Hall of Famer | 1300 | 9--15 | 4 | 2 | 9 | 10 | -- | 3 | 13 |

#### Science

| Lvl | Title | Salary | Hours | Cook | Mech | Char | Body | Logic | Creat | Friends |
|-----|-------|--------|-------|------|------|------|------|-------|-------|---------|
| 1 | Test Subject | 155 | 9--15 | -- | -- | -- | -- | -- | -- | 0 |
| 2 | Lab Assistant | 230 | 23--05 | -- | -- | -- | -- | -- | -- | 0 |
| 3 | Field Researcher | 320 | 9--15 | -- | -- | -- | -- | 2 | -- | 0 |
| 4 | Science Teacher | 375 | 9--16 | -- | -- | 1 | -- | 3 | -- | 1 |
| 5 | Project Leader | 450 | 10--17 | -- | -- | 2 | -- | 4 | 1 | 3 |
| 6 | Inventor | 540 | 10--19 | -- | 2 | 2 | -- | 4 | 3 | 4 |
| 7 | Scholar | 640 | 10--15 | -- | 4 | 2 | -- | 6 | 4 | 5 |
| 8 | Top Secret Researcher | 740 | 10--15 | 1 | 6 | 4 | -- | 7 | 4 | 7 |
| 9 | Theorist | 870 | 10--14 | 1 | 7 | 4 | -- | 9 | 7 | 8 |
| 10 | Mad Scientist | 1000 | 10--14 | 2 | 8 | 5 | -- | 10 | 10 | 10 |

#### Xtreme

| Lvl | Title | Salary | Hours | Cook | Mech | Char | Body | Logic | Creat | Friends |
|-----|-------|--------|-------|------|------|------|------|-------|-------|---------|
| 1 | Daredevil | 175 | 9--15 | -- | -- | -- | -- | -- | -- | 0 |
| 2 | Bungee Jump Instructor | 250 | 9--15 | -- | -- | -- | -- | -- | -- | 0 |
| 3 | Whitewater Guide | 325 | 9--15 | -- | -- | -- | 2 | -- | -- | 1 |
| 4 | Xtreme Circuit Pro | 400 | 9--15 | -- | 1 | -- | 4 | -- | -- | 2 |
| 5 | Bush Pilot | 475 | 9--15 | 1 | 2 | -- | 4 | 1 | -- | 3 |
| 6 | Mountain Climber | 550 | 9--15 | 1 | 4 | -- | 6 | 1 | -- | 4 |
| 7 | Photojournalist | 650 | 9--15 | 1 | 5 | 2 | 6 | 1 | 3 | 5 |
| 8 | Treasure Hunter | 725 | 10--17 | 1 | 6 | 3 | 7 | 3 | 4 | 7 |
| 9 | Grand Prix Driver | 825 | 10--16 | 1 | 6 | 5 | 7 | 5 | 7 | 9 |
| 10 | International Spy | 925 | 11--17 | 2 | 6 | 8 | 8 | 6 | 9 | 11 |

### 5.3 Salary Range Summary

| Career | Entry Salary | Top Salary | Top Friends Req |
|--------|-------------|------------|-----------------|
| Entertainment | 100 | 1400 | 14 |
| Pro Athlete | 110 | 1300 | 13 |
| Business | 120 | 1200 | 14 |
| Life of Crime | 140 | 1100 | 12 |
| Science | 155 | 1000 | 10 |
| Xtreme | 175 | 925 | 11 |
| Medicine | 200 | 850 | 11 |
| Politics | 220 | 750 | 17 |
| Law Enforcement | 240 | 700 | 12 |
| Military | 250 | 650 | 8 |

Entry salary and top salary are inversely correlated by design -- higher-paying entry jobs cap out lower, and vice versa.

### 5.4 Carpool Vehicles

The vehicle that picks up a Sim for work changes with career level, serving as a visible status indicator.

| Vehicle | Appearance | Typical Levels |
|---------|-----------|----------------|
| Junker / Sloppy Jalopy | Beaten-up 1957 Chevy Bel Air | Levels 1--3 |
| Standard Car / Sedan | Clean mid-range sedan | Levels 4--6 |
| Town Car | Lincoln Town Car style with soft top | Levels 7--8 (Business) |
| Limousine | Stretch limo | Levels 7--10 (most careers) |
| Squad Car | Police car | Law Enforcement levels 1--6 |
| Military Vehicles | Jeep/truck (low), helicopter (high) | Military only |

**Career-specific exceptions:**
- **Entertainment**: Gets Limo early at level 6 (Supporting Player).
- **Law Enforcement**: Squad Car for levels 1--6, Limo for 7--10.
- **Military**: Uses military vehicles instead of civilian cars throughout.

---

## 6. Relationship System

### 6.1 Relationship Score

Each pair of Sims has a **relationship score** from **-100 to +100**.

| Range | Status |
|-------|--------|
| -100 to -25 | Enemy |
| -25 to +25 | Acquaintance |
| +25 to +50 | Warm (not yet friend) |
| +50 to +100 | Friend |
| +70+ | Eligible for romantic interactions |

**Family friends** (required for career promotion) count any Sim at 50+ relationship with *any* household member, not just the working Sim.

### 6.2 Relationship Decay

Relationships with Sims **not on the current lot** decay at approximately **2 points per day**. Phone calls can offset this decay.

### 6.3 Social Interaction Point Values

**Positive Interactions (accepted):**

| Interaction | Social Points | Relationship Points |
|---|---|---|
| Talk (good topic match) | +3 | +3 |
| Talk (bad topic match) | +1 | -3 |
| Compliment (accepted) | +5 | +5 |
| Joke (laughed at) | +5 | +13 |
| Flirt (accepted) | +5 | +13 |
| Back Rub (accepted) | +5 | +7 |
| Hug (accepted) | +7 | +15 |
| Apologize (accepted) | +10 | +15 |
| Kiss (passionate) | +12 | +20 |

**Negative Interactions:**

| Interaction | Social Points | Relationship Points |
|---|---|---|
| Compliment (rejected) | -5 | varies |
| Flirt (rejected) | -10 | -17 |
| Insult (angry reaction) | -10 | -7 to -14 |
| Tease (cry reaction) | -13 | -7 |
| Slap | 0 | -20 |

### 6.4 Interaction Availability

Not all interactions are available at all times. Requirements:

| Interaction | Minimum Relationship | Category |
|---|---|---|
| Talk, Joke, Entertain | Any (strangers OK) | Friendly |
| Tickle, Compliment | ~25+ | Friendly |
| Hug | ~50+ | Friendly |
| Back Rub | ~50+ | Physical (friendly) |
| Flirt | ~70+ | Romantic |
| Kiss | ~70+ | Romantic |
| Propose > Move In | ~80+ | Romantic (non-household only) |
| Propose > Marriage | ~90+ | Romantic (household only) |

Romantic interactions are restricted to adult-adult pairs. Success chance depends on mood and personality even when the interaction is available (§6.5).

### 6.5 Interaction Success Factors

Outcome (accept vs. reject) depends on:
- Current relationship score
- Both Sims' mood
- Personality traits (Nice, Outgoing, Playful especially)
- **Interests**: If Sims share high interest in the same topics, Talk interactions succeed more often

### 6.6 Interests System

Each Sim has hidden interest scores (0--10) in multiple topics.

**Base game interests (8):** Travel, Money, Politics, 60's, Weather, Sports, Music, Outdoors.

**Children's interests** replace the first four with: Toys, Aliens, Pets, School.

[Hot Date] adds 7 adult-only interests: Exercise, Food, Parties, Style, Hollywood, Technology, Romance.

- Sims with **6+ points** in a topic: conversations about it succeed (green +).
- Sims with **less than 3 points** in a topic: conversations about it fail (red -).
- Sims talk about their highest-interest topics when initiating conversation.

### 6.7 Zodiac Compatibility

Zodiac signs create attraction/repulsion between Sims:

| Zodiac | Attracted To | Repelled By |
|--------|-------------|-------------|
| Aries | Gemini, Taurus | Virgo, Cancer |
| Taurus | Aries, Libra | Virgo, Cancer |
| Gemini | Pisces, Virgo | Capricorn, Aries |
| Cancer | Taurus, Scorpio | Gemini, Aries |
| Leo | Sagittarius, Cancer | Capricorn, Gemini |
| Virgo | Aquarius, Sagittarius | Leo, Taurus |
| Libra | Cancer, Virgo | Pisces, Scorpio |
| Scorpio | Pisces, Leo | Libra, Aquarius |
| Sagittarius | Pisces, Capricorn | Libra, Scorpio |
| Capricorn | Aquarius, Taurus | Leo, Gemini |
| Aquarius | Capricorn, Sagittarius | Scorpio, Virgo |
| Pisces | Scorpio, Gemini | Leo, Aries |

### 6.8 Romance, Marriage & Babies

**Romance progression (base game):**
1. Build relationship to ~50+ through friendly interactions (Talk, Joke, Compliment, Tickle).
2. At ~70+ relationship, romantic interactions unlock (Flirt, Kiss, etc.).
3. For non-household Sims: **Propose > Move In** (at ~80+ relationship + good mood) brings the NPC into the player's household, combining their funds.
4. For household members: **Propose > Marriage** (at ~90+ relationship + good mood on both Sims) marries them. Two Sims in the same household CAN marry -- there is no restriction requiring them to be from different households.

The base game has a single relationship bar per Sim pair. The **Crush** / **Love** statuses and the dual **Daily / Lifetime** relationship bars were added by **[Hot Date]**.

**Baby creation:**
- Two opposite-gender adult Sims with high relationship (~90+) Kiss -- a dialog pops up: "Should we have a baby?"
- [Livin' Large] The **Vibromatic Heart Bed** (S4,500) "Play in Bed" interaction provides an alternate trigger for the baby dialog.
- If accepted, a baby bassinet appears immediately.
- Sims may also receive a random phone call offering **adoption**.

**Baby lifecycle:**
- Babies are treated as **objects**, not full Sims. They occupy a bassinet.
- Babies require feeding approximately every 6 Sim-hours.
- After **72 Sim-hours (3 Sim-days)**, the baby automatically ages up to a **child**.
- Baby appearance (head, clothes) is **random** -- no genetics system exists in The Sims 1.

**Children:**
- Children **never age into adults** in the base game.
- Adults **never age or die of old age** in the base game.
- Children attend school daily (school bus at 9:00 AM). Cannot miss school.
- Children do homework on any available surface.
- Grades improve with good mood at departure + completing homework.
- Relatives may call and give S100 for high grades.
- **Seven consecutive days of F grades** = child sent to **Military School** (permanently removed).
- If all adults in a household die, children and babies are removed by the Social Worker.

### 6.9 Visitors & Guest Mechanics (Base Game)

In the base game (without House Party expansion), there is no formal "throw party" phone interaction. Guest mechanics:

- Sims can **phone individual Sims** to invite them over.
- Neighbors and known Sims occasionally walk by on the sidewalk and can be greeted.
- Visitors have higher need exit thresholds than residents -- they leave the lot when their needs drop rather than suffering.
- Visitors will use objects on the lot (TV, food, bathroom) but will leave if environment/needs become poor.
- The **"throw party" phone interaction** was added by the House Party expansion.

---

## 7. Personality System

### 7.1 Five Traits

Each trait is on a **0--10 scale**. Players allocate **25 total personality points** during Create-a-Sim. Choosing a zodiac sign presets the distribution, which can then be manually adjusted.

| Trait | Low (0--3) | High (7--10) |
|---|---|---|
| **Neat/Sloppy** | Leaves messes; hygiene decays faster; never cleans autonomously | Cleans proactively; washes dishes immediately; flushes toilet; hygiene decays slower |
| **Outgoing/Shy** | Slow to approach strangers; Social decays slowly; difficulty making friends | Social decays fast (needs constant socializing); excels at parties; approaches strangers easily |
| **Active/Lazy** | Prefers sitting; comfort breaks needed more often | Prefers exercise, swimming, dancing; loses energy slower |
| **Playful/Serious** | Prefers reading, chess, painting (quiet activities) | Prefers games, TV, jokes, dancing (loud/social activities); life of the party |
| **Nice/Grouchy** | Insults, teases, taunts; steals garden gnomes; kicks over trash cans; quick temper | Gives compliments and hugs naturally; improves others' moods; relationship gains easier |

### 7.2 Zodiac Sign Default Allocations

Each zodiac sign provides a starting personality point distribution (total = 25):

| Zodiac | Neat | Outgoing | Active | Playful | Nice |
|--------|------|----------|--------|---------|------|
| Aries | 5 | 8 | 6 | 3 | 3 |
| Taurus | 5 | 5 | 3 | 8 | 4 |
| Gemini | 4 | 7 | 8 | 3 | 3 |
| Cancer | 6 | 3 | 6 | 4 | 6 |
| Leo | 4 | 10 | 4 | 4 | 3 |
| Virgo | 9 | 2 | 6 | 3 | 5 |
| Libra | 2 | 8 | 2 | 6 | 7 |
| Scorpio | 6 | 5 | 8 | 3 | 3 |
| Sagittarius | 2 | 3 | 9 | 7 | 4 |
| Capricorn | 7 | 4 | 1 | 8 | 5 |
| Aquarius | 4 | 4 | 4 | 7 | 6 |
| Pisces | 5 | 3 | 7 | 3 | 7 |

### 7.3 Personality Effects on Skill Gain

| Skill | Accelerated By | Notes |
|---|---|---|
| Creativity | Playful (high) | Higher Playful = faster gain from Easel/Piano |
| Body | Active (high) | Higher Active = faster gain from exercise/swimming |
| Charisma | Outgoing (high) | Higher Outgoing = faster gain from Mirror/Cabinet |
| Logic | Serious (low Playful) | Lower Playful = faster gain from Chess/Telescope |

### 7.4 Personality Effects on Fun Activities

Personality determines maximum fun value from specific objects:
- Playful Sims get up to **+10 bonus fun points** from games, TV, computer
- Serious Sims get bonus fun from chess, reading, painting
- Active Sims get bonus fun from exercise and swimming

---

## 8. Death Mechanics

### 8.1 Causes of Death (Base Game)

Four death types in the base game:

| Death Type | Trigger | Time to Death | Prevention |
|---|---|---|---|
| **Starvation** | Hunger bottoms out at -100 | ~24 Sim-hours after Hunger first reaches -100 | Keep fridge stocked; ensure path to kitchen is unblocked |
| **Fire** | Cooking with Skill < 3; objects too close to fireplace | Sim on fire for ~1 Sim-hour without extinguishing | Cooking Skill 3+; smoke detector in each room; clear fireplace area |
| **Drowning** | Energy depletes to zero while swimming | Immediate upon Energy reaching zero in pool | Ensure pool has ladder; watch Energy bar before swimming |
| **Electrocution** | Repairing broken electronics with low Mechanical skill | Instant (single failed repair roll) | Mechanical Skill 3+; hire Repairman; never repair in standing water |

**Fire risk by Cooking skill level:**

| Cooking Skill | Fire Chance (per stove use) |
|---|---|
| 0 | ~15% |
| 1 | ~5% |
| 2 | ~1% |
| 3+ | ~0% |

**Electrocution risk by Mechanical skill level:**

| Mechanical Skill | Shock Chance (per electronics repair) | Notes |
|---|---|---|
| 0 | Very high (~50--100%) | Sources vary; unanimously "almost certain" |
| 1 | ~25% | |
| 2 | ~10% | |
| 3--5 | ~5% | |
| 6+ | ~1% | |
| Light bulb change (any skill) | ~1% | |
| Repairing in standing water | Increased (exact modifier unknown) | Puddles from broken plumbing |

### 8.2 Fire Spread & Suppression

**Spread behavior:**
- Fire spreads to adjacent tiles each game tick, consuming all flammable objects.
- Fire can travel between floors via staircases (including spiral/metal staircases).
- Fire continues until it runs out of flammable objects or is extinguished.
- Burned objects leave **ash piles** (count as trash, reduce Room score).

**Sim panic behavior:**
- Sims **panic** when fire is detected and may repeatedly run toward the fire rather than away.
- Panicking Sims cancel all queued actions.
- Sims can attempt to extinguish fires manually, but panic often interrupts this.

**Smoke Detector** (S50): Placed on wall. Automatically calls fire department when fire is detected in the **same room**. Coverage is per-room -- must install one in each room with fire hazards. Multiple detectors needed for multi-room coverage.

**Firefighter response**: Arrives after smoke detector triggers (or phone call). Extinguishes all fires on the lot. No charge for real fires. Charges **S100 fine** for false alarms.

### 8.3 Drowning Mechanics

- Sims cannot climb out of pools without ladders. Removing the ladder traps Sims.
- When a Sim's Energy motive depletes to zero while swimming, they drown.
- A Sim can enter a pool that doesn't have a ladder, but cannot exit without one.

### 8.4 Expansion Death Types

| Death Type | Expansion | Trigger |
|---|---|---|
| **Guinea Pig Disease** | [Livin' Large] | Bitten by sick guinea pig (dirty cage); fatal if needs drop too low while sick. Cure: maintain high Energy/Comfort or use white potion. |
| **Toadification** | [Makin' Magic] | Spell transforms Sim into toad; toad eaten by pet dragon or cat |
| **Skydiving Malfunction** | [Superstar] | Sim falls asleep in Galileo's Free-for-All machine (S16,999); exit blocked |

### 8.5 Death Sequence

1. Sim dies.
2. **Base game**: Tombstone (outdoor death) or Urn (indoor death) appears at the death location. No Grim Reaper in base game.
3. **[Livin' Large]+**: The **Grim Reaper** appears. Another household Sim can attempt to **plead** with the Reaper.

### 8.6 Grim Reaper Pleading [Livin' Large]

- A household Sim can plead with the Grim Reaper to spare the deceased.
- Success chance is based on the **relationship score** of the pleading Sim toward the deceased:
  - Below 25: **guaranteed failure**
  - Above 25: percentage chance proportional to the score, **capped at 90%**
- If the Reaper agrees to consider the plea, he challenges the pleading Sim to **Rock-Paper-Scissors**.
  - **Win**: Deceased is revived.
  - **Lose**: Deceased dies permanently OR is resurrected as a zombie (personality inverted, gray skin).

### 8.7 Ghost Behavior

- Ghosts appear **only at night** near their tombstone/urn.
- Ghosts **wake sleeping Sims** in the vicinity (do not place urns in bedrooms).
- Ghost mood reflects death circumstances:
  - Ghosts who died of hunger become angry if no fridge is on the lot or fridge is empty.
  - Ghosts whose spouse remarries become angry (only calmed if living spouse divorces).
- Moving the tombstone/urn off the lot removes the ghost.

---

## 9. NPC System

### 9.1 Service NPCs

| NPC | Trigger | Cost | Schedule | Function |
|-----|---------|------|----------|----------|
| **Maid (Brigit)** | Phone booking | S10/hour | Daily, 10 AM -- 5 PM | Cleans dishes, mops puddles, flushes toilets, makes beds, picks up trash. Does not cook. |
| **Gardener (Gloria)** | Phone booking | S10/hour | Every 3 days | Waters all indoor and outdoor plants |
| **Repairman (Bruno)** | Phone call | S50/hour | On demand | Repairs all broken objects |
| **Pizza Delivery (Dale)** | Phone order | S40 per pizza (6 slices) | Within 1 Sim-hour of call | Delivers pizza (33 Hunger points per slice) |

### 9.2 Emergency NPCs

| NPC | Trigger | Cost | Behavior |
|-----|---------|------|----------|
| **Firefighter (Freddy)** | Smoke detector triggers; or phone call | Free | Extinguishes fires. Charges S100 fine for false alarms. |
| **Police Officer (Michelle)** | Burglar alarm triggers; or phone call | Free | Arrests burglar. Household receives S1,000 reward. |

### 9.3 Antagonist NPCs

| NPC | Trigger | Behavior |
|-----|---------|----------|
| **Burglar ("Some Sneaky Sim")** | Random, at night | Steals the most expensive portable item first, then works down by value. Flees if Burglar Alarm (S250, wall-mounted) sounds. Surrenders when police arrive. |
| **Repo Man (Bud)** | Bills left unpaid past due date | Bills escalate color: white > yellow > orange > red. After red bills go 2+ days unpaid, warning at 7:00 AM. Bud arrives at 6:00 PM. Repossesses objects equal in value to the unpaid bill (may take more). Cannot be stopped. |
| **Social Worker (Sylvia)** | Baby cries for extended period without care | Warning message appears after several Sim-hours of baby neglect. If baby not cared for within 1 additional Sim-hour, Sylvia arrives and permanently removes the baby. Children sent to military school for chronic F grades (7 consecutive days). |

### 9.4 Utility NPCs

| NPC | Schedule | Function |
|-----|----------|----------|
| **Newspaper Girl (Nancy)** | Daily at 9:05 AM | Delivers newspaper. Stops delivering if 5+ unread papers accumulate outside. |
| **Mail Carrier (Carrie)** | Every 3 days | Delivers bills. Can block carpool/school bus path. |

### 9.5 Expansion NPCs [Livin' Large]

| NPC | Trigger | Behavior |
|-----|---------|----------|
| **Tragic Clown (Sunny)** | Household owns the Tragic Clown painting + average mood is very low | Appears uninvited. Attempts to cheer Sims up but fails, making mood worse. Very difficult to remove. |
| **Grim Reaper** | Sim death | Collects the deceased. Can be pleaded with (see §8.6). |
| **Evil Clone** | Chemistry Set (dark green potion) | Identical appearance, opposite personality. Autonomous and disruptive. |

---

## 10. Time System

### 10.1 Time Scale

- **1 Sim minute = approximately 1 real second** at normal speed.
- Three speed settings: **Normal (1)**, **Fast (2)**, **Ultra (3)**.
- Game **auto-switches to Ultra** when all Sims are asleep or no controllable Sims are on the lot.
- A full 24-hour Sim day takes approximately 24 real minutes at normal speed.

### 10.2 Day/Night Cycle

- Visual representation: Sun/Moon moves across the sky. Light blue background during day, dark blue at night.
- When a family first moves onto a lot, the clock starts at **7:00 AM**.
- **No weekends**. No named days of the week. Every day is identical structurally.
- Bills arrive every 3 days (only notable cycle).

### 10.3 Daily Schedule Pattern

| Time | Typical Activity |
|------|-----------------|
| 6:00 AM | Sims may wake naturally if Energy is full |
| 7:00--8:00 AM | Morning routine (bathroom, breakfast) |
| 8:00--9:00 AM | Carpool/school bus arrival window (varies by career) |
| 9:00 AM--3:00 PM | Typical work/school hours (varies widely by career level) |
| 3:00 PM--10:00 PM | Post-work: skill building, socializing, entertainment |
| 10:00 PM--6:00 AM | Sleep |

---

## 11. Building & Buy Mode

### 11.1 Starting Budget

Every new household begins with **S20,000** regardless of family size. This must cover both the lot purchase and all construction/furnishing.

### 11.2 Lot Sizes

The base game neighborhood has **10 lots** of varying sizes:

- Lots are tile-based grids. Buildable area ranges from ~784 tiles (small lots) to ~1,760 tiles (large lots).
- Lot prices range from approximately **S3,500 to S11,500**, proportional to area.
- Buildings are limited to **2 floors** maximum.
- Lot terrain is flat by default but can be sculpted in Build Mode (§11.3).

### 11.3 Build Mode Tools (12 total)

| Tool | Function |
|------|----------|
| **Terrain** | Raise, lower, level, and grass tools |
| **Water/Pool** | Create swimming pools (requires ladder for Sim access) |
| **Wall & Fence** | Draw walls and fences |
| **Wallpaper** | Apply wall textures |
| **Floor** | Apply floor textures |
| **Door** | Place door types in walls |
| **Window** | Place window types in exterior walls |
| **Roof** | Add/modify roof |
| **Stair** | Connect first and second floors |
| **Fireplace** | Place fireplace units |
| **Plant** | Place decorative plants |
| **Hand** | Select, move, rotate, or sell placed objects |

### 11.4 Buy Mode Categories (8 total)

| Category | Subcategories |
|----------|---------------|
| **Seating** | Dining Chairs, Lounge Chairs, Sofas, Beds, Other |
| **Surfaces** | Counters, Tables, End Tables, Desks |
| **Decorative** | Paintings, Sculptures, Rugs, Plants, Other |
| **Electronics** | Entertainment, Video, Audio, Phones, Other |
| **Appliances** | Stoves, Refrigerators, Small Appliances, Large Appliances |
| **Plumbing** | Toilets, Sinks, Showers, Bathtubs |
| **Lighting** | Table Lamps, Standing Lamps, Wall Lamps, Other |
| **Miscellaneous** | All items not fitting other categories |

Base game catalog contains approximately **150 buy-mode items** and **110 build-mode items**.

### 11.5 Object Breakage & Maintenance

Many objects can break through normal use. Cheaper objects break more frequently than expensive ones.

**Breakable object types:**
- **Plumbing** (toilets, sinks, showers, dishwashers): Clog or leak, creating puddles that lower Room score and pose electrocution risk.
- **Electronics** (TVs, computers, stereos): Stop working. Repairing carries electrocution risk (§8.1).
- **Appliances** (stoves, trash compactors): Malfunction.

**Repair options:**
- **DIY**: Click the broken object > Repair. Requires the Sim to walk to the object and spend time fixing it. Speed and success scale with Mechanical skill. Low Mechanical = electrocution risk on electronics. Repairing in standing water (from plumbing leaks) increases risk.
- **Repairman** (§9.1): S50/hour, no electrocution risk, repairs all broken objects on the lot.

**Cleaning dishes**: Dirty dishes left on surfaces lower Room score. Sims can wash dishes by hand at a sink, or a **Dishwasher** (S550, placed adjacent to a counter) automates the process — Sims place dirty dishes in it and it runs automatically. Neat Sims autonomously clean dishes; Sloppy Sims leave them.

**Plumbing floods**: Broken sinks, toilets, and dishwashers create expanding puddles. Puddles lower Room score (-25 each), must be mopped up manually by a Sim or cleaned by the Maid, and increase electrocution risk for nearby electronics repairs.

**Essential lot objects**: Every lot has a **Mailbox** and **Trash Can** that cannot be sold or deleted in Buy Mode. The Mailbox receives bills and newspapers; the Trash Can is the primary waste disposal. (`move_objects on` cheat bypasses this restriction -- see §18.)

### 11.6 Object Depreciation

Objects lose value once placed. Selling a used object returns less than purchase price (initial depreciation of ~15% on placement, with further decline over time). Bills are calculated as a percentage of total lot value (lot + all placed objects) and arrive every 3 Sim-days.

---

## 12. Economy

### 12.1 Income Sources

| Source | Amount |
|--------|--------|
| Career salary | S100--S1,400/day depending on career and level (§5.2) |
| Promotion bonus | 2x new daily salary (one-time on promotion day) |
| Painting sales | S1--S166 per painting, scaling with Creativity skill + time spent |
| Child grade bonus | S100 (random phone call from relative for high grades) |
| Burglar arrest reward | S1,000 (police must catch the burglar) |
| Move-in | When an NPC Sim moves into the household via Propose > Move In, their funds are added to household funds |

### 12.2 Job Finding

Sims find careers through two methods:

| Method | Object | Price | Listings Per Day | Notes |
|---|---|---|---|---|
| **Newspaper** | Delivered daily by Nancy (§9.4) | Free | 1 | One random career at level 1. If undesirable, wait for next day's paper. |
| **Computer** | Moneywell Computer (S999) or Microscotch Q5 (S1,800) or Meet Marco (S6,500) | S999+ | 3 | Three random career listings. More variety per day. |

Only Level 1 positions are offered. Each listing is a random career track the Sim is not already employed in.

**Computers** also provide Fun (Play game interaction) and can be used for other activities depending on the model:

| Computer | Price | Fun Rating | Notes |
|---|---|---|---|
| Moneywell Computer | S999 | 3 | Basic model; find job + play games |
| Microscotch Q5 | S1,800 | 5 | Mid-tier |
| Meet Marco | S6,500 | 7 | Best; highest Fun rating |

### 12.3 Phone System

The telephone (S50, wall-mounted; or S75, tabletop) provides access to essential services:

| Phone Option | Function |
|---|---|
| **Call...** > *[Sim name]* | Call a known Sim to chat (restores Social, maintains relationship) or invite them over |
| **Services** > Maid | Book daily maid service (§9.1) |
| **Services** > Gardener | Book gardener service (§9.1) |
| **Services** > Repairman | Call for immediate repair visit (§9.1) |
| **Services** > Pizza | Order pizza delivery (§9.1) |
| **Fire** | Call fire department (if no smoke detector) |
| **Police** | Call police (if no burglar alarm) |
| **Quit Job** | Resign from current career (must find a new one via newspaper/computer) |

**Incoming calls**: The phone rings periodically. Unanswered calls go away after ~6 rings. Incoming call types:
- Friend/acquaintance calling to chat (Social boost, relationship maintenance)
- Relative calling to give the child S100 for good grades
- Random adoption offer ("Would you like to adopt a baby?")
- Chance card outcome notification (on return from work)
- Prank calls (occasionally, no gameplay effect)

### 12.4 Expenses

| Expense | Amount |
|---------|--------|
| Lot purchase | S3,500--S18,000 |
| Construction & furniture | Variable |
| Bills (every 3 days) | Based on total lot value |
| Maid | S10/hour |
| Gardener | S10/hour |
| Repairman | S50/hour |
| Pizza delivery | S40 per pizza |
| Object replacement (fire/theft) | Original purchase price |

---

## 13. Controls & Input

Keyboard + Mouse only (no gamepad support).

| Input | Action |
|-------|--------|
| Left Click | Select Sim, issue command, place object |
| Right Click | Rotate view / cancel |
| Mouse Wheel / Arrow Keys | Scroll/pan camera |
| 1 / 2 / 3 | Normal / Fast / Ultra speed |
| P | Pause |
| Tab | Cycle between Sims in household |
| F1 | Live Mode |
| F2 | Buy Mode |
| F3 | Build Mode |
| Ctrl+Z | Undo (build/buy mode) |

---

## 14. UI & HUD

### 14.1 Live Mode HUD

The primary control panel occupies the **bottom of the screen**:

- **Sim portraits**: Bottom-left; clickable to select household members. Green/red mood diamond beside each portrait.
- **Control panel center**: Shows the selected Sim's info. Tabbed panels for:
  - **Needs**: 8 horizontal bars (Hunger, Comfort, Hygiene, Bladder, Energy, Fun, Social, Room). Green = positive, Red = negative.
  - **Skills**: 6 skill progress bars with current level.
  - **Relationships**: Grid of known Sim portraits with relationship bar (-100 to +100).
  - **Job**: Current career, title, level, next work time, skill and friend requirements for next promotion.
- **Action queue**: Icon tiles displayed above the selected Sim's portrait showing pending actions (§1.2).
- **Speed controls**: Bottom-right. Three speed buttons (1/2/3) + pause.
- **Clock**: Displays current Sim time.
- **Household funds**: Simoleon balance displayed prominently.
- **Mode buttons**: Live/Buy/Build mode toggle (also F1/F2/F3).

### 14.2 Interaction Pie Menu

Clicking on an object or Sim opens a radial **pie menu** of available interactions. Submenus expand for objects with multiple options (e.g., a bookshelf offers Study Cooking, Study Mechanical, Read). Social interactions on other Sims are filtered by relationship score, age, and mood (§6.4).

---

## 15. Save System

- Manual save only (no autosave in base game).
- Saving writes the **entire neighborhood state** -- all lots, all families, all relationships. There is one save state per neighborhood, not per household.
- Saving is available only in **Live Mode** (not in Buy/Build Mode).
- Exiting without saving discards all progress since last save.
- The player can only control one household at a time. Switching households (via the neighborhood screen) returns to the last saved state of the new household.

---

## 16. Pre-Made Families & Neighborhood

### 16.1 Neighborhood 1 Layout

The base game ships with a single neighborhood containing **10 lots** on **Sim Lane**. The neighborhood is a small suburban area with a single loop of road. Lots range from ~784 to ~1,760 buildable square tiles, priced from S3,500 to S11,500.

### 16.2 Pre-Built Families

| Lot | Family | Members | Funds | Notes |
|---|---|---|---|---|
| 2 Sim Lane | **Newbie** | Bob (Aquarius), Betty (Aries) | ~S20,000 | Tutorial family; 2 adults, no children |
| 5 Sim Lane | **Goth** | Mortimer (Taurus), Bella (Cancer), Cassandra (Cancer, child) | S35,658 | Iconic family; large Gothic house valued ~S17,693 |
| 3 Sim Lane | **Pleasant** | Jeff (Cancer), Diane (Aries), Daniel (Sagittarius, child), Jennifer (Sagittarius, child) | ~S20,000 | 2 adults, 2 children |
| 7 Sim Lane | **Roomies** | Chris, Melissa | ~S20,000 | Two unrelated adult roommates |

**Unoccupied families (in family bin):**
- **Michael Bachelor** -- Single adult male. Designed as solo-play challenge.

### 16.3 Downloadable Families (from Maxis Website)

Originally available for free download (now archived):
- **Hatfield** -- Poor family with basic furnishings
- **Jones** -- Edward, Barbara, Bobby (placed at 1 Sim Lane)
- **Valentino** -- Rudy and Julia in a large house
- **Snooty Patooty** -- Chip and Buffy (wealthy couple parody)
- **Maximus** -- Two adults in unconventional house

### 16.4 Empty Lots

Several lots are available for purchase: 1, 4, 6, 8, 9, and 10 Sim Lane. Some have pre-built empty houses (e.g., 10 Sim Lane has a small 4-room starter house), while others are bare land.

---

## 17. Camera & Presentation

### 17.1 Camera System

The game uses a **3D isometric** perspective with a fixed-angle camera:

- **Rotation**: 4 fixed angles (90-degree increments), rotated via keyboard (< / >) or toolbar buttons.
- **Zoom**: 3 zoom levels (close, medium, far). Scrollwheel or +/- keys.
- **Pan**: Arrow keys or mouse drag near screen edges.
- **Floor selection**: Toggle between 1st and 2nd floor view. When viewing the 2nd floor, the 1st floor roof/ceiling is hidden.

### 17.2 Wall Visibility Modes

Three wall display modes, toggled via toolbar buttons:

| Mode | Behavior |
|---|---|
| **Walls Up** | All walls fully rendered. Interior visibility limited by camera angle. |
| **Walls Cutaway** | Walls between camera and Sims are cut to half-height, revealing room interiors while preserving spatial context. Default mode. |
| **Walls Down** | All walls hidden. Floor plan view -- only floors, objects, and Sims visible. |

---

## 18. Cheats

Open cheat console with **Ctrl+Shift+C**. Type the cheat and press Enter.

| Cheat | Function | Compatibility |
|---|---|---|
| `klapaucius` | +S1,000 Simoleons | Original unpatched version only |
| `rosebud` | +S1,000 Simoleons | Patched version (replaces klapaucius) |
| `rosebud;!;!;!` (etc.) | +S1,000 per `!` appended | Stack for bulk money; each `!` adds another S1,000 |
| `move_objects on/off` | Move any object freely | Includes mailbox, trash can, even Sims themselves |
| `autonomy [0-100]` | Set free will level | 0 = no autonomous behavior; 100 = maximum |
| `interests` | Display selected Sim's interest values | Debug/inspection tool |
| `grow_grass [0-150]` | Adjust grass growth rate | |
| `map_edit on/off` | Modify buildable lot area | Allows building outside normal boundaries |
| `prepare_lot` | Fix lot; remove incomplete objects | Debug cleanup tool |
| `water_tool` | Enable water placement tool | |
| `history` | Write family history to file | Exports to text file |
| `nessie` | Force Nessie appearance on neighborhood screen | Easter egg |

**What cheats reveal about mechanics:**
- `move_objects on` confirms all entities (including "permanent" objects like mailbox/trash and Sims themselves) are movable grid-based entities.
- `autonomy` confirms the autonomous behavior system uses a continuous 0-100 variable, not a binary toggle.
- `rosebud` replacing `klapaucius` after a patch shows the game was actively updated post-launch.

---

## 19. Base Game vs. Expansion Packs

Features that are **NOT** in the base game (added by expansions):

| Feature | Expansion |
|---|---|
| Grim Reaper NPC | Livin' Large |
| Guinea Pig Disease (death type) | Livin' Large |
| Genie Lamp | Livin' Large |
| Tragic Clown Painting (functional) | Livin' Large |
| Chemistry Set (Concatenation Station) | Livin' Large |
| Servo (robot) | Livin' Large |
| Roaches | Livin' Large |
| Heart-shaped Vibrating Bed | Livin' Large |
| Party throwing (phone interaction) | House Party |
| Party-specific NPCs (Mime, DJ, etc.) | House Party |
| Downtown / Community Lots | Hot Date |
| Daily/Lifetime dual relationship bars | Hot Date |
| 15-interest system (expanded from 8) | Hot Date |
| Restaurants and shops | Hot Date |
| Vacation destinations | Vacation |
| Pets (cats, dogs) | Unleashed |
| Old Town neighborhood | Unleashed |
| Studio Town / Fame system | Superstar |
| Magic system / Toadification death | Makin' Magic |
| Additional career tracks (11 total added across all EPs) | Various |

**Base game confirmed features:**
- 8 motives, 6 skills, 10 career tracks, 5 personality traits
- 8 interests (Travel, Money, Politics, 60s, Weather, Sports, Music, Outdoors)
- 4 death types (Starvation, Fire, Drowning, Electrocution)
- Single neighborhood with 10 lots
- ~150 buy mode objects across 8 categories
- Single relationship bar (-100 to +100)
- No community lots (residential only)
- No aging beyond baby-to-child
- No Grim Reaper (Sims simply die and leave tombstone/urn)
- No weekends or named days of the week

---

## 20. Open Questions / Unverified

1. **Exact mood weighting formula**: Sources confirm weighted averaging with Hunger/Energy/Bladder weighted highest, but no source provides the exact multiplier values for each motive. The weights appear to increase dynamically as a motive drops lower.
2. **Precise need decay rates per Sim-minute**: The Prima guide references decay rates during work but exact per-minute decay for each need in idle/active states is not publicly documented. Decay rates are embedded in SimAntics bytecode (BHAV resources).
3. **Skill gain rate per Sim-hour**: Base rate is ~3--4 Sim-hours per point with personality acceleration up to ~2x, but exact per-level escalation values are not documented.
4. **Cooking fire probability per skill level**: Community consensus is ~15%/5%/1%/0% for skills 0/1/2/3+. Not datamined -- these are observational estimates.
5. **Relationship interaction success formula**: Success depends on relationship + mood + personality + interests, but the exact formula combining these factors is embedded in interaction BHAV trees, not publicly documented.
6. **Bill percentage**: Bills are based on total lot value and arrive every 3 days. Community estimates range from 1--3% of lot value per bill cycle. Exact percentage not confirmed.
7. **Object advertising strength values**: The system is confirmed (0--50 scale per motive per object) but per-object advertising tables are embedded in game data files (TTAB resources), not in public documentation.
8. **Zodiac sign defaults**: The values shown in §7.2 are sourced from community guides and cross-referenced across multiple sources. Minor discrepancies exist for some signs.
9. **Electrocution risk at Mechanical 0**: Sources unanimously describe it as "very high" or "almost certain" but exact percentage varies (50% to 100% across different guides). The graduated risk table in §8.1 uses ranges to reflect this uncertainty.
10. **Mad Scientist top salary**: One source shows S870, another S1,000. The S1,000 figure is used in §5.2 pending verification against game files.
11. **Entertainment Superstar salary**: One source shows S1,100, another S1,400. The higher figure is used in §5.2 pending verification.
12. **Exact social interaction point values**: Values in §6.3 are from one community source. Exact values may vary by game version or be modified by personality/mood factors not captured.
13. **Room score formula**: How Room motive is calculated from object Room ratings, lighting, cleanliness, and wall/floor decoration is not precisely documented. The system appears to be additive with modifiers for messes and room size.
14. **Starvation timer**: Spec uses ~24 Sim-hours after Hunger hits -100. Some sources cite 18 hours, others 48. The timer may vary based on other motive states (conflicting sources).

---

## 21. References

### Guides & Wikis
- [The Sims Prima Official eGuide](https://archive.org/details/The_Sims_Prima_Official_eGuide) -- Internet Archive (most detailed source for object values, formulas)
- [The Sims: Livin' Large Prima Official eGuide](https://archive.org/stream/The_Sims_Livin_Large_Prima_Official_eGuide/) -- Internet Archive
- [The Sims Zone Career Index](https://www.thesimszone.co.uk/extras/index.php?ID=48) -- Complete career tables
- [The Sims Zone NPC Guide](https://www.thesimszone.co.uk/extras/index.php?ID=73) -- NPC details and schedules
- [The Sims Wiki (Fandom)](https://sims.fandom.com/) -- General mechanics reference
- [StrategyWiki: The Sims](https://strategywiki.org/wiki/The_Sims) -- Skills and personality
- [Teoalida: The Sims 1 Career Tracks](https://www.teoalida.com/thesims/the-sims-1-career-tracks/) -- Salary data
- [Teoalida: The Sims 1 Neighborhoods](https://www.teoalida.com/thesims/the-sims-1-neighborhoods/) -- Lot and family data
- [SimEchoes](https://www.simechoes.org/simechoes1/info/moods.html) -- Cooking formula and motive mechanics
- [Captain Packrat: Sims Cheats & Tips](https://www.captainpackrat.com/Misc/simcheats.htm) -- Cooking formula, fire/electrocution percentages, equipment modifiers
- [Wikipedia: The Sims](https://en.wikipedia.org/wiki/The_Sims_(video_game)) -- General mechanics overview

### Gameplay Guides
- [TheGamer: All Needs/Motives](https://www.thegamer.com/sims-1-all-needs-motives-keep-happy/) -- Need system overview
- [TheGamer: Every Death Ranked](https://www.thegamer.com/sims-1-every-death-ranked/) -- Death mechanics
- [TheGamer: How to Make Friends](https://www.thegamer.com/sims-1-make-friends-meet-sims-interests/) -- Social interaction and interests
- [TheGamer: Career Overview](https://www.thegamer.com/sims-1-all-job-paths-careers-salaries/) -- Career track data
- [TheGamer: Creating a Sim](https://www.thegamer.com/the-sims-1-how-to-create-a-sim-personality-traits-star-signs/) -- Personality and zodiac data
- [TheGamer: Fire Prevention](https://www.thegamer.com/sims-1-fire-prevention-extinguish-alarm/) -- Fire mechanics
- [ScreenRant: Romance Guide](https://screenrant.com/sims-1-romance-marriage-babies-guide/) -- Romance and baby mechanics
- [ScreenRant: Friendship Guide](https://screenrant.com/sims-1-how-to-make-keep-friends/) -- Relationship thresholds
- [ScreenRant: Appliance Investment](https://screenrant.com/sims-1-invest-appliances-improve-hunger-tired-fun/) -- Object ratings and prices
- [Drew1440 Blog](https://drew1440.com/2021/12/30/the-sims/) -- Family and gameplay overview
- [Corylea: How to Make Friends in The Sims 1](http://corylea.com/HowToMakeFriendsInTheSims1.html) -- Detailed interaction guide

### Community & Modding
- [FreeSO (GitHub)](https://github.com/riperiperi/FreeSO) -- Open-source reimplementation of The Sims Online engine; SimAntics bytecode reference
- [TSO Mania Guides](https://www.tsomania.net/gameguides/) -- Relationship system, skills, interactions
- [TSO Mania Buy Mode Catalog](https://www.tsomania.net/catalogs/buy.php) -- Object catalog with prices and motive ratings
- [GMTK: The Genius AI Behind The Sims](https://gmtk.substack.com/p/the-genius-ai-behind-the-sims) -- Advertisement system and autonomous behavior analysis
- [Steam Community: Perfecting Your Personality](https://steamcommunity.com/sharedfiles/filedetails/?id=3432470319) -- Personality trait effects
- [Steam Community: Ultimate Sims 1 Guide](https://steamcommunity.com/sharedfiles/filedetails/?id=3418368145) -- General mechanics

### Technical
- [SimAntics (SimsTek Wiki)](https://simstek.fandom.com/wiki/SimAntics) -- BHAV bytecode documentation for object behavior scripting
- [Object TTAB Tutorial](http://www.woobsha.com/diy-TTABTutorialMac.htm) -- Object advertising/motive advertisement data structure
- [OBJD Documentation (SimsWiki)](https://simswiki.info/wiki.php?title=OBJD) -- Object data file format, catalog price and motive rating fields
