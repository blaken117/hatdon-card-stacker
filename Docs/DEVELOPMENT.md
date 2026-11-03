# Development record

## 2026-11-03 — Prototype 006: ghost grips and further mouse-control takes

User requested more recordings of two cards leaning against one another and attempts to move through another card/the table. They also requested translucent cartoon hands while locked, clarified as faint ghost hands that do not obstruct the camera. A procedural palm, one finger and one thumb follow each locked card in world space. The grip meshes use independent roots so card thickness scaling and future card cloning do not distort/duplicate them. A rim-weighted transparent shader keeps facing surfaces nearly clear; ghosts have no shadows, colliders, rigid bodies, joints or forces. Reopening a hand, committing or resetting hides the visual. Left/right use subtle cool/warm tints.

The new opt-in mouse-trial capture decomposes each orientation target into a T twist and two E trackball drags, followed by Q depth and W view-plane position drags. All changes call the actual BeginPointerDrag/DragPointerTo path with screen-picked cards. Target calculations are capture-only cursor guidance; they do not assign card poses or add a gameplay snap. The second draw is carried behind the first locked card to avoid crossing it en route. Contact takes request below-table motion and a 12mm crossing into the locked card, then reverse away and return before commit. Grips must be visible while locked and absent for the released simulation. Existing Prototype 005 source/build/take remains untouched.

Validation: updated deck/mouse tests verify ghost visibility on lock, disappearance on reopening, both grips while both locks are set, and disappearance on commit. The full balance suite also passes. The fresh 24/32-degree takes are 1240 frames (41.33 seconds) each; the 28-degree contact take is 1600 frames (53.33 seconds), all 1920x1080/30fps and silent. Each pair remains standing for 13.67 released seconds; maximum drift is 0.0956/0.0199/0.0846mm respectively. The flat card stops at 0.1591mm centre height when dragged toward a point 12mm below the table; the 12mm card-crossing attempt leaves the locked body's position/rotation unchanged. The approach view zooms out before staging, then focuses again for the final lowering so the card remains in frame and the next grab stays outside the HUD. Placement honours its contact stop rather than forcing exact target coordinates. Capture startup sets background execution before its first yield so multiple recording windows can initialise independently.

## 2026-11-03 — Prototype 005: deck and mouse controls

The table starts empty with a 52-card stock. Dragging the visible deck draws exactly one card into the preferred free hand, or the other hand if occupied. Full hands refuse another draw without consuming stock. The first pickup uses a horizontal plane 60mm above the tabletop; later W drags use the camera-facing plane through the card. Q maps vertical pointer motion to view depth, E uses a virtual trackball around the unchanged rigid-body centre, and T twists about the viewing axis. Shift scales movement/rotation to one tenth. Camera orbit/pan/zoom remain separate. Player-facing axis arrows/rings are removed; the midpoint marker and world pose readouts remain.

Both locks are still required for joint commit. Committed cards remain dynamic while subsequent draws fill the freed slots. Collision obstacles refresh per placement operation, so newly drawn and previously released cards participate in swept contact checks. Wooden desktop and floor colliders provide support outside the felt; decorative props remain nonphysical and the deck picking trigger is excluded from placement contact. Reset cleans up clones, hides the original templates and refills stock. The stock count is 52 but printed faces currently reuse the existing ace/seven art.

The new automated deck/mouse suite checks empty start, actual stock ray picking, one-card-per-drag, two-hand capacity, view/depth movement, Shift sensitivity, centre-preserving trackball/twist, release and continued drawing, contact with previously placed cards, and full reset. Legacy regression fixtures explicitly create a held pair instead of altering the normal empty start. Existing physical card parameters and lock/fall/triangle tests remain in use.

Validation passed in the Windows player: deck/mouse suite, selection/camera suite, movement/contact suite and full balance suite. The 24/28/32-degree triangles remained standing with maximum drift 0.0121/0.0153/0.0192mm over 20 simulated seconds; the misaligned pair still fell. Both development and clean capture builds compile. A first pickup test exposed a per-step movement cap before reaching the pickup plane; pickup now uses bounded swept increments to reach its plane while respecting contact.

