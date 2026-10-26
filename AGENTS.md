# Card Stacker development

- This is a separate Unity project. Do not copy legacy Hatdon games, credentials or paid assets into it.
- Use Unity 6000.3.8f1. Keep Assets .meta files in Git and use visible meta files / Force Text serialization.
- Keep the two-hand hold/release interaction central. Do not silently snap, glue or auto-balance released cards.
- Held cards must eventually respect contact instead of teleporting through geometry.
- Start with rigid cards; physics tuning is experimental, not a claim of exact real-world card simulation.
- Verify editor compilation and the relevant scene before describing a prototype as working. Record blocked checks honestly.
- Preserve milestones with commits and annotated tags after review. Do not commit Library, user settings, credentials or builds.
- Update CHANGELOG.md and Docs/DEVELOPMENT.md when behavior changes. No social publishing is authorized by a code change.
