# Hatdon Card Stacker

A Unity experiment about building structures from playing cards using two virtual hands and precise positioning.

## Status

Lock-and-commit physics prototype. Position each card, lock each hand, then commit both to release them into gravity and contact physics. Two-card triangles have been verified after release; this is not a finished game.

## Open the project

1. Install **Unity 6000.3.8f1** through Unity Hub and activate an eligible licence.
2. Clone this repository and add its root folder in Unity Hub.
3. Open the project. Unity generates its local Library and editor settings.
4. Open `Assets/_Project/Scenes/CardStacker.unity`.
5. To recreate the starting scene, choose **Hatdon > Card Stacker > Create Setup Scene** (this replaces it after confirmation).

The setup scene contains a table, camera, light and two held test cards. Press **Play** to use selection, the camera and the on-screen control guide. No Asset Store assets or online services are required.

## Current controls

The playing area now uses green felt on a walnut desktop, with a small reading lamp, spare deck and plant in a quiet room. The two held cards show original ace-of-spades and seven-of-hearts artwork, with matching navy patterned backs and rounded paper edges. Dressing applies automatically on Play in the existing scene. No colliders or rigid bodies are added by the artwork.

| Input | Action |
| --- | --- |
| Hover over a card | Subtle blue outline |
| Left-click a card | Select it; gold outline |
| Left-click empty table / Esc | Deselect without changing held/released state |
| Left / Right hand buttons / Tab | Select hand; reopen its lock for adjustment |
| W / Move button | Show X/Y/Z movement handles; drag an arrow |
| E / Rotate button | Show X/Y/Z rotation rings; drag a ring |
| Shift while dragging | One-tenth movement or rotation sensitivity |
| L / Lock buttons | Lock a hand at its current pose; still held |
| Enter / Commit both | Release both cards when both hands are locked |
| R / Reset cards | Restore the starting held cards for another attempt |
| Right-drag | Orbit selected card, or table centre |
| Middle-drag | Pan |
| Mouse wheel | Zoom within near/far limits |
| F | Focus selected card, or table centre |
| Home | Restore initial camera |
| Front / Side / Overhead buttons | Alignment views |

Selecting a card does not move the camera. The camera stays above the tabletop. Both hand slots currently start occupied. Adjusting one card keeps the other held in place. Position is shown in world millimetres and rotation in world Euler degrees. Red is X, green is Y (height), blue is Z. Choose a different camera view if an axis is viewed end-on.

Moves and rotations stop when the card meets the table or other card. Contact is checked in small swept steps with a refined stopping point; cards do not push each other while held. Selecting a locked hand reopens it; lock it again before committing. Both locks are required, and overlapping placements are rejected. Commit changes both bodies to dynamic in the same update without moving them or adding a release impulse. Released cards can be inspected but cannot be repositioned with handles. Reset to try again.

The rigid-card model uses 63x88x0.32mm cards at 1.8g, 240Hz physics, finite paper/felt friction, zero bounce and low linear/angular damping. These are initial approximation/tuning values, not measured material calibration. Card flex, humidity, edge deformation and orientation-dependent aerodynamics are not simulated. No joints, frozen constraints, automatic balance or post-release pose corrections hold a structure up. Additional cards and individual release remain future work.

## Intended interaction

- Move and rotate one card precisely, including fine adjustment.
- Keep up to two cards held, one per virtual hand.
- Switch the active hand without releasing the other card.
- Lock each hand, then release both together and let gravity/contact determine the result.
- Orbit the camera and inspect alignment from different sides.

Two-card tents at 24, 28 and 32 degrees from vertical stayed standing for 20 simulated seconds in regression tests. Both cards had real table/card contacts; unsupported tilted cards fell and an impulse could disturb the tent. The same 28-degree pose was assembled through the contact-aware placement API. Rigid cards remain the initial approximation; card flex is outside this prototype.

## Version history and dev logs

Small, meaningful commits preserve each working change. Reviewed dev-log milestones receive annotated tags; setup is not a released gameplay prototype. See `Docs/DEVELOPMENT.md` and `CHANGELOG.md`.

Dev-log takes preserve the complete Windows build alongside a source archive, capture manifest and clean gameplay footage. The recording helper runs an explicitly requested automated demonstration of existing controls; it does not add card-placement gameplay.

Track `Assets` (including `.meta` files), `Packages` and `ProjectSettings`. Generated caches, builds, local settings and credentials are excluded. Git is the source of truth; Unity Version Control is not configured alongside it.

## Reuse

Deck and tabletop artwork is generated by `Tools/generate_tabletop_textures.py` (Pillow/NumPy). The baked PNGs are included; Python is not required to play. The generator uses the locally installed Georgia Bold font for raster lettering; no font file or third-party deck artwork is redistributed.

Public source is available for inspection. No open-source reuse licence has been selected yet; no additional reuse permissions are granted by this repository. Unity and third-party components remain under their own terms.
