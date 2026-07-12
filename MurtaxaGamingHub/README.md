# 🎮 Murtaxa Gaming Hub

A complete **third-person open-world action adventure** game built in Unity for Android.  
Forests, mountains, villages, rivers, caves, hidden treasure — and enemies that hunt you.

---

## 📋 Table of Contents

1. [Requirements](#requirements)
2. [Project Structure](#project-structure)
3. [Setup Instructions (Step by Step)](#setup-instructions)
4. [Scene Setup Guide](#scene-setup-guide)
5. [Script Reference](#script-reference)
6. [Android APK Build Guide](#android-apk-build-guide)
7. [Performance & Optimization Tips](#performance--optimization-tips)
8. [Audio Asset Checklist](#audio-asset-checklist)
9. [Free Asset Sources](#free-asset-sources)

---

## Requirements

| Tool | Version |
|------|---------|
| Unity Editor | **2022.3 LTS** (recommended) or **2023.x** |
| Android Build Support | Module installed via Unity Hub |
| Android SDK | API Level 33 (auto-installed by Unity Hub) |
| Android NDK | r23b (bundled with Unity Hub) |
| Java JDK | 11 (bundled with Unity Hub) |
| TextMeshPro | 3.0.9 (auto-imported via Package Manager) |
| Universal Render Pipeline | 14.0.9 (see setup step 4) |
| AI Navigation | 1.1.5 (for NavMesh) |

> **Free Unity version is sufficient.** Unity Personal or Unity Student can build APKs without watermarks if revenue < $100k/year.

---

## Project Structure

```
MurtaxaGamingHub/
├── Assets/
│   ├── Scripts/
│   │   ├── Audio/
│   │   │   ├── AudioManager.cs          ← Central SFX/music hub
│   │   │   ├── FootstepAudio.cs         ← Surface-based footsteps
│   │   │   └── MusicManager.cs          ← Zone-based music switching
│   │   ├── Enemy/
│   │   │   ├── EnemyAI.cs               ← Patrol/Chase/Attack FSM
│   │   │   └── EnemyStats.cs            ← Enemy health, drops, death
│   │   ├── NPC/
│   │   │   ├── NPCInteraction.cs        ← Proximity detect + [E] interact
│   │   │   └── NPCDialogue.cs           ← Dialogue tree data driver
│   │   ├── Player/
│   │   │   ├── PlayerController.cs      ← Walk, run, jump, crouch
│   │   │   ├── PlayerCombat.cs          ← 3-hit combo melee system
│   │   │   ├── PlayerStats.cs           ← HP, stamina, XP, leveling
│   │   │   └── ThirdPersonCamera.cs     ← Orbit cam with collision
│   │   ├── Systems/
│   │   │   ├── GameManager.cs           ← Game state machine
│   │   │   ├── ItemPickup.cs            ← World item collector
│   │   │   ├── Checkpoint/
│   │   │   │   └── CheckpointSystem.cs  ← Checkpoint + respawn
│   │   │   ├── Inventory/
│   │   │   │   ├── InventoryItem.cs     ← ScriptableObject item def
│   │   │   │   └── InventorySystem.cs   ← Add/remove/use items
│   │   │   ├── Quest/
│   │   │   │   ├── QuestData.cs         ← ScriptableObject quest def
│   │   │   │   └── QuestSystem.cs       ← Active quest tracking
│   │   │   └── Save/
│   │   │       ├── GameData.cs          ← Plain serializable save object
│   │   │       └── SaveSystem.cs        ← JSON save/load to disk
│   │   ├── UI/
│   │   │   ├── UIManager.cs             ← Panel switcher (central)
│   │   │   ├── HUDController.cs         ← HP bar, stamina, toasts
│   │   │   ├── MainMenuUI.cs            ← New Game / Continue / Quit
│   │   │   ├── PauseMenuUI.cs           ← Resume / Save / Settings
│   │   │   ├── InventoryUI.cs           ← Inventory grid + tooltip
│   │   │   ├── QuestLogUI.cs            ← Active quest + objectives
│   │   │   ├── MapUI.cs                 ← World map with icons
│   │   │   ├── DialogueUI.cs            ← NPC dialogue + choices
│   │   │   ├── SettingsUI.cs            ← Volume, quality, touch
│   │   │   └── GameOverUI.cs            ← Death screen
│   │   ├── World/
│   │   │   ├── TreasureChest.cs         ← Interactable loot chest
│   │   │   └── WorldZone.cs             ← Trigger-based zone events
│   │   └── Utils/
│   │       ├── Singleton.cs             ← Generic persistent singleton
│   │       ├── ObjectPool.cs            ← Reusable object pool
│   │       └── GameEvents.cs            ← Decoupled event bus
│   ├── Plugins/Android/
│   │   └── AndroidManifest.xml          ← Custom Android manifest
│   └── [Art, Audio, Prefabs — see below]
├── Packages/
│   └── manifest.json                    ← All required packages
└── ProjectSettings/
    └── ProjectSettings.asset            ← Company, bundle ID, Android API
```

---

## Setup Instructions

### Step 1 — Install Unity Hub & Unity Editor

1. Download **Unity Hub** from [unity.com/download](https://unity.com/download)
2. In Unity Hub → **Installs** → **Install Editor** → choose **Unity 2022.3 LTS**
3. In the install wizard, check these **modules**:
   - ✅ **Android Build Support**
   - ✅ **Android SDK & NDK Tools**
   - ✅ **OpenJDK**

### Step 2 — Open the Project

1. In Unity Hub → **Projects** → **Add** → navigate to `MurtaxaGamingHub/` folder
2. Click the project to open it — Unity will import all assets (first import takes 2–5 min)

### Step 3 — Import Required Packages

Unity will auto-install packages from `Packages/manifest.json`.  
If any fail, install manually via **Window → Package Manager**:

| Package | How to Install |
|---------|---------------|
| **TextMeshPro** | Package Manager → Unity Registry → TextMeshPro → Install + Import TMP Essentials |
| **AI Navigation** | Package Manager → Unity Registry → AI Navigation |
| **Universal Render Pipeline** | Package Manager → Unity Registry → Universal RP |
| **Cinemachine** | Package Manager → Unity Registry → Cinemachine |

### Step 4 — Configure Universal Render Pipeline (URP)

1. **Edit → Project Settings → Graphics**
2. Set **Scriptable Render Pipeline Settings** to your URP asset  
   (Create one via **Assets → Create → Rendering → URP Asset (with Universal Renderer)**)
3. **Edit → Project Settings → Quality** → set each quality level to use the URP asset
4. **Edit → Render Pipeline → Upgrade Project Materials to URP**

### Step 5 — Set Up Scenes

Create two scenes (you will build them in Unity Editor):

| Scene Name | Purpose |
|-----------|---------|
| `MainMenu` | Title screen (add `MainMenuUI`, `AudioManager`, `UIManager`) |
| `GameWorld` | The open world (add Player, terrain, enemies, NPCs, etc.) |

In **File → Build Settings → Scenes in Build**, add in order:
1. `Assets/Scenes/MainMenu.unity` (index 0)
2. `Assets/Scenes/GameWorld.unity` (index 1)

### Step 6 — Set Up Android Build Target

1. **File → Build Settings**
2. Select **Android** → click **Switch Platform** (takes ~1 minute)
3. **Player Settings** (button at bottom):
   - **Company Name**: `MurtaxaGaming`
   - **Product Name**: `Murtaxa Gaming Hub`
   - **Package Name**: `com.murtaxagaming.hub`
   - **Minimum API Level**: `Android 6.0 (API 23)`
   - **Target API Level**: `Android 13 (API 33)`
   - **Scripting Backend**: `IL2CPP`
   - **Target Architectures**: ✅ `ARM64` (✅ `ARMv7` optional for older devices)

---

## Scene Setup Guide

### MainMenu Scene

Create a new scene. Add these GameObjects:

```
MainMenu Scene
├── [Canvas] MainMenuCanvas
│   ├── [Panel] MainMenuPanel          ← Attach MainMenuUI.cs
│   │   ├── [TextMeshPro] TitleText    
│   │   ├── [Button] NewGameButton
│   │   ├── [Button] ContinueButton
│   │   ├── [Button] SettingsButton
│   │   └── [Button] QuitButton
│   ├── [Panel] SettingsPanel          ← Attach SettingsUI.cs (inactive by default)
│   └── [Panel] LoadingPanel           (inactive by default)
│
├── [EmptyGO] Managers
│   ├── Attach: GameManager.cs
│   ├── Attach: AudioManager.cs        ← 4 AudioSource children
│   ├── Attach: UIManager.cs           ← Wire all panels
│   └── Attach: SaveSystem.cs          ← Wire allItemDefinitions
│
└── [Main Camera]
```

### GameWorld Scene

```
GameWorld Scene
│
├── [Terrain] WorldTerrain             ← Unity Terrain component, paint textures
│
├── [EmptyGO] Player                   ← Tag: "Player"
│   ├── CharacterController
│   ├── Animator (with Animator Controller)
│   ├── Attach: PlayerController.cs    ← Drag camera transform
│   ├── Attach: PlayerCombat.cs        ← Set enemyLayers mask
│   ├── Attach: PlayerStats.cs
│   └── Attach: FootstepAudio.cs       ← Wire surface clips
│
├── [EmptyGO] CameraRig                ← Parent of Main Camera
│   ├── Attach: ThirdPersonCamera.cs   ← Set target = Player
│   └── [Main Camera]
│
├── [EmptyGO] Managers
│   ├── Attach: GameManager.cs
│   ├── Attach: AudioManager.cs
│   ├── Attach: UIManager.cs
│   ├── Attach: InventorySystem.cs
│   ├── Attach: QuestSystem.cs         ← Wire allQuests array
│   ├── Attach: SaveSystem.cs
│   ├── Attach: CheckpointManager.cs
│   ├── Attach: ObjectPool.cs
│   └── Attach: MusicManager.cs
│
├── [Canvas] HUDCanvas                 (Screen Space Overlay, sort order 0)
│   ├── [Panel] HUDPanel               ← Attach: HUDController.cs
│   ├── [Panel] InventoryPanel         ← Attach: InventoryUI.cs (inactive)
│   ├── [Panel] QuestLogPanel          ← Attach: QuestLogUI.cs (inactive)
│   ├── [Panel] MapPanel               ← Attach: MapUI.cs (inactive)
│   ├── [Panel] PausePanel             ← Attach: PauseMenuUI.cs (inactive)
│   ├── [Panel] GameOverPanel          ← Attach: GameOverUI.cs (inactive)
│   ├── [Panel] SettingsPanel          ← Attach: SettingsUI.cs (inactive)
│   └── [Panel] DialoguePanel          ← Attach: DialogueUI.cs (inactive)
│
├── [NavMesh Surface]                  ← Add NavMeshSurface component, Bake
│
├── [Prefab] Enemy_Wolf × N            ← EnemyAI + EnemyStats + NavMeshAgent + Animator
├── [Prefab] NPC_Villager × N          ← NPCInteraction + NPCDialogue
├── [Prefab] Checkpoint_Campfire × N   ← Checkpoint + Box Collider (IsTrigger)
│
├── [World Zones] (empty GOs with Box Colliders, IsTrigger)
│   ├── Zone_Forest                    ← Attach: WorldZone.cs, zoneName="Forest"
│   ├── Zone_Mountain                  ← zoneName="Mountains"
│   ├── Zone_Village                   ← zoneName="Village"
│   └── Zone_Cave                      ← zoneName="Cave"
│
└── [Treasure Chests]                  ← TreasureChest.cs, unique chestId each
```

### Enemy Animator Controller

Create an **Animator Controller** for enemies with these states & parameters:

| Parameter | Type |
|-----------|------|
| `Speed` | Float |
| `Attack` | Trigger |
| `Alert` | Trigger |
| `Die` | Trigger |

States: `Idle → Walk (Speed>0.1) → Run (Speed>0.5) → Attack (trigger) → Die (trigger)`

### Player Animator Controller

| Parameter | Type |
|-----------|------|
| `Speed` | Float |
| `IsGrounded` | Bool |
| `Jump` | Trigger |
| `IsCrouching` | Bool |
| `Attack1` | Trigger |
| `Attack2` | Trigger |
| `Attack3` | Trigger |
| `Die` | Trigger |

---

## Script Reference

### Key Inspector Fields to Wire Up

| Script | Required Assignments |
|--------|---------------------|
| `PlayerController` | `cameraTransform` = Main Camera transform |
| `PlayerCombat` | `enemyLayers` = Enemy layer mask |
| `ThirdPersonCamera` | `target` = Player transform |
| `EnemyAI` | `patrolPoints[]`, `playerLayer`, `obstacleLayer` |
| `AudioManager` | 4 AudioSource children + Sound Library populated |
| `SaveSystem` | `allItemDefinitions[]` = all InventoryItem SOs |
| `QuestSystem` | `allQuests[]` = all QuestData SOs |
| `UIManager` | All panel references |
| `HUDController` | Slider/TMP references |
| `ObjectPool` | `pools[]` with tags, prefabs, sizes |

### Creating ScriptableObjects

**Items** (right-click in Project window):
```
Assets → Create → MurtaxaGaming → Item
```
Fill: `itemId` (unique!), `displayName`, `itemType`, stats.

**Quests**:
```
Assets → Create → MurtaxaGaming → Quest
```
Fill: `questId`, `title`, `objectives[]`, `reward`.

---

## Android APK Build Guide

### Option A — Debug APK (no signing needed, for testing)

1. Open **File → Build Settings**
2. Confirm **Android** is selected
3. Click **Build** → choose output folder → Unity exports `MurtaxaGamingHub.apk`
4. Enable **USB Debugging** on your Android device
5. Transfer APK via cable and install (you may need "Install from unknown sources" enabled)

### Option B — Release APK (for Play Store / sharing)

#### 1. Create a Keystore

```
Window → Asset Management → Unity Distribution Portal
```
OR in Player Settings → **Publishing Settings** → **Keystore Manager**:
- Click **Create New** → fill Alias, Password, fill details → **Add Key**
- Unity stores `murtaxa.keystore` in your project root — **back this up!**

#### 2. Configure Signing

In **Player Settings → Publishing Settings**:
- **Custom Keystore**: ✅
- **Keystore Path**: path to your `.keystore`
- **Keystore Password**: your password
- **Key Alias**: your alias
- **Key Password**: your key password

#### 3. IL2CPP Build (recommended for release)

- **Player Settings → Other Settings → Scripting Backend**: `IL2CPP`
- **Target Architectures**: ✅ ARM64, ✅ ARMv7
- This reduces APK size and improves runtime performance

#### 4. Build

**File → Build Settings → Build** → save APK.

#### 5. Optional — AAB for Play Store

For Google Play Store submissions, switch to **Build App Bundle (AAB)**:
- **File → Build Settings → ✅ Build App Bundle (Google Play)**
- This generates a `.aab` file — upload this to Play Console

---

## Performance & Optimization Tips

These settings are critical for smooth 60 FPS on mid-range Android devices:

### Graphics
| Setting | Recommendation |
|---------|---------------|
| Render Pipeline | URP (already configured) |
| Shadows | Medium — Max Distance 40m |
| Texture Quality | Half Resolution for Low quality |
| Draw Distance | Per-camera via culling masks |
| Occlusion Culling | ✅ Bake in Window → Occlusion Culling → Bake |
| LOD Groups | Add to all world meshes (3 LOD levels) |
| Batching | ✅ Static Batching + GPU Instancing on materials |
| Post Processing | Keep to 1-2 effects max (Bloom + Color Grading) |

### Physics
| Setting | Recommendation |
|---------|---------------|
| Fixed Timestep | `0.02` (50 Hz) — don't go lower |
| Layer Collision Matrix | Disable unused layer collisions |
| Rigidbody Sleep | Use default (0.005) |

### Code
- `Application.targetFrameRate = 60` ← set in `GameManager.Awake()` ✅ (already done)
- `QualitySettings.vSyncCount = 0` ← ✅ (already done)
- Use **ObjectPool** for all spawned objects (projectiles, particles, damage text) ✅
- Avoid `FindObjectOfType` in `Update()` — cache in `Start()` ✅
- Use **Animator hashes** instead of string-based parameter names ✅

### Memory
- Compress audio: set PCM → **ADPCM** for SFX, **Vorbis** for music
- Texture compression: ASTC for Android (set in Texture Import Settings)
- Use **Addressables** if the project grows beyond 300MB

---

## Audio Asset Checklist

The `AudioManager` expects these named entries in its **Sound Library**:

| Sound Key | Description |
|-----------|-------------|
| `PlayerHurt` | Player takes damage |
| `PlayerDeath` | Player dies |
| `Heal` | Health item used |
| `Jump` | Player jumps |
| `Crouch` | Player crouches |
| `StandUp` | Player stands up |
| `StaminaEmpty` | Stamina depleted beep |
| `LevelUp` | Level up fanfare |
| `Attack1` | First combo hit |
| `Attack2` | Second combo hit |
| `Attack3` | Third combo hit |
| `HitConnect` | Melee hit lands |
| `HitMiss` | Melee swing misses |
| `EnemyAlert` | Enemy spots player |
| `EnemyAttack` | Enemy attacks |
| `EnemyDeath` | Enemy dies |
| `ItemPickup` | Item collected |
| `ItemUse` | Item consumed |
| `ItemDrop` | Item dropped |
| `ChestOpen` | Treasure chest opens |
| `CheckpointActivate` | New checkpoint reached |
| `CheckpointRest` | Rested at checkpoint |
| `QuestAccepted` | New quest accepted |
| `QuestComplete` | Quest finished |
| `NPCGreet` | NPC greeting sound |
| `DialogueOpen` | Dialogue starts |
| `DialogueClose` | Dialogue ends |
| `UIClick` | Button press |
| `UIOpen` | Panel opens |
| `UIClose` | Panel closes |
| `PauseMenu` | Game paused |
| `SaveGame` | Game saved |

---

## Free Asset Sources

### 3D Models & Terrain
- **Synty Studios Polygon packs** — [syntystore.com](https://syntystore.com) *(paid, ~$20-50, highly recommended for low-poly)*
- **Unity Asset Store** → search "Low Poly Nature" → many free options
- **Sketchfab** — [sketchfab.com/features/free-3d-models](https://sketchfab.com/features/free-3d-models)
- **Kenney.nl** — [kenney.nl/assets](https://kenney.nl/assets) *(100% free low-poly packs)*

### Animations (Player & Enemy)
- **Mixamo** — [mixamo.com](https://mixamo.com) *(free Adobe service — walk, run, attack, idle, die animations, auto-rigged to your character)*
  - Download as FBX for Unity, set "In place" for locomotion animations

### Audio
- **Freesound.org** — [freesound.org](https://freesound.org)
- **Sonniss GDC Pack** — free yearly game audio bundle
- **OpenGameArt.org** — [opengameart.org](https://opengameart.org)
- **Zapsplat** — [zapsplat.com](https://zapsplat.com)

### Fonts (for TextMeshPro)
- **Google Fonts** — [fonts.google.com](https://fonts.google.com) — free, OFL licensed
- Recommended: **Cinzel** (medieval title), **Lato** (clean UI text)

---

## Known Limitations & Next Steps

| Limitation | Solution |
|-----------|---------|
| No mobile on-screen joystick | Add Unity's `UI.InputField` or use **Unity's On-Screen Controls** from the Input System package |
| No multiplayer | Add **Unity Netcode for GameObjects** |
| No cutscenes | Use **Unity Timeline + Cinemachine** |
| Map is static | Render a top-down camera to a RenderTexture for a live minimap |
| No localization | Add **Unity Localization** package |
| No achievements | Add **Google Play Games Plugin for Unity** |

---

## Quick Start Summary

```
1. Open Unity 2022.3 LTS
2. Add project → MurtaxaGamingHub/
3. Wait for import
4. Package Manager → install TextMeshPro, AI Navigation, URP
5. Configure URP (Edit → Project Settings → Graphics)
6. Create MainMenu + GameWorld scenes
7. Build scene hierarchies per Scene Setup Guide above
8. Import Mixamo animations → set up Animator Controllers
9. Create Item + Quest ScriptableObjects
10. Wire all Inspector references
11. File → Build Settings → Android → Build
12. Install APK on Android device
```

---

*Built with Unity 2022.3 LTS · Universal Render Pipeline · Murtaxa Gaming Hub v1.0.0*
