# Changelog

## Esc admin tools and manual recording workflow — 2026-11-06

- Adopt user-recorded footage: retain feature notes and a shot checklist, remind the user before final editing, and stop automatic gameplay captures.
- Add an Esc menu with pending/recorded shots, 52 Card Pickup preparation, repeatable seeds, a ghost-hand scene and normal gameplay setup.
- Add manual release/pause, slow motion, single-frame stepping, clean HUD, camera framing/bookmark and local recording notes/clip paths.
- Split pickup preparation from release so the user can start a screen recorder before the cards fall.
- Add a clean release-build option without legacy recording helpers.

## Full ghost hands, solid icons and 52-card drop — 2026-11-06

- Replace the two-digit grip with a faceted four-finger/thumb hand and longer wrist based on the supplied reference.
- Keep ghosts above each card and pointing down, independently of card rotation; retain low opacity and no physical support.
- Replace stroke icons with original filled SVG designs and matching Unity UI textures.
- Add an explicit F8 test that clears the table, releases 52 cards from separated random airborne poses, and consumes the displayed deck.
- Add a fresh capture showing the hand revision followed by a complete 52-card drop, plus spawn/fall/reset verification.

## Minimal HUD and air resistance — 2026-11-06

- Replace all HUD panels/text with four large bottom-centred mode icons: move, depth, rotate and twist.
- Highlight the active mode; retain W/Q/E/T, Shift, L, Tab, Enter, R and camera shortcuts.
- Free former panel areas for scene picking; only the four actual button rectangles block input.
- Replace uniform body damping with orientation-dependent air resistance distributed over sixteen surface patches, including resistance to tumbling.
- Verify broadside versus edge-first drops, dissipative force, sleep, balance, contact, deck/hand lifecycle and camera controls.

## Ghost hands and mouse-control recordings — 2026-11-03

- Show a faint cartoon finger/thumb grip only while each hand is locked; hide it on unlock, release or reset.
- Use transparent fill and soft rims with no shadows or physics components.
- Add opt-in captures that draw from the deck, assemble leaning cards with the actual W/Q/E/T pointer paths, and test tabletop and locked-card contact stops.
- Keep all physical card settings and player manipulation behavior unchanged.

## Deck and mouse controls — 2026-11-03

- Start with an empty table and a 52-card deck; drag one card into each free hand.
- Replace player-facing axis handles with W view-plane movement, Q depth, E midpoint trackball rotation and T twist, all driven by mouse drags with Shift precision.
- Commit frees both hands while previous cards remain dynamic; new draws respect their contact surfaces.
- Reset clears drawn cards and refills the deck. Add deck count and updated on-screen instructions.
- Add physical wood/floor surfaces, deck picking and regression coverage for the new draw/control cycle.
- Preserve the existing card mass, paper/felt friction and release physics. Face art currently repeats the two existing designs.

## Locked-contact and failed-placement verification — 2026-11-03

- Added explicit assertions that dragging into a locked card stops movement without pushing the locked card.
- Added a deliberately depth-misaligned placement that falls after commit.
- Added a recording showing the blocked drag, misalignment, joint release and fall.
- Gameplay and physics tuning are unchanged; the previous successful triangle remains preserved.

## Lock, commit and physical balance — 2026-11-03

- Added independent hand locks, reopen-to-adjust, two-lock commit and reset.
- Commit releases both cards into gravity without changing their poses or adding an impulse; handles cannot move released cards.
- Added paper/felt physics materials, 1.8g mass, 0.32mm thickness, physical box inertia, low damping and fine contact tolerances.
- Added 240Hz simulation and per-card solver settings for thin-body contacts.
- Corrected collision queries to use rigid-body poses instead of interpolated display transforms.
- Verified overlap rejection, unsupported falls, API-assembled triangle, three lean angles, actual contacts and disturbance by an impulse.
- Preserved the green felt visual surface at the physical tabletop height.

## Tabletop artwork — 2026-11-03

- Added original ace-of-spades / seven-of-hearts faces and matching patterned backs.
- Added rounded visual card geometry and paper edges; box colliders remain unchanged.
- Added felt, walnut grain, stitching, warm lamp, spare deck, plant and room dressing.
- Kept selection, camera, manipulation, mass and contact behavior unchanged.
- Added an art demonstration showing both face and back textures.

## Recording revision — 2026-11-03

- Added a faster 26-second automated demonstration with visible cursor drags, varied timing, pauses and corrections.
- Card manipulation uses the real screen-handle drag path; gameplay is unchanged.
- Preserved the earlier 45-second take and its build/source archive.

## Card manipulation — 2026-10-27

- Added draggable world-axis movement arrows (W) and rotation rings (E).
- Added Shift fine adjustment at 10% sensitivity, live millimetre/degree readouts and colour-coded axis labels.
- Added swept contact checks for translation and rotation against the table and other card, including thin-face crossing.
- Added contact feedback, bounded placement, and drag cancellation on focus loss.
- Kept the other card held and camera controls separate; no card release or balancing yet.
- Added movement tests and a second dev-log demonstration sequence.

## Dev-log capture workflow — 2026-10-27

- Added an opt-in automated demo recorder for existing selection and camera controls.
- Added a clean capture build option and fixed-frame 1920x1080 / 30fps video output.
- First milestone preserves source, complete Windows build and a selection/camera take.

## Selection and camera — 2026-10-27

- Added card picking, blue hover outline and gold selection outline.
- Added two hand buttons, with deselection preserving both held cards.
- Added right-drag orbit, middle-drag pan, bounded zoom, F focus and Home reset.
- Added front, side and overhead views and an always-visible control guide.
- Added an explicit development-player smoke test and Windows preview build entry point.
- Card movement, rotation and release remain the next milestone.

## Setup — 2026-10-27

- Created a standalone Unity project structure and public-source documentation.
- Added an editor command to create and validate a minimal table/two-card scene.
- Configured Git exclusions for Unity caches, builds and credentials.
- Interactive controls and validated balancing are not implemented yet.
- After licence activation, compiled successfully in Unity 6000.3.8f1 and generated/validated the starting scene, materials, build scene list and project settings.