Prototype 005's capture helper uses deck picking and the real pointer-drag paths, then locks/commits a pair and draws a third card. It is an automated demonstration, not human play-test footage. Build, source, footage and test records belong under `DevLogs/Card-Stacker/prototypes/005-deck-mouse-controls/`.

## 2026-11-03 — Locked contact and a failed placement

User asked whether the other card stops at a locked card and requested a visibly misaligned placement that falls. The held movement logic already rejects penetration. The added check keeps the first hand locked, requests a 12mm move through its contact boundary, verifies that the second card stops on its own side and that the locked body has neither moved nor rotated.

The failure fixture uses the same 28-degree lean and all existing physics settings, but offsets the second card 68mm in depth. Their 63mm-wide upper edges miss by 5mm, leaving each inclined card unsupported by the other. Both are locked and committed normally; the regression test checks that both become dynamic and fall flat on the table under gravity. No impulse, forced drop, changed friction or post-release pose assignment is used. Existing aligned-triangle and bump tests remain in the suite.

Failure Take 01 uses actual screen-handle drags to demonstrate the contact stop, move the second card out of alignment, lock and commit both, then observe seven released seconds. It is an automated demonstration, not a human play-test. The successful triangle take/build stays unchanged; the new build, source, footage and checksums belong under `DevLogs/Card-Stacker/prototypes/004-lock-commit-physics/failure-take01/`.

## 2026-11-03 — Prototype 004: hand locks, commit and physical cards

User requested locking each hand, switching back to adjust either locked card, a final joint commit into physics, and a test with two leaning cards forming a triangle. Each lock retains its kinematic hold. Selecting that hand reopens its lock. Both must be locked to commit. The commit rejects material overlap, ends manipulation, and switches both rigid bodies to dynamic before the next physics step, with zero release velocity and no pose correction. After commit, selection is inspection only; Reset restores the initial held poses.

Runtime physics: card size 63x88x0.32mm, mass 0.0018kg, gravity 9.81m/s², 240Hz step, 64 position / 16 velocity iterations per body, physically dimensioned box inertia, 25µm collider contact offsets and a 0.12m/s depenetration-speed cap. Paper static/dynamic friction is 0.48/0.32; felt is 0.70/0.52, combined by Average. Bounce is zero. Linear/angular damping is 0.04/0.08 and sleeping uses a small energy threshold. These are initial plausible rigid-body tuning values, not measured card-stock calibration. There is no flex, humidity, deformation, air-lift or orientation-dependent aerodynamic model. The existing rounded visuals still use rectangular box collision geometry.

A release validation test exposed interpolation-related stale-transform queries. Both held movement and pre-commit overlap checks now use root rigid-body positions/rotations, so overlap is evaluated in physics space. The original hidden table renderer no longer covers the felt; the visible mat is just beneath the collider surface so thin resting cards remain visible.

Validation: unsupported tilted cards fall flat without tunnelling; gravity acceleration is checked; overlaps refuse release; hand locks can be reopened; reset works; no release teleport/impulse is introduced. A triangle assembled through the actual placement API remained standing after commit for 20 simulated seconds. Separate 24/28/32-degree fixtures survived 20 seconds, with maximum centre drift 0.0121/0.0153/0.0380mm and table/other-card contacts on both bodies. No joints, frozen constraints or kinematic support were present after release. A 0.0002Ns impulse at an offset disturbed each structure. Movement and selection/camera regressions also passed.

The capture helper assembles the 28-degree triangle through actual screen-picked BeginDrag/DragTo calls, locks each hand, commits both, then leaves the bodies to simulate for 13 seconds while orbiting for inspection. The helper never assigns a card pose. Its calculated targets are for the automated demonstration only; the playable game has no triangle snap or auto-balance function.

Unity API references used for configuration: https://docs.unity.com/en-us/engine/6000.0/script-reference/unityengine/rigidbody and https://docs.unity.com/en-us/engine/6000.0/manual/physics-section/physics-overview/collision-section/collider-surfaces/class-physics-material . Actual stability claims above are from the local engine tests, not these API references.

## 2026-11-03 — Prototype 003: tabletop artwork

User requested more character in the table/environment and actual playing-card textures. Added five original baked textures: ace of spades, seven of hearts, a symmetric navy patterned back, felt and walnut grain. The deterministic Pillow/NumPy generator is included under Tools. It rasterises lettering from the locally installed Georgia Bold font; no font files, branded deck scans or paid art assets are included.

