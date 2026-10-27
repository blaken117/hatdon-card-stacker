# Changelog

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
