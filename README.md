# Hatdon Card Stacker

A Unity experiment about building structures from playing cards using two virtual hands and precise positioning.

## Status

Selection and camera prototype. Two cards remain held while you select them and inspect the table. Card positioning, rotation and release are the next milestone; this is not a finished game.

## Open the project

1. Install **Unity 6000.3.8f1** through Unity Hub and activate an eligible licence.
2. Clone this repository and add its root folder in Unity Hub.
3. Open the project. Unity generates its local Library and editor settings.
4. Open `Assets/_Project/Scenes/CardStacker.unity`.
5. To recreate the starting scene, choose **Hatdon > Card Stacker > Create Setup Scene** (this replaces it after confirmation).

The setup scene contains a table, camera, light and two held test cards. Press **Play** to use selection, the camera and the on-screen control guide. No Asset Store assets or online services are required.

## Current controls

| Input | Action |
| --- | --- |
| Hover over a card | Subtle blue outline |
| Left-click a card | Select it; gold outline |
| Left-click empty table / Esc | Deselect; both cards remain held |
| Left / Right hand buttons | Switch selected held card |
| Right-drag | Orbit selected card, or table centre |
| Middle-drag | Pan |
| Mouse wheel | Zoom within near/far limits |
| F | Focus selected card, or table centre |
| Home | Restore initial camera |
| Front / Side / Overhead buttons | Alignment views |

Selecting a card does not move the camera. The camera stays above the tabletop. Both hand slots currently start occupied; picking up, moving and releasing cards are not implemented yet.

## Intended interaction

- Move and rotate one card precisely, including fine adjustment.
- Keep up to two cards held, one per virtual hand.
- Switch the active hand without releasing the other card.
- Release either card, or both together, and let gravity/contact determine the result.
- Orbit the camera and inspect alignment from different sides.

Start by proving a stable two-card tent, then add more cards. Rigid cards are the initial approximation; card flex is outside the first prototype.

## Version history and dev logs

Small, meaningful commits preserve each working change. Reviewed dev-log milestones receive annotated tags; setup is not a released gameplay prototype. See `Docs/DEVELOPMENT.md` and `CHANGELOG.md`.

Dev-log takes preserve the complete Windows build alongside a source archive, capture manifest and clean gameplay footage. The recording helper runs an explicitly requested automated demonstration of existing controls; it does not add card-placement gameplay.

Track `Assets` (including `.meta` files), `Packages` and `ProjectSettings`. Generated caches, builds, local settings and credentials are excluded. Git is the source of truth; Unity Version Control is not configured alongside it.

## Reuse

Public source is available for inspection. No open-source reuse licence has been selected yet; no additional reuse permissions are granted by this repository. Unity and third-party components remain under their own terms.