`CardTableArt` applies the visuals on Play before control initialisation. Cards receive rounded render geometry, separate face/back print surfaces, and paper-coloured edges. Their existing box colliders, dimensions, rigid bodies, masses and poses are preserved. The rounded visual corners are cosmetic; collision remains the existing rectangle approximation. Environment dressing creates meshes and lights only, with no colliders or rigid bodies, so selection rays and held-card contact tests retain their existing physics objects.

The scene uses a green felt mat with stitched margins on walnut, a small warm reading lamp, decorative spare deck, potted plant, table legs, floor, walls and a quiet framed print. The lamp and ambient/window lighting keep attention on the playing area. Existing HUD and camera controls remain. The art capture turns a held card around using the real rotation-ring drag path to show its back, then returns it to its face.

Selection/camera and card-movement regression checks passed in a graphics-enabled Windows development player. Art does not implement release or balance. Preserve the source/build and new visual take under `DevLogs/Card-Stacker/prototypes/003-tabletop-art/`.

## 2026-11-03 — Faster, more natural demonstration take

The user requested a new testing recording with faster pacing and more human-like control. Prototype 002 Take 02 uses a 26-second scripted sequence with a visible arrow cursor, eased approach/drag speeds, short inspection pauses, a small rotation correction, fine adjustment, hand switching, contact testing and a brief orbit. All card adjustments run through screen-picked handles and the actual `BeginDrag`/`DragTo` path. The script does not directly change card poses or bypass contact checks.

This is an automated demonstration styled to resemble normal play, not a recording of a human tester. Gameplay is unchanged. Take 01, its source tag and its build remain untouched. New take/build/source/cue/checksum records live in `DevLogs/Card-Stacker/prototypes/002-card-manipulation/take02/`. Activate it only with `HATDON_CAPTURE`, `-recordDevlog` and `-naturalTake`.

## 2026-10-27 — Prototype 002: card movement and rotation

User approved adding precise movement and rotation of the selected card, Shift fine adjustment, live position/angle feedback and contact checks. The two existing hands remain occupied and held; releasing is intentionally a later milestone.

`CardManipulator` renders screen-sized world X/Y/Z arrows or rotation rings with a small overlay shader. Picking uses projected handle segments, then incremental pointer deltas control an axis. Movement projects pointer delta onto the axis; rotation projects it onto the selected ring tangent. Shift multiplies both by 0.1 without jumping when toggled. W/E and the selected-card panel switch tools. The panel shows world position in millimetres and world Euler angles in degrees. The other hand is never repositioned by the selected card's adjustments.

Placement checks Unity box penetration against the table and other card, sampling at most 0.2mm of translation or 0.2 degrees of rotation per step, with eight bisection refinements at contact. Penetration tolerance is one micrometre. This prevents the thin cards from being teleported through each other even if the requested endpoint is clear. It is a kinematic stop, not a force-based grip, snapping, physical stability or card flex. Placement is bounded near the table. Camera gestures are suppressed during card dragging and focus loss cancels gestures without releasing either card.

Movement verification covers free XYZ translation/rotation, screen picking of arrows/rings, 10x fine movement/rotation, table stops, card-to-card stops, thin-face crossing, rotation-corner contact, moving away from contact and unchanged second-hand pose. The selection/camera regression suite remains applicable.

Both movement and selection/camera suites passed in the Windows development player. A clean capture build recorded Prototype 002 Take 01: 45 seconds at 1920x1080 / 30fps, showing all three movement/rotation axes, actual Shift handle-drag logic, hand switching and contact limits. The selected-card UI and gizmos were visually checked. Preserve it under `DevLogs/Card-Stacker/prototypes/002-card-manipulation/` with the build, source archive, cue sheet and checksums. Release and balance remain unimplemented.

## Dev-log preservation and recording

Meaningful prototypes are preserved as a source commit/tag, complete standalone Windows build, clean 1920x1080 footage, screenshot and local manifest with checksums and cue points. Footage/build archives live outside the public source repository under `DevLogs/Card-Stacker/prototypes/`. Small fixes do not need separate takes unless their behavior is useful to show.

