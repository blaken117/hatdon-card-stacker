# Hatdon Card Stacker

A Unity experiment about building structures from playing cards using two virtual hands and precise positioning.

## Status

Deck and mouse-control prototype with a minimal four-icon HUD and orientation-dependent air resistance. Draw cards into two free hands, position and lock each, then commit both into gravity and contact physics. Draw the next pair while earlier cards remain on the table. This is not a finished game.

## Open the project

1. Install **Unity 6000.3.8f1** through Unity Hub and activate an eligible licence.
2. Clone this repository and add its root folder in Unity Hub.
3. Open the project. Unity generates its local Library and editor settings.
4. Open `Assets/_Project/Scenes/CardStacker.unity`.
5. To recreate the starting scene, choose **Hatdon > Card Stacker > Create Setup Scene** (this replaces it after confirmation).

Press **Play** to start with an empty table and a 52-card deck. The scene's two hidden card templates supply the first pair. No Asset Store assets or online services are required.

## Current controls

The playing area uses green felt on a walnut desktop, with a reading lamp, deck and plant. Cards have original ace-of-spades and seven-of-hearts artwork, patterned backs and rounded paper edges. The deck currently repeats these two face designs rather than printing 52 unique faces. Dressing applies automatically on Play. The felt, wooden desktop and floor provide physical surfaces; the deck has a picking trigger.

| Input | Action |
| --- | --- |
| Hover over a card | Subtle blue outline |
| Drag from deck | Draw one card into a free hand and pull it onto the table |
| Left-click a card | Select it; gold outline; drag to adjust a held card |
| Left-click empty table / Esc | Deselect without changing held/released state |
| Tab | Switch hand; reopen its lock for adjustment |
| First icon / W | Mouse drag moves across the camera view |
| Second icon / Q | Mouse up/down moves nearer/farther along the view direction |
| Third icon / E | Mouse drag rotates around the card's fixed midpoint |
| Fourth icon / T | Mouse left/right twists around the viewing axis |
| Shift while dragging | One-tenth movement or rotation sensitivity |
| L | Lock a hand at its current pose; still held |
| Enter | Release both cards when both hands are locked |
| R | Clear all cards and refill the deck |
| F8 | Clear the table and replay a fresh random 52-card drop test |
| Right-drag | Orbit selected card, or table centre |
| Middle-drag | Pan |
| Mouse wheel | Zoom within near/far limits |
| F | Focus selected card, or table centre |
| Home | Restore initial camera |

The only HUD elements are four large icons centred along the bottom: move, depth, rotate and twist, with the active mode highlighted. Click an icon or use W/Q/E/T. The first drag from the deck pulls a card onto a horizontal plane 60mm above the tabletop. Subsequent W drags move across the camera view; use Q for depth. E uses a virtual trackball, while T gives a separate twist. Shift reduces sensitivity to one tenth. Two hands can be occupied at once; selecting a locked hand reopens it. Selecting a card does not move the camera.

Locking a card shows a faint, faceted ghost hand with four fingers, thumb and wrist. It stays above the card's upper extent and points vertically down, independently of card tilt; its palm turns toward the camera horizontally. Near-clear fill keeps the card readable. These are visual indicators with no colliders, shadows, joints or forces. Unlocking or committing removes them.

The toolbar uses original filled SVG designs, with transparent textures baked for Unity's immediate-mode UI. SVG sources are under `Tools/UIIcons`; regenerate them and the matching textures with `Tools/generate_solid_ui_icons.py`.

Moves and rotations stop at physical surfaces and other cards, including earlier placed cards. Contact uses small swept steps with a refined stopping point. Both occupied hands must be locked, and overlapping placements refuse commit. Commit changes both bodies to dynamic without moving them or adding a release impulse, then frees the hands for the next draws. Released cards remain simulated and inspectable; mouse adjustment applies only to held cards. Reset clears the table.

The locked-card check is explicitly tested: dragging the other card through it stops the moving card and leaves the locked pose unchanged. A separate failed-placement test offsets the second card in depth so their upper edges miss; after commit, both fall flat through gravity without an added push. The successful aligned triangle and failed placement use the same physics settings.

The rigid-card model uses 63x88x0.32mm cards at 1.8g, 240Hz physics, finite paper/felt friction and zero bounce. Sixteen surface patches apply still-air drag using point velocity, so broadside motion meets more resistance than edge-first motion and tumbling produces resisting torque. Uniform linear/angular damping is zero. This flat-plate approximation is experimental, not measured card-stock calibration; card flex, humidity, edge deformation, lift and unsteady flutter are not simulated. No joints, frozen constraints, automatic balance or post-release pose corrections hold a structure up. Release is still a joint two-card commit.

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
