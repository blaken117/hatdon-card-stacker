# Hatdon Card Stacker

A Unity experiment about building structures by positioning playing cards in pairs.

## Status

Choose Practice or one of ten tower challenges, from a leaning pair to a four-floor terrace. Classic challenges are also available from the menu. Challenges have a toggleable guide and complete once the released cards match it and stay in place for three seconds. Each table saves locally so you can return to it later.

Draw and position the first card, then click **Commit card** to hold it in place. Repeat with the second card, then click **Commit pair** to release them together. Use **Release card** when placing one card on its own.

## Open the project

1. Install **Unity 6000.3.8f1** through Unity Hub and activate an eligible licence.
2. Clone this repository and add its root folder in Unity Hub.
3. Open the project. Unity generates its local Library and editor settings.
4. Open `Assets/_Project/Scenes/CardStacker.unity`.
5. To recreate the starting scene, choose **Hatdon > Card Stacker > Create Setup Scene** (this replaces it after confirmation).

Press **Play** to open the main menu. Each new table starts with a 52-card deck.

## Current controls

The playing area uses green felt on a walnut desktop, with a reading lamp, deck and plant. Cards have original ace-of-spades and seven-of-hearts artwork, patterned backs and rounded paper edges. The deck currently repeats these two face designs rather than printing 52 unique faces. Dressing applies automatically on Play. The felt, wooden desktop and floor provide physical surfaces; the deck has a picking trigger.

| Input | Action |
| --- | --- |
| Hover over a card | Subtle blue outline |
| Drag from deck | Draw one card into a free hand and pull it onto the table |
| Left-click a card | Select it; gold outline; drag to adjust a held card |
| Left-click empty table | Deselect without changing held/released state |
| Esc | Return to the main menu or close a dialog |
| Shift+Esc | Open the paused admin and recording tools |
| Space / . | Play/pause / advance one frame while paused |
| F10 | Toggle HUD and selection markers for clean takes |
| First icon / W | Drag across the camera view and scroll during the same drag to adjust depth |
| Second icon / hold R during a drag | Rotate around the card's fixed midpoint. Release R to return to the selected tool. |
| Third icon / hold T during a drag | Move the mouse left/right to tilt around the viewing axis. Release T to return to the selected tool. |
| Shift while dragging | One-tenth movement or rotation sensitivity |
| Release left mouse button | End the drag and leave the card ready for adjustment |
| Commit card button | Hold the selected card in place. Select it again to adjust it. |
| Commit pair button | Release both cards into physics after committing each one |
| Release card button | Release one held card on its own |
| Reset cards button | Confirm clearing the current table and refilling its deck |
| Right-drag | Orbit selected card, or table centre |
| Middle-drag | Pan |
| Mouse wheel outside a card drag | Zoom within near/far limits |
| F | Focus selected card, or table centre |
| Home | Restore initial camera |

The HUD shows Move, Rotate and Tilt icons beside Reset cards and prompts the next placement. Click an icon to select it, or press W for Move. The first drag from the deck pulls a card onto a horizontal plane 60mm above the tabletop. Subsequent Move drags follow the camera view, with the wheel controlling depth. Shift reduces sensitivity to one tenth. You can adjust the first card again before drawing the second.

The toolbar uses original filled SVG designs, with transparent textures baked for Unity's immediate-mode UI. SVG sources are under `Tools/UIIcons`; regenerate them and the matching textures with `Tools/generate_solid_ui_icons.py`.

Moves and rotations stop at physical surfaces and other cards, including earlier placed cards. Contact uses small swept steps with a refined stopping point. If cards overlap at release, adjust either card and let go again. A valid pair enters physics at the positions you set, without a release impulse. Reset clears the table.

The locked-card check is explicitly tested: dragging the other card through it stops the moving card and leaves the locked pose unchanged. A separate failed-placement test offsets the second card in depth so their upper edges miss; after commit, both fall flat through gravity without an added push. The successful aligned triangle and failed placement use the same physics settings.

The rigid-card model uses 63x88x0.32mm cards at 1.8g, 240Hz physics, finite paper/felt friction and zero bounce. Sixteen surface patches apply still-air drag using point velocity, so broadside motion meets more resistance than edge-first motion and tumbling produces resisting torque. Uniform linear/angular damping is zero. This flat-plate approximation is experimental, not measured card-stock calibration; card flex, humidity, edge deformation, lift and unsteady flutter are not simulated. No joints, frozen constraints, automatic balance or post-release pose corrections hold a structure up. Release is still a joint two-card commit.

## Intended interaction

- Move and rotate one card precisely, including fine adjustment.
- Keep up to two cards held, one per virtual hand.
- Switch the active hand without releasing the other card.
- Place the first card, then let go of the second to release the pair together.
- Orbit the camera and inspect alignment from different sides.

Two-card tents at 24, 28 and 32 degrees from vertical stayed standing for 20 simulated seconds in regression tests. Both cards had real table/card contacts; unsupported tilted cards fell and an impulse could disturb the tent. The same 28-degree pose was assembled through the contact-aware placement API. Rigid cards remain the initial approximation; card flex is outside this prototype.

## Version history and dev logs

Small, meaningful commits preserve each working change. Reviewed dev-log milestones receive annotated tags; setup is not a released gameplay prototype. See `Docs/DEVELOPMENT.md` and `CHANGELOG.md`.

The user records new dev-log footage. Each feature keeps source/build notes plus a shot in the Esc menu and `Docs/RECORDING-CHECKLIST.md`. The menu prepares repeatable scenes, controls pause/speed, hides the HUD, saves a camera view and tracks recorded/pending takes locally. It does not record video. Before final editing, check outstanding shots and verify the user's clips. Earlier automatic captures remain archived; no new gameplay footage is generated by ordinary updates.

Track `Assets` (including `.meta` files), `Packages` and `ProjectSettings`. Generated caches, builds, local settings and credentials are excluded. Git is the source of truth; Unity Version Control is not configured alongside it.

## Reuse

Deck and tabletop artwork is generated by `Tools/generate_tabletop_textures.py` (Pillow/NumPy). The baked PNGs are included; Python is not required to play. The generator uses the locally installed Georgia Bold font for raster lettering; no font file or third-party deck artwork is redistributed.

Public source is available for inspection. No open-source reuse licence has been selected yet; no additional reuse permissions are granted by this repository. Unity and third-party components remain under their own terms.
