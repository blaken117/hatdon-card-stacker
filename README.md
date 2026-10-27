# Hatdon Card Stacker

A Unity experiment about building structures from playing cards using two virtual hands and precise positioning.

## Status

Card movement and rotation prototype. Select either held card and use world-axis handles to position and rotate it, with fine adjustment and contact limits. Release and physical balancing are the next milestone; this is not a finished game.

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
| W / Move button | Show X/Y/Z movement handles; drag an arrow |
| E / Rotate button | Show X/Y/Z rotation rings; drag a ring |
| Shift while dragging | One-tenth movement or rotation sensitivity |
| Right-drag | Orbit selected card, or table centre |
| Middle-drag | Pan |
| Mouse wheel | Zoom within near/far limits |
| F | Focus selected card, or table centre |
| Home | Restore initial camera |
| Front / Side / Overhead buttons | Alignment views |

Selecting a card does not move the camera. The camera stays above the tabletop. Both hand slots currently start occupied. Adjusting one card keeps the other held in place. Position is shown in world millimetres and rotation in world Euler degrees. Red is X, green is Y (height), blue is Z. Choose a different camera view if an axis is viewed end-on.

Moves and rotations stop when the card meets the table or other card. Contact is checked in small swept steps with a refined stopping point; cards do not push each other while both are held. This is a kinematic placement constraint, not a physical balance simulation. Picking up additional cards and releasing the held cards are not implemented yet.

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