Prototype 001 is selection and camera control. Its first take is a deterministic 42-second demonstration using the real control methods in a running Unity player. It is automated demonstration footage, not a human play-test. The recording build uses `HATDON_CAPTURE` to enable an opt-in `-recordDevlog` runner without the development-build watermark. Normal launch still runs the interactive game; the recording runner activates only with explicit capture arguments. Frames are captured after rendering, including the actual HUD, and encoded at 1920x1080 / 30fps. No game audio exists at this milestone, so the take is silent.

The capture demonstrates hover/select/deselect, both hand slots, focus, orbit, pan, zoom, front/side/overhead and reset. The two cards remain held throughout; movement and release are not demonstrated or claimed.

## 2026-10-27 — Selection and camera prototype

User approved click selection, two hand slots, hover/selection outlines, orbit/pan/zoom, focus/reset and alignment views, with clear controls in the UI.

`CardStackerControls` attaches to the existing scene camera when Play starts, so the currently open scene does not need to be recreated. Left-click selects and empty-space click or Escape deselects; neither alters held physics state. Hand buttons change selection only. Selection does not recenter the camera. Right-drag orbits the selected card (or table centre), middle-drag pans, wheel zooms, F focuses and Home resets. Alignment buttons provide front/side/overhead views. The 1920x1080 HUD scales to the game window and blocks scene input beneath its panels.

Both initial cards remain held. No movement, release, snapping or artificial balancing was added. Camera clipping prevention currently uses a tabletop height floor and a minimum orbit distance; it is not a general obstacle collision system.

Verification uses an isolated copy of the project to avoid closing the user's open Unity editor. A development-only player smoke test exercises ray picking, selection without recentering, deselection, focus, camera limits, presets/reset and unchanged held-card transforms, then captures the real player UI.

Validation passed in a Unity 6000.3.8f1 Windows development build. The real player completed all smoke assertions and produced a 1920x1080 screenshot with the gold selected-card outline, hand buttons and readable control guide. Headless player capture was black, so visual inspection used a graphics-enabled player run. This validates the current selection/camera behavior, not future card placement or balancing.

## 2026-10-27 — Project setup

The user proposed a 3D house-of-cards game, centred on minute position/angle control and a maximum of two held cards. One hand holds the first card while the other positions the second; releasing allows physical balance or collapse.

Unity was selected for the physics prototype. Git/GitHub will preserve source and let viewers inspect the implementation. Stranded Harvester is parked separately for a later dev log. No dev-log episode number is assigned to this project yet.

Repository: https://github.com/blaken117/hatdon-card-stacker (public).

The user completed Unity sign-in/licence setup. Unity 6000.3.8f1 then compiled the editor code and generated `Assets/_Project/Scenes/CardStacker.unity` successfully. The setup validator confirmed exactly two held rigid-body cards with colliders and positive masses, plus the table and main camera. The scene, contact material, visual materials and generated project settings are preserved in Git. This is a validated starting scene, not a playable card-placement prototype.

A second graphics-enabled editor run rendered the actual scene at 1920x1080 for visual inspection and exited successfully. This confirms the camera, table and both card materials render. The initial shader uses the built-in render pipeline; no external art assets are required.

### First gameplay milestone

1. Two virtual hand slots, active-hand switching and independent release.
2. Constrained translation and rotation handles with fine adjustment.
3. Contact-aware held movement that prevents overlap-induced launch on release.
4. A repeatable two-card tent that stands when placed correctly and falls when unsupported.
5. More cards, camera controls and restart after that interaction works.

### Physics validation

Test flat resting, a single unsupported upright card falling, symmetric leaning pairs, sliding, edge contacts, release without artificial impulse and disturbance of an existing structure. Tune friction/contact/solver settings against these scenarios. Continuous collision detection may help fast movement but does not guarantee resting-stack stability.

The setup uses visually exaggerated thickness (0.0008m) and light rigid cards (0.0018kg). These are initial values, not validated material measurements. The project uses a 0.01s physics step and additional solver iterations as an initial baseline. No final balance tuning has been performed.

### Publication

Only this project belongs in its public repository. No account secrets, old game code, private business files or third-party paid assets. Choose a reuse licence before describing it as open source. Video production and publishing are separate steps.
