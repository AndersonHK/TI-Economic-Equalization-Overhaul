# Hypothetical role-aware AI hull-appearance selection

Status: hypothetical future design, not part of the minimum fuel-cap AI pass.

## Purpose

Future graphical hull variants may differ in measured fuel volume, engine and
reactor-bay scale, hull mass and dimensions, and resulting target profile. If
those differences become mature gameplay inputs, an AI could treat appearance
as a constrained design choice rather than as a cosmetic result assigned after
the ship has already been designed.

The sound dependency order is **candidate drive -> appearance -> completed
ship**. Drive architecture must be known before appearance is evaluated because
De Laval, magnetic, and pulsed installations do not consume graphical engine
space in the same way. Appearance must then be locked before reactor choice,
mass, crew, fuel capacity, acceleration, delta-v, and combat scoring are
calculated.

This proposal deliberately remains separate from the minimum implementation.
It adds choice, randomness, role weighting, and new cache requirements that are
not necessary merely to make the AI obey a fuel cap.

## Candidate order

Do not choose one global winning drive before looking at appearances. Instead,
evaluate appearances conditionally for every candidate drive and compare the
resulting completed candidates:

```text
for each candidate drive:
    resolve the drive architecture
    evaluate each available appearance with that drive
    discard physically invalid appearances
    rank the remaining appearances for the requested role
    choose between the best two suitable appearances
    lock the chosen appearance
    finish and score that drive/appearance candidate

select the best completed candidate across drives
```

This ordering respects the drive-to-appearance dependency without hiding a
potentially superior drive/appearance combination behind an early global drive
choice.

## Drive-architecture policy

Use stable template metadata rather than drive-name parsing:

- `TIDriveTemplate.nozzle` distinguishes `DeLaval`, `Magnetic`, and `Pulsed`
  graphical families.
- `TIDriveTemplate.driveClassification` distinguishes fission-pulse from
  fusion-pulse drives.
- `TIDriveTemplate.thrusters` identifies the installed cluster variation.

The proposed fission-pulse exception is explicit:

```text
if driveClassification == Fission_Pulse:
    graphical drive scale = 1
    allowed installed cluster = x1
else:
    graphical drive scale = variant/nozzle-specific measured scale
    normal cluster rules apply
```

Do not apply the fixed rule to every `Nozzle.Pulsed` drive. Fusion-pulse
architecture may receive a different policy later.

All appearance-sensitive values should come from a shared immutable snapshot
keyed by the ship's hull and resolved appearance, with the candidate drive
supplied where engine geometry depends on nozzle family. Do not mutate shared
`TIShipHullTemplate` fields to represent per-ship appearance.

## Appearance evaluation

Each `(hull, appearance, drive)` evaluation should expose at least:

- maximum legal propellant tanks after the role payload reservation;
- maximum attainable delta-v at that cap;
- cruise and combat acceleration;
- graphical drive scale and drive mass/power consequences;
- availability of a compatible reactor within the measured reactor bay;
- dry and wet mass;
- target cross-section or the future canonical hit-size metric; and
- construction and propellant cost.

Hard feasibility precedes preference. Reject an appearance when its model is
unavailable, it cannot carry at least one tank, it cannot support any legal
drive/reactor pairing, or it misses a hard operational floor that another
appearance can meet.

## Role-sensitive ranking

The ranking should consume outcomes rather than raw hull or fuel volume:

| Role tendency | Leading appearance criteria |
|---|---|
| Explorer, colony, and long-range transport | Required route or delta-v, attainable delta-v, then fuel efficiency and cost |
| Interceptor, strike, and patrol | Minimum delta-v, acceleration, then target profile |
| Standoff and bomber | Range floor, acceleration floor, then target profile and cost |
| Defender and protector | Acceleration, survivability/profile, then sufficient local delta-v |

The exact weights are deferred until variant mass, dimensions, and hit-size
rules exist together. A lexicographic set of hard floors followed by normalized
scores is safer than adding raw cubic metres, tonnes, acceleration, and target
area directly.

## Controlled randomness

Randomness should create variety only among sound candidates:

1. sort all feasible appearances by the role score;
2. retain at most the best two;
3. require the second appearance to meet every hard floor and remain within a
   defined score tolerance of the first; and
4. choose between them with a biased weight such as 70/30 or a calibrated
   softmax.

Use a reproducible game-state seed that includes faction, role, hull, drive,
and design iteration. Repeated evaluation of the same design attempt must not
change appearance midway through construction or validation.

After appearance is locked, exact module and crew choices may reveal that the
provisional capacity estimate was optimistic. In that case abandon the
candidate and retry its second-ranked appearance or another drive. Never change
appearance silently in the tank or armor tuning loop.

## Cache implications

Vanilla's human designer caches propulsion statistics by role, hull, and drive.
That is insufficient when one drive can randomly receive different
appearances. A future implementation must either:

- include resolved appearance in the cache key; or
- use an appearance-independent best-achievable envelope only for early drive
  pruning and store the selected candidate's actual statistics separately.

Aliens require no special selection logic while their hulls retain one
appearance. The same pipeline naturally degenerates to a one-item appearance
list.

## Deferred acceptance cases

- A long-range role normally chooses one of the two appearances with the best
  legal attainable delta-v for its selected drive.
- A short-range interceptor may prefer a smaller, lighter target even when a
  larger variant carries more fuel.
- Fission-pulse candidates remain x1 and graphical scale 1 on every appearance.
- Magnetic and De Laval candidates consume their own measured appearance
  scales.
- The second-ranked appearance is never selected when it violates a hard role,
  fuel, or reactor constraint.
- Re-evaluating one design attempt produces the same random choice and does not
  corrupt the drive-stat cache.

