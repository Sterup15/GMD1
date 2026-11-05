# AGENTS.md

Guidance for AI agents working on **A Chance at Glory**, a Unity 2D top-down bullet-hell / roguelite hybrid. Read this before changing code, prefabs, or blog posts.

## The game in one paragraph

The player (an archer) moves with WASD/arrows (touch controls for phones and tablets are planned) and auto-fires at the nearest enemy in `ShootRange`. Enemies (melee knight, ranged archer) spawn in a ring around the player at an ever-increasing rate. Kills drop gold; gold fills a level bar, and each level-up pauses the game and offers 3 upgrade cards. When the boss timer hits zero, all regular enemies despawn, the camera locks into an arena, and the boss (Togre) spawns. Killing the boss wins the run. Runs last 5–10 minutes.

Design pillars (from the GDD, `Blog posts/BlogPost2.md`): **Sensation** (dodging and firing barrages), **Challenge** (short runs that ramp fast), **Mastery** (learning enemy patterns and upgrade combos). Planned future work: more characters, upgrades (legendary effects), enemies, bosses, multiple bosses per run, persistent between-run upgrades.

## Tech and project facts

- Unity **6000.3.8f1**, URP 2D, new **Input System**, TextMeshPro, uGUI.
- No pathfinding: the map is one flat, convex area and enemies steer straight at the player. The earlier NavMeshPlus pathfinding is kept on the `navmesh-pathfinding` git branch. `Assets/NavMeshPlus-master` is still vendored (third-party, don't edit) but no game code uses it.
- Art: **Tiny Swords** asset pack by Pixel Frog (`Assets/Tiny Swords`, third-party). Music from ebunny (Pixabay).
- Target platforms: a **WebGL** build that must work on desktop browsers and on phones/tablets (iPhone, iPad, Android), plus Windows desktop. `Builds/WebGL` is committed and deployed to GitHub Pages by `.github/workflows/deploy-pages.yml` on every push to `master`.
- Build scenes: `MainMenu` (index 0), `SampleScene` (index 1, the actual game). `MainScene.unity` is not in the build.
- IDE: Rider. `*.csproj`/`*.sln` are generated and gitignored.
- There are no automated tests. Verification means running the game in the Unity editor — say so when you cannot do that rather than claiming a change works.

## Folder layout ("feature folders")

Everything belonging to one entity lives in that entity's folder: scripts, sprites, animations, audio, and prefab.

```
Assets/GameObjects/
  Common/            shared systems: Audio, Camera, DamageNumber, Events, Health (bar UI),
                     Projectiles, Settings (saved player settings), Stats, UI (buttons, main/pause/settings/character select menus,
                     victory/defeat, Pointer: mouse cursor and selection hand), Upgrades, Utils
    Effects/         shared visual effects, one folder each: DustPuff (death and dash smoke), Explosion (area damage, used by bombs), Fire (imported, not used yet)
    Pickups/         things the player collects, one folder each: Gold, Steak
    Terrain/         map decoration: Scripts (TerrainScatter, RandomAnimationStart, CanopyFade), Trees/ (Sprite/Anims, Tree1-4 and Stump1-4 prefabs), GroundPatches/ (Sprite, GroundPatch_<Colour>_<Shape> prefabs)
  Enemy/
    Common/          components shared by all enemies (EnemyHealth, EnemyRetreat, GoldDropper)
    MeleeEnemy/      enemy type folder
      Shared/        Scripts/Behaviour (EnemyMelee*) used by every melee enemy
      RedKnight/     Sprite/Anims, RedKnight.prefab
      Skull/         Sprite/Anims, Skull.prefab
    RangedEnemy/     enemy type folder
      Shared/        Scripts/Behaviour (EnemyRanged*) used by every ranged enemy
      RedArcher/     Sprite/Anims, RedArcher.prefab
    TogreBoss/       Scripts/Behaviour, Scripts/Health (BossHealthBar), Sprite/Anims, AudioFiles, prefab
    Spawner/         EnemySpawner, BossTimer
  Music/             MusicController, MainMenuMusicController, AudioFiles
  Player/
    Shared/          Scripts/{Movement,Health,Gold,Dash,Selection}, AudioFiles used by every character, CharacterRoster.asset
    Characters/      one folder per playable character
      Archer/        Sprite/Anims, ArcherSprite.prefab, AudioFiles, Archer.prefab (arrows via ProjectileSpawner), ArcherDefinition.asset
      Bombardier/    Sprite/Anims (own art: kettle helmet and tabard in the archer's palette), Scripts (BombThrower), Bomb/ (Bomb script, sprite, prefab), Bombardier.prefab, BombardierDefinition.asset
Assets/Input/        GameInput.cs + GameControls.inputactions
Assets/UI/           HUD bars (HealthBar, GoldBar, StaminaBar with dash charge pips) and UI textures
Assets/World/        tilemaps
Tools/SpriteGen/     Python pixel-art generator for character sheets (outside Assets, not imported by Unity)
```

Enemies are grouped by **type** (see rule 1): `Enemy/<Type>/Shared/` holds everything the enemies of that type share (behaviour scripts, and any shared audio or assets), and `Enemy/<Type>/<EnemyName>/` holds only what is unique to that enemy (sprites, animation clips, Animator Controller, prefab, and its own audio).
- A new enemy of an existing type goes in `Enemy/<Type>/<EnemyName>/` with `Sprite/Anims` and `<EnemyName>.prefab`, and uses the type's `Shared` scripts.
- A new enemy type gets `Enemy/<NewType>/Shared/Scripts/Behaviour` plus a folder for its first enemy.
- When something unique to one enemy becomes useful to others of the same type, move it to `Shared/`.

Namespaces mirror the folder path, e.g. `GameObjects.Enemy.RangedEnemy.Shared.Scripts.Behaviour`.

## Architecture rules

The whole game is **component-based and event-driven**. These rules come from the code and the devblogs; follow them.

### 1. Composition over inheritance, one job per component (SRP)
- Actors have no class hierarchy. They are a GameObject built from small components (`Stats`, `ActorEvents`, `EnemyHealth`, `ProjectileSpawner`, `GoldDropper`, `ActorSoundController`, ...).
- When a component starts doing a second job, pull that job out into its own component. Example: gold dropping was moved out of `EnemyHealth` into `GoldDropper`.
- **Every enemy type gets its own dedicated scripts.** An enemy *type* is a behaviour archetype (melee, ranged, boss, and later things like a grenade thrower), not a specific enemy or skin. Never add an `enemyType` enum or `if (isBoss)` branches to a shared script. Each type has its own `<Type>` (behaviour/movement), `<Type>MovementState` (state machine and animation), and `<Type>AnimationEvents` scripts. They share only the generic components in `Enemy/Common`.
- **A new enemy of an existing type reuses that type's scripts.** It only gets its own folder with sprites, an Animator Controller (same state names, e.g. Idle/Move/Attack), stats and a prefab. Example: `Skull` is a melee enemy that uses the `EnemyMelee*` scripts. Write new scripts only when the enemy needs behaviour its type doesn't have, and then it's a new type.

### 2. Two event buses: decouple, don't reference siblings
- **`ActorEvents`** (`Common/Events/ActorEvents.cs`) is a per-actor bus on each actor's root GameObject. Components on the same actor talk through it, not through direct references to each other. Adding a signal means: add `public event Action OnX;` plus a `public void X() => OnX?.Invoke();` raiser.
- **`GlobalEvents`** (`Common/Events/GlobalEvents.cs`) is a static, game-wide bus for moments the whole game cares about: boss spawned/defeated, player died/won, level up, bounce unlocked. Use it for cross-system reactions (music, UI screens, camera, victory) so nothing has to reference the spawner or the player.
- Subscribe in `OnEnable` and **always unsubscribe in `OnDisable`**. Static events survive scene reloads, so a missing unsubscribe leads to null-reference errors after a restart. Cache `ActorEvents` in `Awake` (`GetComponent`, or `GetComponentInParent` from a child sprite).
- Don't over-decouple. Plain local events are fine for one-to-one wiring (e.g. `BossTimer.OnTimerComplete` → spawner, `BossHitbox.OnHit` → `BossMelee`).

### 3. Animation drives gameplay timing
- Gameplay moments (firing a shot, finishing an attack, the end of a windup, opening and closing the hitbox, footsteps, swings, death complete) are **Animation Events** on the clips, not timers. The one exception is the dash: there's no dash clip, so `PlayerDash` times it and raises `DashStarted`/`DashEnded` on `ActorEvents`, which movement (dash velocity), `PlayerHealth` (dash i-frames) and `DashDust` listen to.
- Each actor's sprite child has an `*AnimationEvents` script whose methods only forward to `ActorEvents` (`public void OnShotFired() => _events.ShotFired();`). It never references gameplay components.
- Attack speed scales `animator.speed` from `Stats.FireRate`, so shot timing follows the animation.
- State machines use `EnemyMoveStateEnum` (Idle, Move, Windup, Attack, Recover, Die) and play clips by name with `animator.Play("Idle")` (constants in the script), not through Animator transitions. Locked states (Attack/Windup/Recover/Die) are exited only by animation events.
- Death is two-phase. `EnemyHealth` raises `DeathStarted` if anything listens (the boss plays a Die clip, then `DeathComplete` → `FinaliseDeath()`). Otherwise it finalises immediately. `FinaliseDeath` raises `Death` (gold drop, spawner counters) and destroys the actor.

### 4. Universal Stats with the Observer pattern
- Every actor has a `Stats` component: `MoveSpeed, FireRate, Damage, MaxHealth, ShootRange, PickupRange, Bounce, Penetration, Haste, DashCharges, Area` (`Area` multiplies explosion radius, base 1). Never hardcode these on behaviour scripts. Read `stats.X.Value` instead.
- `Stat` = `baseValue + _bonus`. **`SetBaseValue`** is for difficulty scaling (spawner). **`AddBonus`/`RemoveBonus`** is for upgrades. Don't mix them up.
- React to changes by subscribing to `Stat.OnValueChanged` instead of polling (e.g. `PlayerHealth` reacting to `MaxHealth`).
- Adding a stat: add the field to `Stats`, a value to `StatType`, and a case to `Stats.GetStat`.
- `Haste` shortens every cooldown without a cap: `cooldown = base / (1 + Haste / 100)`, so 100 Haste halves it. New abilities with a cooldown must use the same formula.
- `StatType` values are stored as ints in the upgrade assets, so new stats are only ever appended at the end of the enum.
- Integer-like stats (`Damage`, `MaxHealth`, `Bounce`, `Penetration`, `DashCharges`) are floats rounded with `Mathf.RoundToInt` at the point of use.

### 5. Data-driven, tunable in the Inspector (Open/Closed)
- **Upgrades** are `UpgradeDefinition` ScriptableObjects (`Create > BulletHell > Upgrade Definition`) in `Common/Upgrades/Resources/Upgrades/<Rarity>/`, loaded with `Resources.LoadAll`. A new stat upgrade is a new asset, not new code. Naming convention: `AD+1`, `AS+3`, `Haste+2`, `Bounce+2`, `Pen+1`, with the suffix matching the rarity tier (Common +1 … Legendary +4). Special stats with only a few tiers (`Bounce`, `Pen`, `Dash`) use +1/+2 on the two rarest tiers. The `Dash+1`/`Dash+2` charge cards are one-time picks (the HUD has 4 charge pips) and are meant to become an item once the Items system exists.
- Upgrade cards are shared by every character and only say how many **points** they give (`points`, +1 to +4), using in-game stat names instead of numbers: Strength (`Damage`), Agility (`FireRate`), Vitality (`MaxHealth`), Swiftness (`MoveSpeed`), Sight (`ShootRange`), Greed (`PickupRange`), Haste, Blast (`Area`). Never put percentages or raw values on a card. What one point is worth lives in one table, `UpgradeScreen.pointValues` (`StatPointValue`: flat, or `percentOfBase` = a percentage of the character's own `Stat.BaseValue`, so a slow-firing character doesn't outgrow a fast one). The values keep the archer's old numbers (Agility 40%, Swiftness 13.33%, Sight 8%, Greed 40%, Blast 10% of base; Strength/Vitality 1 and Haste 10 flat). Stats missing from the table (bounce, penetration, dash charges) get 1 per point. Never change a character's base stats with `AddBonus`, because percentage points are computed from `BaseValue`.
- Cards that only make sense for some attacks have a `requirement` (`UpgradeRequirementType`: None, Projectiles, Explosions). Each character prefab has an `UpgradeEligibility` component with one bool per requirement (archer: projectiles, Bombardier: explosions), and `UpgradeScreen` filters the pool by it on every level-up. Something that adds a new kind of attack mid-run (e.g. the Shoulder Ballista or Backpack Catapult items) calls `UpgradeEligibility.Set(requirement, true)`. A new requirement is a new enum value plus a bool on `UpgradeEligibility`, not character checks in the upgrade code.
- **Difficulty** uses `AnimationCurve`s (X = run time in seconds or player level, Y = multiplier or value): spawn rate, enemy health/speed/damage, gold drop, minimum enemies alive, gold threshold. Provide a default curve in `Awake` and `OnValidate` (see `CurveUtils.EaseIn`) for when the serialized curve is empty.
- Global balance knobs live on one component (e.g. `UpgradeScreen.pointValues`), not spread across many assets.
- Tunables are `[SerializeField] private` fields with `[Header]` and `[Tooltip]` where useful.

### 6. Abstractions where it pays off
- `IDamageable` (`TakeDamage(int)`) is the only thing projectiles know about. New damageable things implement it, and `Projectile` stays unchanged.
- `IPickup` (`Collect(GameObject collector)`, `Common/Pickups/Scripts`) is the only thing `PlayerPickup` knows about. A new pickup (gold, steak, magnet, ...) implements it in its own `Common/Pickups/<Name>/` folder. It decides for itself whether it is consumed (e.g. the steak stays on the ground at full health).
- Enemies move straight at the player: each behaviour script sets `rb.linearVelocity` from the normalised direction to the player. This only works because the map is convex with nothing inside it to walk around. A map with obstacles would need pathfinding back (see the `navmesh-pathfinding` branch).
- `ProjectileSpawner` is shared by the archer (`TargetModeEnum.NearestEnemy`) and enemies (`TargetModeEnum.Player`), fires on its actor's `ShotFired` event and pushes `Damage/Bounce/Penetration` from `Stats` into the projectile at fire time.
- **Characters** share everything in `Player/Shared` and differ only in their sprite child (Animator Controller with the same Idle/Move/Attack states and a `ShotFired` event on Attack), `Stats` values and **attack component**, which listens to `ShotFired`: `ProjectileSpawner` for the archer, `BombThrower` for the Bombardier. A new character is a copy of an existing character folder with its own art and attack component, plus a `<Name>Definition` asset (`CharacterDefinition`: display name, description, portrait, prefab) added to `Player/Shared/CharacterRoster.asset`. The select screen builds its portraits from the roster and reads the base stats from the prefab's `Stats`, so it needs no UI or code changes. Movement states never fire attacks themselves.
- The game scene has no placed player. `PlayerSpawner` (execution order -50) spawns the character picked on the select screen (`CharacterSelection.Selected`, from the roster) in `Awake`, so everything that looks for the `Player` tag in `Start` finds it. Never reference the player from scene objects; find it by tag. The last pick is saved (PlayerPrefs, by asset name), so playing the game scene directly uses it.
- `Targeting.Nearest(tag, from, maxRange)` (`Common/Utils`) is the one place that finds the nearest tagged target.

### 7. Prefer simple solutions
- When a cheap arithmetic compensation fixes a problem (e.g. a trigger firing twice), prefer it over tracking collections like `HashSet`s of hit targets. Add a collection only if the arithmetic would be wrong or fragile.
- Keep scripts small. No speculative abstraction, managers, or base classes "for later".

### 8. Keep things modular
- Build features as self-contained pieces that can be added, removed or swapped without editing unrelated code: a component, prefab or screen owns one feature and talks to the rest through `ActorEvents`/`GlobalEvents`, `GameInput`, data assets (`UpgradeDefinition`, `CharacterRoster`) or a small public API (`SettingsMenu.Open(onClosed)`), never through references into another feature's internals.
- Content is data, not code: a new character, upgrade, enemy or setting should be new assets plus at most a new enum value, with no lists of content hardcoded in scripts and no `if (character == ...)` branches.
- Prefabs work on their own when dropped into a scene: they find what they need by tag, events or self-loading (`SelectionPointer`, `GameCursor`) instead of needing scene-specific wiring, and a missing optional piece logs a warning instead of breaking.
- Modular doesn't mean abstract (rule 7): split along real feature boundaries that already exist, not for hypothetical future reuse.

## Unity and physics conventions

- 2D physics only: `Rigidbody2D`, `Collider2D`, `OnTriggerEnter2D/Stay2D`, `Physics2D.*`. Movement sets `rb.linearVelocity` in `FixedUpdate`, and input is read in `Update`.
- Layers (`ProjectSettings/TagManager.asset`): `Water` (obstacle and spawn check), `PlayerProjectile`, `EnemyProjectile`, `Player`, `Enemy`, `Pickup`, `CameraWalls`, `Terrain` (trees and stumps), `PlayerFeet` (a small capsule on the player's `Feet` child). Terrain collides only with PlayerFeet, so the player bumps into trunks with their feet and can stand right below a tree, while enemies, projectiles, pickups and the player's body pass through. Scripts that detect the player through a child collider check `attachedRigidbody.CompareTag("Player")`, because only the root is tagged. Collision behaviour is controlled with the layer matrix and `Physics2D.IgnoreLayerCollision` (player i-frames, camera walls).
- Tags: `Player` and `Enemy` are used for targeting (`FindWithTag`, `FindGameObjectsWithTag`, `CompareTag`).
- Tilemaps sit at Z = 0. Draw order comes from sorting layers/order, not Z offsets. Within the same sorting order, sprites are sorted by Y (`Renderer2D` transparency sort mode Custom Axis, (0, 1, 0)), so lower on screen draws in front. URP 2D only reads this from `Assets/Settings/Renderer2D.asset`, not from the Graphics project settings, and in YAML Custom Axis is `m_TransparencySortMode: 3` (2 is Orthographic, which sorts by Z, so everything at Z = 0 ties and the draw order flickers between animation frames). Every animated sprite at order 10 (characters, enemies, gold) uses Sort Point = Pivot, because Center follows each frame's cropped bounds and jitters near the switch line. The player, enemies, gold and trees share order 10 so they all sort by Y together (projectiles and effects sit above at 12). Trees fade (`CanopyFade`, a trigger over the canopy) while the player is behind them, so the player is never hidden. Tree and stump sprites use Sort Point = Pivot, with the pivot about 0.4 units above the trunk base (the player's feet-to-centre height) so the player draws in front of a tree when standing below it and behind it when above.
- Trees and stumps are placed at run start by `TerrainScatter` (random positions with a minimum spacing, away from the player start and the water). Tune how many show on screen with `count` and `minSpacing`, not by hand-placing them.
- Ground variety comes from soft-edged patch sprites (`GroundPatches/`), cut from the palette's color1/2 grass texture with a dithered edge and no outline, placed by a second `TerrainScatter` (the `GroundPatches` scene object) at sorting order 3, just above the ground tilemap (2). Don't build patches from the palette's border tiles: their outline is made for water edges and doesn't blend into grass.
- The playable area is the rectangular grass island. The water around it (`TilemapObstacle`, a `TilemapCollider2D` on the `Water` layer) is the map boundary and the spawn check. Don't add anything inside the island that blocks enemies unless pathfinding comes back (trees and stumps only block the player).
- Pausing uses `Time.timeScale = 0`. Every exit path from a paused screen or scene load must restore `Time.timeScale = 1f`.
- Editor-only helpers (`OnDrawGizmosSelected`, `OnValidate`) are wrapped in `#if UNITY_EDITOR`.
- Spawner stat injection happens between `Instantiate` (runs `Awake`) and the first frame (`Start`). Components that read spawner-set stats (e.g. `EnemyHealth`) must read them in `Start`, not `Awake`.

## Input and UI (keyboard and touch)

- **All input goes through the static `GameInput`** (`Assets/Input/GameInput.cs`, execution order -100): `Move`, `InteractPressed`, `DashPressed` (Space / gamepad south), `PausePressed` (Esc / P / gamepad Start), `NavigateLeft/Right/Up/Down`. Never read `Keyboard.current`, the mouse or touches directly in gameplay scripts. The game must be fully playable with the keyboard alone, and on phones and tablets with on-screen touch controls: a joystick on the left that moves the player and a dash button on the right. Touch controls feed the same `GameInput` actions, so gameplay code never knows which device is used. Menus and upgrade cards must work with both keyboard navigation and taps.
- Pause opens the `PauseMenu` (Resume, Settings, Main Menu) unless another screen has already paused the game (`Time.timeScale == 0`). Nothing quits a run instantly; leaving goes through `MainMenuButton`. Esc is not enough on the web, because browsers use it to leave fullscreen, so P is also bound.
- The settings page (`SettingsMenu`, rows of `SettingsRow`) opens from the main menu and the pause menu, which instantiate the prefab and pass a callback to return to. Values live in the static `GameSettings` (PlayerPrefs). A new setting is a value appended to `SettingType`, a case in `GameSettings`, and a row in the prefab.
- Menus handle their own selection with `GameInput` (select index, call `Button.Select()`, `onClick.Invoke()` on Interact). Follow `DefeatScreen`/`VictoryScreen`/`UpgradeScreen`, or `PauseMenu`/`SettingsMenu`/`MainMenuController`, which also pick up a mouse click (they read the EventSystem's selection each frame). Menus that do that must set their buttons' Navigation to None: the scenes' UI input module uses Unity's default UI actions, so arrow keys would otherwise move the selection twice, and Enter/Space/gamepad South already click the selected button (so never add those keys to Interact).
- The selected button is marked by `SelectionPointer`, a Tiny Swords pointing hand that follows `EventSystem.currentSelectedGameObject` on every canvas. It and the arrow mouse cursor (`GameCursor`) load themselves from `Common/UI/Pointer/Resources` at startup, so menus only need to select their buttons.
- Reusable buttons are self-wiring components with `[RequireComponent(typeof(Button))]` that add their listener in `Start` (`RestartButton`, `MainMenuButton`, `QuitButton`). The main menu is Play / Settings / Quit Game: Play opens the `CharacterSelectScreen` (portraits on top; Back / Talents / Start below, with Talents disabled until the talent system exists and skipped by keyboard navigation). Picking a portrait only selects the character; only Start begins the run.
- Victory and defeat screens follow the same pattern: subscribe to a `GlobalEvents` event, show the panel, pause.
- Menu windows use the scroll (`UI/Background Textures/Banner.png`) as a Tiled image with a dark title. Patterned backgrounds must be Tiled, not Sliced: slicing stretches the middle of the texture across the window.
- UI should match the cartoonish Tiny Swords style. Text uses TextMeshPro.

## Audio

- `MusicController` swaps looping clips on `GlobalEvents` (normal / boss / defeat / victory) and doesn't restart a clip that is already playing.
- Volumes come from `GameSettings`: master sets `AudioListener.volume`, music controllers scale their source by `MusicVolume` (and listen to `OnVolumeChanged`), and sound effects multiply their `PlayOneShot` volume by `EffectsVolume`. A new sound source must apply one of these.
- `ActorSoundController` sits on each actor, listens to `ActorEvents`, and plays a random clip from a `SoundPool` with `PlayOneShot`. New SFX = new pool field + subscription, with clips assigned in the Inspector. Audio files go in the owning entity's `AudioFiles/` folder.

## Code style

Match the newer files (`ActorEvents`, `BossMelee`, `UpgradeScreen`):

- File-scoped braces style `namespace X { ... }` matching the folder path. One public type per file, with the file named after the type. Enums end in `Enum` or `Type` (`EnemyMoveStateEnum`, `StatType`).
- Private fields use `_camelCase`. Serialized fields are `[SerializeField] private camelCase`. Public API uses PascalCase properties and methods. Constants use PascalCase (`InvulnerabilityDuration`). Some older scripts (`PlayerMovement`, `EnemyMelee`) use un-prefixed private fields. Don't mass-rename them, but use `_camelCase` in new code.
- Get components in `Awake`, find the player with `GameObject.FindWithTag("Player")` in `Start`, and log a `Debug.LogWarning`/`LogError` with the class name prefix if a required piece is missing.
- Use expression-bodied members for one-line forwarders and subscriptions, and align `+=`/`-=` blocks vertically.
- Use guard clauses with early `return`. Write few comments: only explain non-obvious *why* (see the wall-orientation comment in `Projectile`).
- Events: `public event Action OnSomething;` raised with `?.Invoke()`.
- A handler that has to match a typed event (`Action<Vector2>`) but doesn't use the value names the parameter `_` (`private void HideHealthBar(Vector2 _)`). Don't use a lambda instead, because it can't be unsubscribed in `OnDisable`.

## Known quirks and debt (be aware, don't "fix" silently)

- Static state and `Physics2D.IgnoreLayerCollision` settings survive scene loads. Reset them at run start or in `OnDisable` (see `GlobalEvents.ResetRunState()` called from `CameraWalls.Awake`, and `PlayerHealth.OnDisable`).
- `PlayerHealth.OnHealthChanged`, `PlayerGold.OnGoldChanged`, and the state-changed actions on the movement-state scripts are static per-class events rather than going through the buses.
- `CameraFollow`/`CameraWalls` subscribe in `Start`/`OnDestroy` instead of `OnEnable`/`OnDisable`.
- A few namespaces don't match their folder (`BossTimer` → `GameObjects.Enemy.Common.Spawner`, the UI buttons → `GameObjects.Common.UI.Scripts`, `UpgradeDefinition` → `GameObjects.Common.Upgrades`). `GameInput` is in the global namespace.
- Targeting uses `FindGameObjectsWithTag` every frame. This is fine at the current scale, but consider it if enemy counts grow a lot.

Mention these when they matter for your change. Ask before broad refactors.

## Workflow

- Hand-editing Unity YAML (`.prefab`, `.unity`, `.anim`, `.controller`, `.asset`, `.meta`) is allowed for wiring components, building prefabs, animation clips/events and controllers. Base new assets on an existing working one (e.g. copy `MeleeEnemy/RedKnight/RedKnight.prefab` for a new melee enemy) and keep the structure intact:
  - Every new asset gets a fresh random 32-hex GUID in its `.meta`. Every new object inside a file gets a fresh `fileID` that is unique within that file, and all internal `{fileID: X}` references are remapped consistently.
  - Script references use `{fileID: 11500000, guid: <script .meta guid>, type: 3}`. Sprite references use the sprite's internal ID from the texture `.meta` (`nameFileIdTable`). Clips and controllers use `fileID: 7400000` / `9100000`.
  - Serialized field names in YAML must match the C# field names exactly.
  - Don't reorder or regenerate existing fileIDs/GUIDs, because other assets reference them.
  - Things that still need the editor: slicing new spritesheets when no frame size is known, and anything that needs visual tuning (collider sizes, offsets). List those steps for the user, and always say that the result must be checked by opening the prefab and playing in the editor.
- New character art can start from `Tools/SpriteGen/bombardier_gen.py` (numpy + Pillow): it draws Idle/Run/Shoot sheets as full 192px cells in the Tiny Swords palette with 2px outlines (1px only on tiny details), sized to match the other units via `K`, and writes a preview next to the archer to `Tools/SpriteGen/out/` (gitignored). Copy it per character, change `frame()` and the pose lists, and check the preview before installing the sheets.
- **Review after every feature or fix.** Before committing, review the change (and the code it touches) so the game runs as smoothly as possible and stays modular:
  - Performance: no allocations or `GetComponent`/`Find*` calls in `Update`/`FixedUpdate` or in per-hit/per-shot paths, cache what can be cached, avoid per-frame work that an event could replace, and keep asset import settings lean (audio, textures).
  - Over-engineering: remove code, fields, events or abstractions the change doesn't need, and prefer the simpler solution (rule 7).
  - Modularity (rule 8): the feature is self-contained, talks to others only through events, `GameInput`, data assets or a small public API, adds content as data instead of hardcoded lists, and can be removed without touching unrelated scripts.
  - Optimisations must not break the other rules in this file (component split, event buses, animation-driven timing, Inspector tunables, all input through `GameInput`), and they go in their own commit when they are separate from the feature. Report what the review found and changed, or say that nothing needed changing.
- Always keep `.meta` files with their assets, and never delete or regenerate GUIDs.
- Don't touch `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `obj/`, or third-party folders (`NavMeshPlus-master`, `Tiny Swords`, `TextMesh Pro`). Leave `_Recovery/` to the user.
- Git: work on `master`. Commit messages are plain, descriptive sentences, with no `Co-Authored-By` or other AI attribution trailers (same for PR descriptions). Group changes into separate commits by logical change (one fix or feature per commit, docs apart from code) so each one can be reverted without affecting the others. Stage files explicitly, and never sweep in unrelated working-tree changes (e.g. `ProjectSettings/`, `_Recovery/`). `.gif` files are tracked with Git LFS. WebGL build files are regular git objects (not LFS), and pushing to `master` redeploys the web build. Commit or push only when asked.

## Devblogs and docs

The project is also a course deliverable (GMD). The devblog lives in `Blog posts/BlogPost<N>.md`, with media in `Blog posts/BlogPost<N>Misc/`, and each post is linked from `README.md`.

- Voice: first person, casual and self-deprecating humour ("primate brain", scope-creep jokes), but technically concrete. Each post covers a milestone: the goals from the GDD, what shipped, and a tour of the notable systems with short code excerpts, **naming the design principle or pattern** behind each one (SRP, Observer, Bridge, Dependency Inversion, Open/Closed, data-driven design). Each post ends with a "Next up" list. GIFs and screenshots show the results.
- **Don't put hyperlinks inline in posts.** Add external sources to `Blog posts/Links.md` (URL + short description). The README's "References & Resources" section mirrors the important ones.
- In the "Topics from the requirements" list, each bullet is one short, high-level sentence with no code snippets and no class/component names.
- Credit third-party assets (Tiny Swords / Pixel Frog, ebunny music).
