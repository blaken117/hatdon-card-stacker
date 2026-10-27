# Development record

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
