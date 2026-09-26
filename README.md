# DiceBaby

**A 2-player online dice duel made in Unity 6 as a team project.** Two players face each other across a desk in a horror-themed hospital room and roll two dice each round. You only see your own total until the reveal. The lower total costs a heart, and the first player to lose all three hearts loses the match.

---

## Gameplay

- **Two players, one table:** players create or join an online room from the menu and spawn at two chairs on opposite sides of a desk, facing each other. Walking is switched off, so each player can only look around, up to 80° to either side.
- **A round:**
  - A 3-second countdown starts the round.
  - Each player rolls two dice (total 2–12) and may roll a second time, which replaces the first total. After the first roll you can **Keep** instead. The second roll is kept automatically.
  - You see your own total. The opponent's shows "?" until the round result reveals both.
  - The higher total wins, and the other player loses one of their 3 hearts. A tie costs nothing.
- **Match end:** when a player has no hearts left, both players see "YOU WIN!" or "YOU LOSE!".
- **Feedback:** a 3-second roll animation that slows down before it lands, plus sounds for a round win, loss or tie and for the match result.
- **Setting:** a hospital room built from a horror-themed asset pack (walls, doorways, doors, stretchers, trolleys, electrical panels, cables, a lamp). It has baked lightmaps and a post-processing volume (grain, vignette, chromatic aberration, lens distortion, depth of field, ambient occlusion and more).

**Status:** early prototype. The commit history covers four days (Jan 24–27, 2026). The core loop is implemented: menu → create or join a room → both players placed in `MainMap` → countdown → dice rounds → win / lose panel. Not done yet:

- There is no rematch after game over.
- The dice are numbers in the UI, not 3D dice.
- The level is still a blockout. The floor is made of plane primitives with the pack's floor materials, and the desk and chairs are placeholder cubes. A second hospital pack was explored by a teammate in a separate fork and is not placed in this `MainMap`.

## Tech stack

