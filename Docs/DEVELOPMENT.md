# Development record

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
