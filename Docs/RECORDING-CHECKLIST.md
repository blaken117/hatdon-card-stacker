# Card Stacker recording handoff

## User preference — 6 November 2026

The user records gameplay. After updates, keep notes, add repeatable tools to the Esc admin menu and preserve the tested source/build. Do not automatically produce gameplay recordings. Before the final dev-log video, remind the user which takes are outstanding and verify the files they provide. Still images may be used for internal UI verification. Earlier automatic footage remains archived.

## Current shots to record

The in-game catalogue is `Assets/_Project/Resources/DevlogShots.json`. The checkboxes below describe the requested handoff; actual user progress is saved by the game in `devlog-recording-progress.json` under Unity's Application.persistentDataPath. Do not check these off from automated testing or older footage.

- [ ] **52 Card Pickup** (`p008-pickup`): Esc → select the shot → choose a seed → Prepare shot & close menu. The 52 cards wait above the table. Start your screen recorder, then press **Space** to release them. Record the fall plus 10–15 seconds of settling. Get one wide take; optionally record a closer or slow-motion replay.
- [ ] **Ghost Hands** (`p008-ghost-hands`): Prepare the hand preset. Record both faint full hands coming down from above differently tilted cards. Orbit slowly, then press **Enter** to release both cards and show the ghosts disappearing. The preset demonstrates visuals; it is not a balanced structure.
- [ ] **Solid Movement Icons** (`p008-solid-icons`): Prepare normal gameplay with the HUD visible. Draw a card, click all four icons, and demonstrate move/depth/rotation/twist with active-mode highlighting and a fine adjustment.
- [ ] **Admin Recording Tools** (`p009-recording-tools`, optional): Show Esc → 52 Card Pickup → prepare → close → Space. This is a tools/workflow shot only if useful to the episode.
- [ ] **Mouse Movement in 3D** (`p010-mouse-movement`). Prepare the shot in Esc, draw one card and stay in Move. Hold left click, drag across the view, scroll nearer and farther, and continue dragging at the new depth. Repeat with Shift for a precise move.

- [ ] **Hold E to Rotate** (`p011-hold-to-rotate`). In the Esc preset, draw a card in Move mode. Keep left click held while pressing E to rotate, then release E and move again. Keep the HUD visible to show the keybind and mode highlight.

- [ ] **Hold T to Tilt** (`p012-hold-to-tilt`). Use the Esc preset and keep the HUD visible. Grab a card in Move, hold T and move the mouse sideways, then release T and continue moving or scrolling during the same grab.
- [ ] **Rotate with R and Click to Reset** (`p013-rotation-and-reset`). Use the Esc preset to show Hold R rotating and Hold T tilting during a drag. Click Reset cards to clear the table. Use Prototype 011 for the earlier Hold E shot.

- [ ] **Click to Commit a Pair** (`p014-commit-pair-button`). Prepare the Esc preset and keep the HUD visible. Position two cards, lock each with L, and click Commit pair when it becomes available.

- [ ] **Automatic Two-Card Steps** (`p015-automatic-pairs`). Use Prototype 015 to record the first card waiting, the second release dropping both, and the next pair being added. Use Prototype 014 for the historical Ghost Hands and Click to Commit a Pair shots.
- [ ] **Commit Each Card, Then Release the Pair** (`p016-card-and-pair-commits`). Use the Esc preset and keep the HUD visible. Record both Commit card clicks, pause briefly before clicking Commit pair, and draw the next card after release.
- [ ] **Challenges and Ghost Layouts** (`p017-challenge-collection`). Press Shift+Esc and prepare the Bridge preview. Record guide toggling and placing the top card. Use a normal challenge to show its cards and completion after leaving and reopening the table.
- [ ] **Building a Third Floor** (`p018-tower-layouts`). Prepare the pyramid preview with Shift+Esc. Record the bottom row, flat bridges, and the last leaning pair settling on the third floor.

## In-game recording tools

- **Shift+Esc:** open the admin menu in Prototype 017 onward. Esc closes it. Opening cancels drag and camera gestures and pauses the simulation.
- **Prepare shot & close menu:** stages the selected scene without recording video. Pickup stays frozen until Space; the hand and normal-play presets are ready for direct player input.
- **Seed:** reuse a whole-number seed for the same cloud, or click New random seed. Preparing again clears the previous scene.
- **Space:** play/pause. For a prepared pickup cloud, release all 52 cards and start playback.
- **. (period):** advance one 1/60-second frame while paused, using four 240Hz physics steps. A prepared pickup is released on the first step.
- **Speed:** 0.25×, 0.5× or 1× simulation speed. The selected speed survives opening/closing the menu.
- **F10 / Hide HUD:** toggle the mode icons and selection markers for a clean take. Ghost hands stay visible when locked. Keep the HUD on when recording the icon feature. Use Prototype 011 if you need the original four-icon layout for the earlier Solid Movement Icons shot.
- **Camera:** wide drop view, close table view, save/restore one view. The saved camera view persists with recording notes.
- **Mark as recorded:** save the selected shot's status, optional clip path and UTC note timestamp. This is your checklist only; it neither starts recording nor verifies the clip.
- **Save recording notes / Copy notes file path:** save clip-path/seed edits without changing a shot's status, or copy the local state-file location for the editing handoff. On this Windows setup the default folder is `C:\Users\User\AppData\LocalLow\Hatdon\Card Stacker\`.
- **1920 × 1080 window:** set the game window to the dev-log target size. The menu also displays the current render resolution.
- **Reset cards** clears and refills the table. **F8** starts a fresh random pickup test.

The game keeps running in the background so switching to your screen recorder does not pause it unexpectedly. Prepare a pickup first so the cards cannot fall while you start the recorder. No recorder, codec, microphone or video export is started by the admin menu.

## Before the final video

1. Read this checklist and the shot catalogue, then check the user's local progress file. The runtime log / menu identifies its state location via Unity's persistentDataPath.
2. Remind the user of required missing shots. Admin-tools footage is optional.
3. Locate the real clips and confirm the correct feature/version is visible, resolution/aspect, usable duration and any required game sound. Ask for missing files only when needed.
4. Link clips to the prototype notes before editing. A checkbox or an old automatic take is not sufficient proof of a new manual take.
5. For each future feature, add a new stable/versioned shot ID, instructions and a suitable Esc preset. Preserve earlier catalogue entries so recorded history is not lost.
