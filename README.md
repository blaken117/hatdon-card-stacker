# Hatdon Card Stacker

A Unity experiment about building structures from playing cards using two virtual hands and precise positioning.

## Status

Project setup and physics test scene. Interactive card controls are the next prototype; this is not a finished game.

## Open the project

1. Install **Unity 6000.3.8f1** through Unity Hub and activate an eligible licence.
2. Clone this repository and add its root folder in Unity Hub.
3. Open the project. Unity generates its local Library and editor settings.
4. Choose **Hatdon > Card Stacker > Create Setup Scene**.
5. Open `Assets/_Project/Scenes/CardStacker.unity` if it is not already open.

The setup scene contains a table, camera, light and two held test cards. Pressing Play does not yet provide placement controls. No Asset Store assets or online services are required.

## Intended interaction

- Move and rotate one card precisely, including fine adjustment.
- Keep up to two cards held, one per virtual hand.
- Switch the active hand without releasing the other card.
- Release either card, or both together, and let gravity/contact determine the result.
- Orbit the camera and inspect alignment from different sides.

Start by proving a stable two-card tent, then add more cards. Rigid cards are the initial approximation; card flex is outside the first prototype.

## Version history and dev logs

Small, meaningful commits preserve each working change. Reviewed dev-log milestones receive annotated tags; setup is not a released gameplay prototype. See `Docs/DEVELOPMENT.md` and `CHANGELOG.md`.

Track `Assets` (including `.meta` files), `Packages` and `ProjectSettings`. Generated caches, builds, local settings and credentials are excluded. Git is the source of truth; Unity Version Control is not configured alongside it.

## Reuse

Public source is available for inspection. No open-source reuse licence has been selected yet; no additional reuse permissions are granted by this repository. Unity and third-party components remain under their own terms.