| Area | What it uses |
|---|---|
| Engine | **Unity 6** (6000.0.32f1), **Built-in Render Pipeline** |
| Networking | **Photon Fusion 2** (SDK 2.0.9) in *Shared* mode, through the **Clean Multiplayer Pro** 2.0.1 template (session list with search, create / join, room passwords, region selection) |
| Voice | **Photon Voice 2** (2.62), set up by the template |
| Graphics | **Post Processing Stack v2** (3.5.1), baked lightmaps |
| Input | Unity **Input System** 1.11 |
| Camera | **Cinemachine** 2.9 (the template's player follow camera) |
| UI | uGUI + **TextMesh Pro** |
| Dev tooling | **ParrelSync** (a clone editor for two-player testing on one machine), **Git LFS** |

> Timeline, Visual Scripting and Multiplayer Center are in the package manifest, but the game scripts don't use them.

## What's in the project

Most of the project is the multiplayer template, the Photon SDKs and a hospital art pack. The team-authored part splits cleanly between level work and game systems:

| System | Key files | Author (from the commits) |
|---|---|---|
| Hospital room blockout (floors, walls, doors, stretchers, trolleys, cables, lamp, panels) | `Assets/Scenes/MainMap.unity`, `Assets/Horror Hospital Pack/` | Me (as PapiChullo) |
| Baked lighting and post-processing volume | `Assets/Scenes/MainMap/`, `Assets/Scenes/MainMap_Profiles/PostProcessing Profile.asset` | Me (as PapiChullo) |
| Doorway placement and iteration | `Assets/Scenes/MainMap.unity` (four `DoorWay` instances) | Me (as PapiChullo) |
| Networked dice-duel rules (phases, lives, hidden results) | `Assets/Scripts/DiceGame/DiceGameManager.cs`, `Assets/Prefabs/DiceGameManager.prefab` | Teammate (as Kanteot / omeravcioglu) |
| Dice UI (countdown, Roll / Keep buttons, hearts, result and game-over panels, sounds) | `Assets/Scripts/DiceGame/DiceGameUI.cs`, `Assets/Unity Dice/` (the five sound files) | Teammate (as Kanteot) |
| Roll animation | `Assets/Scripts/DiceGame/DiceRollerAnimation.cs` | Teammate (as Kanteot) |
| Template changes (chairs facing each other, look-only camera, dice-manager spawn, "DICE BABY" menu) | `FusionConnection.cs` and `ThirdPersonController.cs` in `Assets/Clean Multiplayer Pro/`, `Assets/Scenes/Menu.unity` | Teammate (as Kanteot) |

`Assets/Scripts/` holds 3 game scripts written by my teammate (~930 lines). My commits focus on the environment: importing and dressing `MainMap`, baking lightmaps, configuring post-processing, and iterating doorways.

### Environment highlights

- **`Assets/Scenes/MainMap.unity`:** the session game level. Hospital Horror Pack prefabs form the room shell and props. Floor surfaces use plane primitives with the pack's floor materials. Desk and chairs are still placeholder cubes placed by the gameplay teammate for seating targets.
- **Baked lighting (`Assets/Scenes/MainMap/`):** three lightmap pairs plus `LightingData.asset`, driven by a directional light, two spot lights and a point light.
- **Post-processing (`Assets/Scenes/MainMap_Profiles/PostProcessing Profile.asset`):** grain, vignette, chromatic aberration, lens distortion, depth of field and ambient occlusion for a horror look.
- **Doorways:** four doorway instances were added and repositioned across two commits after the base room was lit.

## My contributions

Git records me as **PapiChullo** (`melihstudios@gmail.com`). My teammate appears as **Kanteot** (now [omeravcioglu](https://github.com/omeravcioglu)). I built the level; they built the multiplayer dice game.

**As PapiChullo (Jan 24–27, 2026):**

- **Initial Unity 6 project and hospital room:** imported the Hospital Horror Pack and blocked out `MainMap` with floors, walls, doors, stretchers, trolleys, cables, a lamp and other props (`a17a28f`).
- **Lighting and post-processing:** baked lightmaps and a post-processing volume for `MainMap` (`72a5c39`).
- **Doorway iteration:** added and updated four doorway placements (`f63b1f5`, `85e87d7`).
- **Minor:** TextMesh Pro fallback font asset update (`3292c8f`).

I did **not** author the Fusion dice phase machine, dice UI, roll animation, Clean Multiplayer Pro / Photon wiring, look-only camera changes, or win / lose SFX.

## Team and timeline

Two of us worked on the project: me (PapiChullo) and Kanteot / omeravcioglu. `main` has 9 commits: 5 by me and 4 by my teammate.

| Date (2026) | Author | Work |
|---|---|---|
| Jan 24 | Me (PapiChullo) | *Initial commit*: Unity 6 project with the Hospital Horror Pack, first `MainMap` room and `TestMap` |
| Jan 24 | Teammate (Kanteot) | 4 commits: Clean Multiplayer Pro, Photon Fusion 2, Photon Voice 2, ParrelSync, Cinemachine, Post Processing package; dice-game scripts, prefab and UI; placeholder desk and chairs; "DICE BABY" menu; Git LFS; win / lose / tie sounds |
| Jan 25 | Me (PapiChullo) | TextMesh Pro fallback font asset |
| Jan 26 | Me (PapiChullo) | Baked lighting, lights and post-processing for `MainMap`; doorways added |
| Jan 27 | Me (PapiChullo) | Doorway updates |

My teammate's public showcase of the same project (code-focused, with their contribution framing) is at [omeravcioglu/DiceBaby](https://github.com/omeravcioglu/DiceBaby).

## Scenes

| # | Scene (Build Settings order) | Purpose |
|---|---|---|
| 0 | `Assets/Clean Multiplayer Pro/Scenes/Menu.unity` | Template menu. It has no `DiceGameManager` prefab assigned. |
| 1 | `Assets/Clean Multiplayer Pro/Scenes/Game.unity` | Template demo game scene |
| 2 | `Assets/Scenes/MainMap.unity` | **The game level:** hospital room, desk and chairs, dice UI. Sessions load it by name. |
| 3–4 | `Assets/Clean Multiplayer Pro/Scenes/Environment 1.unity`, `Environment 2.unity` | Template demo environments |

These scenes are not in Build Settings:

- `Assets/Scenes/Menu.unity`: the team's copy of the template menu. It is retitled "DICE BABY", and its `FusionConnection` has the chair positions and the `DiceGameManager` prefab set. **Start here.**
- `Assets/Scenes/Game.unity`, `Environment 1.unity`, `Environment 2.unity`: byte-identical copies of the template scenes.
- `Assets/TestMap.unity`: a test scene from the first commit (a plane, two cubes and a cylinder).

## Integrated third-party assets

| Asset | Used for |
|---|---|
| **Clean Multiplayer Pro** 2.0.1 (Avocado Shark) | The multiplayer template: menu, Fusion runner and voice setup, player character and camera. Adapted by my teammate for the dice game. |
| **Photon Fusion 2** (2.0.9) with the Fusion Physics add-on | Networking |
| **Photon Voice 2** (2.62) | Voice chat |
| **Hospital Horror Pack** 1.0 (`Assets/Horror Hospital Pack/`) | All walls, doorways, doors and props in `MainMap`, plus its floor materials. The pack also installed Unity's legacy **Standard Assets** and the `Assets/Editor/ImageEffects/` editors, which `MainMap` does not use. |
| **TextMesh Pro** | All UI text, including the dice numbers and the hearts display |
| **ParrelSync** | A clone editor for two-player testing |

Despite its name, `Assets/Unity Dice/` only holds the five sound effects used by the dice UI.

## Setup notes

1. Open the project in **Unity 6000.0.32f1**.
2. Open `Assets/Scenes/Menu.unity` (not the Clean Multiplayer Pro template menu in Build Settings index 0).
3. Create a Photon Fusion / Voice App ID in the [Photon dashboard](https://dashboard.photonengine.com/) and paste it into `Assets/Photon/Fusion/Resources/PhotonAppSettings.asset`. Committed App IDs were cleared from this repository so the public tree does not expose live credentials.
4. Enter Play mode with ParrelSync (or two editors) to create / join a Shared-mode session and load `MainMap`.

## About this repository

This public repository contains the **team project tree** used during development, including third-party templates and the Hospital Horror Pack. My attributable work is the **hospital room level, baked lighting, post-processing and doorway layout** on `MainMap`. Gameplay networking and dice UI were written by my teammate.

Third-party Asset Store packages remain subject to their own licenses. Do not redistribute this repository as your own commercial product without checking those terms.
