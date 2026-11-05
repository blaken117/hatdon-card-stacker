# Card Stacker development

- This is a separate Unity project. Do not copy legacy Hatdon games, credentials or paid assets into it.
- Use Unity 6000.3.8f1. Keep Assets .meta files in Git and use visible meta files / Force Text serialization.
- Keep the two-hand hold/release interaction central. Do not silently snap, glue or auto-balance released cards.
- Held cards must eventually respect contact instead of teleporting through geometry.
- Start with rigid cards; physics tuning is experimental, not a claim of exact real-world card simulation.
- Verify editor compilation and the relevant scene before describing a prototype as working. Record blocked checks honestly.
- Preserve milestones with commits and annotated tags after review. Do not commit Library, user settings, credentials or builds.
- Update CHANGELOG.md and Docs/DEVELOPMENT.md when behavior changes. No social publishing is authorized by a code change.
- The user records future gameplay footage. Do not invoke capture helpers, make new gameplay MP4s or render feature takes automatically unless explicitly asked again. Keep source/build milestones and test checks; still screenshots are allowed for verification.
- For every feature that needs footage, append a unique versioned shot to `Assets/_Project/Resources/DevlogShots.json`, update `Docs/RECORDING-CHECKLIST.md`, and add an appropriate repeatable Esc-admin preset/tool. Do not silently mark a shot recorded.
- Before final video editing, review the catalogue, checklist and user's local `devlog-recording-progress.json` under Application.persistentDataPath. Remind the user of missing takes and locate/verify the actual clips; a recorded checkbox alone does not prove a file exists. Earlier automatic prototype captures remain archived but do not satisfy new pending user takes by default.
- Use `-releaseBuild` for a clean interactive build without the legacy HATDON_CAPTURE recording helpers.
