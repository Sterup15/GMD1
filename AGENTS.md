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
  Common/            shared systems: Armory (permanent upgrades bought with Glory), Audio, Camera, DamageNumber, Events, Health (bar UI),
                     Progression (run points, Glory), Projectiles, Settings (saved player settings), Stats, UI (buttons, main/pause/settings/character select menus,
                     EndScreen for victory/defeat, Loadout (talent loadout before a run), Navigation (MenuNavigator), Pointer: mouse cursor and selection hand), Upgrades, Utils
    Effects/         shared visual effects, one folder each: DustPuff (death and dash smoke), Explosion (area damage, used by bombs), Fire (imported, not used yet)
    Pickups/         things the player collects, one folder each: Gold, Steak
    Terrain/         map decoration: Scripts (TerrainScatter, RandomAnimationStart, CanopyFade), Trees/ (Sprite/Anims, Tree1-4 prefabs; Stump1-4 exist but aren't scattered), Bushes/ (Anims, Bush1-4 prefabs using the Tiny Swords bush sprites), GroundPatches/ (Sprite, GroundPatch_<Colour>_<Shape> prefabs)
  Enemy/
    Common/          components shared by all enemies (EnemyHealth, EnemyHealthBar, EnemyTarget, EnemyRetreat, GoldDropper)
    MeleeEnemy/      enemy type folder
      Shared/        Scripts/Behaviour (EnemyMelee*) used by every melee enemy
      RedKnight/     Sprite/Anims, RedKnight.prefab
      Skull/         Sprite/Anims, Skull.prefab
    RangedEnemy/     enemy type folder
      Shared/        Scripts/Behaviour (EnemyRanged*) used by every ranged enemy
      RedArcher/     Sprite/Anims, RedArcher.prefab
    TogreBoss/       Scripts/Behaviour, Scripts/Health (BossHealthBar), Sprite/Anims, AudioFiles, prefab
    Spawner/         EnemySpawner (waves), BossSpawner (boss), BossTimer, SpawnPoints (dry spawn spot on the ring)
  Music/             MusicController, MainMenuMusicController, AudioFiles
  Player/
    Shared/          Scripts/{Movement,Health,Gold,Dash,Selection,Talents}, AudioFiles used by every character, CharacterRoster.asset
    Characters/      one folder per playable character
      Archer/        Sprite/Anims, ArcherSprite.prefab, AudioFiles, Archer.prefab (arrows via ProjectileSpawner), ArcherDefinition.asset, Talents/ (talent assets, prefabs and Scripts)
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
- When a component starts doing a second job, pull that job out into its own component. Example: gold dropping, the health bar and damage numbers were moved out of `EnemyHealth` into `GoldDropper`, `EnemyHealthBar` and `DamageNumberSpawner` (which listen to `OnDeath`, `OnHealthChanged` and `OnDamaged`), and the player's i-frames out of `PlayerHealth` into `PlayerInvulnerability`, which starts its window on `OnHit` and the dash events and reports the combined state with `OnInvulnerableChanged` (so `PlayerHealth` ignores damage without referencing it) and the hit window with `OnHitInvulnerabilityStarted/Ended` (the blink).
- **Every enemy type gets its own dedicated scripts.** An enemy *type* is a behaviour archetype (melee, ranged, boss, and later things like a grenade thrower), not a specific enemy or skin. Never add an `enemyType` enum or `if (isBoss)` branches to a shared script. Each type has its own `<Type>` (behaviour/movement), `<Type>MovementState` (state machine and animation), and `<Type>AnimationEvents` scripts. They share only the generic components in `Enemy/Common`.
- **A new enemy of an existing type reuses that type's scripts.** It only gets its own folder with sprites, an Animator Controller (same state names, e.g. Idle/Move/Attack), stats and a prefab. Example: `Skull` is a melee enemy that uses the `EnemyMelee*` scripts. Write new scripts only when the enemy needs behaviour its type doesn't have, and then it's a new type.

### 2. Two event buses: decouple, don't reference siblings
- **`ActorEvents`** (`Common/Events/ActorEvents.cs`) is a per-actor bus on each actor's root GameObject. Components on the same actor talk through it, not through direct references to each other. Adding a signal means: add `public event Action OnX;` plus a `public void X() => OnX?.Invoke();` raiser.
- **`GlobalEvents`** (`Common/Events/GlobalEvents.cs`) is a static, game-wide bus for moments the whole game cares about: boss spawned/defeated, player died/won, level up, enemy killed, run ended. `OnBossSpawnedWithBoss` fires at the same moment as `OnBossSpawned` and carries the boss's `ActorEvents` and name, for listeners that need the boss itself (its health bar). Use it for cross-system reactions (music, UI screens, camera, victory) so nothing has to reference the spawner or the player.
- Subscribe in `OnEnable` and **always unsubscribe in `OnDisable`**. Static events survive scene reloads, so a missing unsubscribe leads to null-reference errors after a restart. Cache `ActorEvents` in `Awake` (`GetComponent`, or `GetComponentInParent` from a child sprite).
- Don't over-decouple. Plain local events are fine for one-to-one wiring (e.g. `BossTimer.OnTimerComplete` → `BossSpawner`, `BossHitbox.OnHit` → `BossMelee`).

### 3. Animation drives gameplay timing
- Gameplay moments (firing a shot, finishing an attack, the end of a windup, opening and closing the hitbox, footsteps, swings, death complete) are **Animation Events** on the clips, not timers. The one exception is the dash: there's no dash clip, so `PlayerDash` times it (its speed is `MoveSpeed` × `dashSpeedPerMoveSpeed`, so Swiftness also lengthens the dash) and raises `DashStarted`/`DashEnded` on `ActorEvents`, which movement (dash velocity), `PlayerInvulnerability` (dash i-frames) and `DashDust` listen to.
- Each actor's sprite child has an `*AnimationEvents` script whose methods only forward to `ActorEvents` (`public void OnShotFired() => _events.ShotFired();`). It never references gameplay components.
- Attack speed scales `animator.speed` from `Stats.FireRate`, so shot timing follows the animation.
- State machines use `EnemyMoveStateEnum` (Idle, Move, Windup, Attack, Recover, Die) and play clips by name with `animator.Play("Idle")` (constants in the script), not through Animator transitions. Locked states (Attack/Windup/Recover/Die) are exited only by animation events.
- Death is two-phase. `EnemyHealth` raises `DeathStarted` if anything listens (the boss plays a Die clip, then `DeathComplete` → `FinaliseDeath()`). Otherwise it finalises immediately. `FinaliseDeath` raises `Death` (gold drop, spawner counters) and destroys the actor.

### 4. Universal Stats with the Observer pattern
- Every actor has a `Stats` component: `MoveSpeed, FireRate, Damage, MaxHealth, ShootRange, PickupRange, Bounce, Penetration, Haste, DashCharges, Area, Luck, ProjectileCount` (`ProjectileCount` is extra projectiles for talents that release several, base 0, read only by talents at the level that uses it; `Area` multiplies explosion radius, base 1; `Luck` raises the odds of rarer upgrade cards: the rarity weights in `UpgradeScreen` add up to 100 so they read as percentages, and each point of Luck adds that rarity's `luckBonus` percentage points (+3 Uncommon, +2 Rare, +1 Legendary, -6 Common). Luck is capped at `UpgradeScreen.maxLuck` (10, where Common reaches 0%), and Luck cards stop being offered once it's reached). Never hardcode these on behaviour scripts. Read `stats.X.Value` instead.
- `Stat` = `baseValue + _bonus`. **`SetBaseValue`** is for difficulty scaling (spawner). **`AddBonus`/`RemoveBonus`** is for upgrades. Don't mix them up.
- React to changes by subscribing to `Stat.OnValueChanged` instead of polling (e.g. `PlayerHealth` reacting to `MaxHealth`).
- Adding a stat: add the field to `Stats`, a value to `StatType`, and a case to `Stats.GetStat`.
- `Haste` shortens every cooldown without a cap: `cooldown = base / (1 + Haste / 100)`, so 100 Haste halves it. New abilities with a cooldown must use the same formula.
- `StatType` values are stored as ints in the upgrade assets, so new stats are only ever appended at the end of the enum.
- Integer-like stats (`Damage`, `MaxHealth`, `Bounce`, `Penetration`, `DashCharges`) are floats rounded with `Mathf.RoundToInt` at the point of use.

### 5. Data-driven, tunable in the Inspector (Open/Closed)
- **Upgrades** are `UpgradeDefinition` ScriptableObjects (`Create > BulletHell > Upgrade Definition`) in `Common/Upgrades/Resources/Upgrades/<Rarity>/`, loaded with `Resources.LoadAll`. A new stat upgrade is a new asset, not new code. Naming convention: `AD+1`, `AS+3`, `Haste+2`, `Bounce+2`, `Pen+1`, with the suffix matching the rarity tier (Common +1 … Legendary +4). Special stats with only a few tiers (`Bounce`, `Pen`, `Dash`) use +1/+2 on the two rarest tiers. The `Dash+1`/`Dash+2` charge cards are one-time picks (the HUD has 4 charge pips) and are meant to become an item once the Items system exists.
- Upgrade cards are shared by every character and only say how many **points** they give (`points`, +1 to +4), using in-game stat names instead of numbers: Strength (`Damage`), Agility (`FireRate`), Vitality (`MaxHealth`), Swiftness (`MoveSpeed`), Sight (`ShootRange`), Greed (`PickupRange`), Haste, Blast (`Area`), Luck, Plenty (`ProjectileCount`). Never put percentages or raw values on a card. What one point is worth lives in one table, `UpgradeScreen.pointValues` (`StatPointValue`: flat, or `percentOfBase` = a percentage of the character's own `Stat.BaseValue`, so a slow-firing character doesn't outgrow a fast one). The values keep the archer's old numbers (Agility 40%, Swiftness 13.33%, Sight 8%, Greed 40%, Blast 10% of base; Strength/Vitality/Luck 1 and Haste 10 flat). Stats missing from the table (bounce, penetration, dash charges, projectile count) get 1 per point. Never change a character's base stats with `AddBonus`, because percentage points are computed from `BaseValue`. To tune how fast one character grows in a stat, use its `UpgradeScaling` component (a multiplier per stat on the character prefab, 1 when not listed, e.g. the Bombardier's Agility at 0.35). It scales only what cards add; the cards and base stats stay the same.
- A card's rarity shows as a glow behind it (`UpgradeCard.rarityGlow`, one colour per rarity with the alpha as strength: none, green, blue, orange). Cards use the scroll background like the menus.
- Cards that only make sense for some attacks have a `requirement` (`UpgradeRequirementType`: None, Projectiles, Explosions, ProjectileCount). Plenty cards need ProjectileCount, which no character has by default: a talent that reads the stat switches it on with `UpgradeEligibility.Set` when it is equipped at that level, so the card is never offered when it would do nothing. Each character prefab has an `UpgradeEligibility` component with one bool per requirement (archer: projectiles, Bombardier: explosions), and `UpgradeScreen` filters the pool by it on every level-up. Something that adds a new kind of attack mid-run (e.g. the Shoulder Ballista or Backpack Catapult items) calls `UpgradeEligibility.Set(requirement, true)`. A new requirement is a new enum value plus a bool on `UpgradeEligibility`, not character checks in the upgrade code.
- **Difficulty** uses `AnimationCurve`s (X = run time in seconds or player level, Y = multiplier or value): spawn rate, enemy health/speed/damage, gold drop, minimum enemies alive, gold threshold. Provide a default curve in `Awake` and `OnValidate` (see `CurveUtils.EaseIn`) for when the serialized curve is empty.
- Global balance knobs live on one component (e.g. `UpgradeScreen.pointValues`), not spread across many assets.
- Tunables are `[SerializeField] private` fields with `[Header]` and `[Tooltip]` where useful.

### 6. Abstractions where it pays off
- `IDamageable` (`TakeDamage(int)`) is the only thing projectiles know about. New damageable things implement it, and `Projectile` stays unchanged.
- `IPickup` (`Collect(GameObject collector)`, `Common/Pickups/Scripts`) is the only thing `PlayerPickup` knows about. A new pickup (gold, steak, magnet, ...) implements it in its own `Common/Pickups/<Name>/` folder. It decides for itself whether it is consumed (e.g. the steak stays on the ground at full health).
- Enemies move straight at the player: each behaviour script sets `rb.linearVelocity` from the normalised direction to the player. This only works because the map is convex with nothing inside it to walk around. A map with obstacles would need pathfinding back (see the `navmesh-pathfinding` branch).
- `ProjectileSpawner` is shared by the archer (`TargetModeEnum.NearestEnemy`) and enemies (`TargetModeEnum.Player`), fires on its actor's `ShotFired` event and pushes `Damage/Bounce/Penetration` from `Stats` into the projectile at fire time.
- **Characters** share everything in `Player/Shared` and differ only in their sprite child (Animator Controller with the same Idle/Move/Attack states and a `ShotFired` event on Attack), `Stats` values and **attack component**, which listens to `ShotFired`: `ProjectileSpawner` for the archer, `BombThrower` for the Bombardier. A new character is a copy of an existing character folder with its own art, attack component, `UpgradeEligibility`/`UpgradeScaling` settings and talents, plus a `<Name>Definition` asset (`CharacterDefinition`: display name, description, portrait, prefab) added to `Player/Shared/CharacterRoster.asset`. The select screen builds its portraits from the roster and reads the base stats from the prefab's `Stats`, so it needs no UI or code changes. Movement states never fire attacks themselves.
- The game scene has no placed player. `PlayerSpawner` (execution order -50) spawns the character picked on the select screen (`CharacterSelection.Selected`, from the roster) in `Awake`, and the character's `PlayerTarget` registers it with the static `Players` registry (`Common/Utils`) during that `Instantiate`, so everything that looks it up in `Start` finds it. Never reference the player from scene objects or find it by tag: use `Players.Local` for what belongs to this device (camera, HUD, upgrade cards) and `Players.Nearest`/`Players.All` for what reacts to any player (spawners, terrain, enemies), so the game is ready for more than one player. Enemies go after the player their `EnemyTargetSelector` (`Enemy/Common`, on every enemy prefab's root) picks: the nearest one not out, rechecked every 1.5 s and switched only when another player is 25% closer. Enemy behaviour scripts and `ProjectileSpawner`'s Player mode read its `Target` every time they need it (null when no player is left, and they stand still). A player who dies is marked out (`PlayerTarget` → `Players.SetOut`) and stays registered, so the camera and HUD keep them while enemies drop them at once. The last pick is saved (PlayerPrefs, by asset name), so playing the game scene directly uses it.
- Short-lived objects spawned per shot, hit or death (projectiles, damage numbers, dust puffs, gold) go through `PrefabPool.Spawn` instead of `Instantiate`, and `PooledObject.Release(gameObject)` instead of `Destroy`. A pooled script resets everything it changes in `OnEnable` or in the setup call its spawner makes (`Start` only runs once per instance), and never relies on a delayed `Destroy(go, t)`; it counts its own lifetime and releases itself.
- `Targeting.NearestEnemy(from, maxRange)` (`Common/Utils`) is the one place that finds the nearest enemy. Enemies register themselves with an `EnemyTarget` component (and drop out when they retreat or die), so lookups walk a short list instead of searching the scene. Every enemy prefab needs `EnemyTarget` on its root.

### 7. Prefer simple solutions
- When a cheap arithmetic compensation fixes a problem (e.g. a trigger firing twice), prefer it over tracking collections like `HashSet`s of hit targets. Add a collection only if the arithmetic would be wrong or fragile.
- Keep scripts small. No speculative abstraction, managers, or base classes "for later".

### 8. Keep things modular
- Build features as self-contained pieces that can be added, removed or swapped without editing unrelated code: a component, prefab or screen owns one feature and talks to the rest through `ActorEvents`/`GlobalEvents`, `GameInput`, data assets (`UpgradeDefinition`, `CharacterRoster`) or a small public API (`SettingsMenu.Open(onClosed)`), never through references into another feature's internals.
- Content is data, not code: a new character, upgrade, enemy or setting should be new assets plus at most a new enum value, with no lists of content hardcoded in scripts and no `if (character == ...)` branches.
- Prefabs work on their own when dropped into a scene: they find what they need through registries (`Players`, `Targeting`), events or self-loading (`SelectionPointer`, `GameCursor`) instead of needing scene-specific wiring, and a missing optional piece logs a warning instead of breaking.
- Modular doesn't mean abstract (rule 7): split along real feature boundaries that already exist, not for hypothetical future reuse.

## Unity and physics conventions

- 2D physics only: `Rigidbody2D`, `Collider2D`, `OnTriggerEnter2D/Stay2D`, `Physics2D.*`. Movement sets `rb.linearVelocity` in `FixedUpdate`, and input is read in `Update`.
- Layers (`ProjectSettings/TagManager.asset`): `Water` (obstacle and spawn check), `PlayerProjectile`, `EnemyProjectile`, `Player`, `Enemy`, `Pickup`, `CameraWalls`, `Terrain` (trees), `PlayerFeet` (a small capsule on the player's `Feet` child), `PlayerHitSafe` and `PlayerDashSafe` (the player's root while invulnerable). Terrain collides only with PlayerFeet, so the player bumps into trunks with their feet and can stand right below a tree, while enemies, projectiles, pickups and the player's body pass through. Scripts that detect the player through a child collider check `attachedRigidbody.CompareTag("Player")`, because only the root is tagged. Collision behaviour is controlled with the layer matrix. Player i-frames are per player: `PlayerInvulnerability` moves that player's root to `PlayerHitSafe` after a hit (enemies pass through, their arrows still land for no damage) or `PlayerDashSafe` during a dash (enemies and arrows pass through), and back to `Player` after. Both copy the `Player` row of the matrix minus what they let through, so a new layer that should touch the player must be ticked for all three. Never use `Physics2D.IgnoreLayerCollision` for one player's state, since it changes the rule for every player; only the camera walls use it (and turn it on for all three player layers in the boss fight).
- Tags: `Player` is used to check hits (`CompareTag`); players are found through `Players`, not by tag. `Enemy` is still set on enemies, but targeting goes through `Targeting`, not tags.
- Tilemaps sit at Z = 0. Draw order comes from sorting layers/order, not Z offsets. Within the same sorting order, sprites are sorted by Y (`Renderer2D` transparency sort mode Custom Axis, (0, 1, 0)), so lower on screen draws in front. URP 2D only reads this from `Assets/Settings/Renderer2D.asset`, not from the Graphics project settings, and in YAML Custom Axis is `m_TransparencySortMode: 3` (2 is Orthographic, which sorts by Z, so everything at Z = 0 ties and the draw order flickers between animation frames). Every animated sprite at order 10 (characters, enemies, gold) uses Sort Point = Pivot, because Center follows each frame's cropped bounds and jitters near the switch line. The player, enemies, gold and trees share order 10 so they all sort by Y together (projectiles and effects sit above at 12). Trees fade (`CanopyFade`, a trigger over the canopy) while the player is behind them, so the player is never hidden. Tree and stump sprites use Sort Point = Pivot, with the pivot about 0.4 units above the trunk base (the player's feet-to-centre height) so the player draws in front of a tree when standing below it and behind it when above.
- Trees are placed at run start by `TerrainScatter` (random positions with a minimum spacing, away from the player start and the water). Tune how many show on screen with `count` and `minSpacing`, not by hand-placing them. Bushes (`Bushes` scene object) are a second scatter that keeps its pieces away from the trees (`TerrainScatter.avoid` + `avoidDistance`; the avoided scatter is placed first, whatever the `Start` order). Bushes have no collider, so nothing bumps into them. Each bush prefab's root sits at the bush's visible base with a `SortingGroup` (order 10), and its sprite child is raised to stand on it, so it Y-sorts from the base even though the pack's sprites are pivoted in the middle. The pack's bush clips don't loop, so `Bushes/Anims` holds looping copies with an `Idle` state.
- Ground variety comes from soft-edged patch sprites (`GroundPatches/`), cut from the palette's color1/2 grass texture with a dithered edge and no outline, placed by a second `TerrainScatter` (the `GroundPatches` scene object) at sorting order 3, just above the ground tilemap (2). Don't build patches from the palette's border tiles: their outline is made for water edges and doesn't blend into grass.
- The playable area is the rectangular grass island. The water around it (`TilemapObstacle`, a `TilemapCollider2D` on the `Water` layer) is the map boundary and the spawn check. Don't add anything inside the island that blocks enemies unless pathfinding comes back (trees and stumps only block the player).
- Only `GamePause` (static, `Common/Utils`) touches `Time.timeScale`. A screen that stops the game calls `GamePause.Pause(this)` when it opens and `GamePause.Resume(this)` when it closes; time runs again only once no screen holds a pause, and loading a scene drops every pause, so leaving through a scene load needs no clean-up. Check `GamePause.IsPaused` instead of comparing the time scale. Online co-op will decide there what may stop time for everyone (solo keeps pausing for level-ups).
- Editor-only helpers (`OnDrawGizmosSelected`, `OnValidate`) are wrapped in `#if UNITY_EDITOR`.
- Spawner stat injection happens between `Instantiate` (runs `Awake`) and the first frame (`Start`). Components that read spawner-set stats (e.g. `EnemyHealth`) must read them in `Start`, not `Awake`.

## Input and UI (keyboard and touch)

- **All input goes through the static `GameInput`** (`Assets/Input/GameInput.cs`, execution order -100): `Move`, `InteractPressed`, `DashPressed` (Space / gamepad south), `PausePressed` (Esc / P / gamepad Start), `NavigateLeft/Right/Up/Down`. Never read `Keyboard.current`, the mouse or touches directly in gameplay scripts. The game must be fully playable with the keyboard alone, and on phones and tablets with on-screen touch controls: a joystick on the left that moves the player and a dash button on the right. Touch controls feed the same `GameInput` actions, so gameplay code never knows which device is used. Menus and upgrade cards must work with both keyboard navigation and taps.
- Pause opens the `PauseMenu` (Resume, Settings, Main Menu) unless another screen has already paused the game (`GamePause.IsPaused`). Nothing quits a run instantly; leaving goes through `MainMenuButton`. Esc is not enough on the web, because browsers use it to leave fullscreen, so P is also bound.
- The settings page (`SettingsMenu`, rows of `SettingsRow`) opens from the main menu and the pause menu, which instantiate the prefab and pass a callback to return to. Values live in the static `GameSettings` (PlayerPrefs). A new setting is a value appended to `SettingType`, a case in `GameSettings`, and a row in the prefab.
- Menus get their keyboard/gamepad selection from `MenuNavigator` (`Common/UI/Navigation`), a plain class the menu creates with its buttons in rows (`new MenuNavigator(buttons)` for one row, or one array per row). Each frame the menu calls `Navigate()` and `Submit()` on Interact. It wraps, skips disabled or hidden buttons, follows the EventSystem's selection (a button a click selected), remembers the last button per row, raises `OnSelectionChanged`, and `BlockInputThisFrame()`/`InputBlocked` keep the key that opened a menu or closed a screen on top of it from also acting on it. A menu only adds its own extras (settings change values with left/right, character select moves Interact on a portrait down to Start). Every menu button needs Navigation set to None: the scenes' UI input module uses Unity's default UI actions, so arrow keys would otherwise move the selection twice, and Enter/Space/gamepad South already click the selected button (so never add those keys to Interact).
- The selected button is marked by `SelectionPointer`, a Tiny Swords pointing hand that follows `EventSystem.currentSelectedGameObject` on every canvas. It and the arrow mouse cursor (`GameCursor`) load themselves from `Common/UI/Pointer/Resources` at startup, so menus only need to select their buttons.
- Reusable buttons are self-wiring components with `[RequireComponent(typeof(Button))]` that add their listener in `Start` (`RestartButton`, `MainMenuButton`, `QuitButton`). The main menu is Play / Settings / Quit Game: Play opens the `CharacterSelectScreen` (portraits on top; Back / Armory / Start below; Armory opens the `ArmoryScreen` on top of it). Picking a portrait only selects the character; only Start begins the run.
- Victory and defeat share one `EndScreen` script (`Common/UI/EndScreen`), set to `Victory` or `Defeat` on each prefab: it subscribes to `OnPlayerWon` or `OnPlayerDied`, shows its panel and pauses. A `RunResultsText` next to it fills the panel's results from `OnRunEnded`.

## Between runs

- `RunPoints` (one object in the game scene, `Common/Progression`) counts the run's points from `GlobalEvents`: kills (`OnEnemyKilled`, raised by `EnemyHealth` before `Death` so the boss's kill counts before the run ends), time survived (pro rata per minute, paused time excluded) and a victory bonus. On `OnPlayerDied`/`OnPlayerWon` it converts the points to **Glory**, banks them, and raises `OnRunEnded(RunResult)` with the breakdown. A defeat keeps what was earned; leaving through the pause menu banks nothing. All rates are Inspector fields on `RunPoints`.
- `Mastery` (static, `Common/Progression`) is per-character XP, keyed by the `CharacterDefinition` asset name. `RunPoints` converts the same run points into XP for `CharacterSelection.Selected` (`masteryXpPerPoint`), so a run earns both. Levels run from 1 to `Mastery.MaxLevel` (20); going from level n to n + 1 costs 350 + 50·n XP (400 for the first level, about one win or two to three losses; 16,150 XP to reach 20). The results screen shows the XP and level-ups, the select screen each character's level and progress. Mastery unlocks talents, talent levels and loadout slots (1, +1 at 8, +1 at 18). `PlayerSpawner` selects the fallback character when the game scene is played directly, so `Selected` is always set during a run.
- The **Armory** (`Common/Armory`) sells permanent upgrades for every character. Each one is an `ArmoryUpgrade` asset in `Common/Armory/Resources/Armory` (stat, value per level as flat or percent of base, max level, cost of the first level plus a flat step and/or a growth factor per level, display order), so a new upgrade is a new asset and gets its row automatically. The static `Armory` saves the bought levels (PlayerPrefs), buys them with `Glory.TrySpend` and refunds everything for free. `ArmoryBonuses` on each character prefab applies them with `AddBonus` in `Start`, so card percentages and `UpgradeScaling` are unaffected; a new character needs this component. Armory Luck counts toward the Luck cap.
- **Talents** are per-character abilities unlocked by mastery and picked in a loadout before each run. A talent is a `TalentDefinition` asset (name, description, unlock mastery level, prefab) listed on its character's `CharacterDefinition.talents`; its behaviour is a script on the talent prefab, kept in `Player/Characters/<Name>/Talents/`. `PlayerSpawner` spawns each equipped talent prefab as a child of the player (`Talents.Apply`), and the script finds `ActorEvents` and `Stats` with `GetComponentInParent` and works purely through events, never by editing the attack scripts: attacks raise `OnProjectileFired(origin, direction)` and `OnBombExploded(position, damage, radius)` for that, and `ProjectileSpawner.Spawn` fires extra projectiles with the shooter's stats. The static `Talents` saves each character's loadout and gives the slots (1, +1 at mastery 8 and 18, `Talents.ExtraSlotLevels`). Every character is meant to have 6 talents with 3 levels each, so that every mastery level unlocks exactly one thing: talents at mastery 1, 2, 4, 6, 10 and 13, their levels 2 at 3, 5, 9, 12, 15 and 17 and their levels 3 at 7, 11, 14, 16, 19 and 20 (so each level 2 comes a few levels after its talent and each level 3 a few more after that), and the extra slots at 8 and 18. The talents are listed on `CharacterDefinition.talents` in that unlock order.
- **Talent levels:** level 1 comes with the unlock; `TalentDefinition.levels` lists levels 2, 3, ... (`TalentLevel`: mastery level, Glory cost, description), bought with Glory through `Talents.TryUpgrade` and saved per talent asset. Any bought level can be equipped (`Talents.EquippedLevel`, defaulting to the highest bought one). `Talents.Apply` passes the equipped level to every `ILeveledTalent` on the spawned prefab before its first frame, so a talent reads its level from there and keeps per-level numbers in an Inspector array (e.g. `BlastDashTalent.bombsPerLevel`). A new level is data on the asset. How to design and build a talent: see "Talent blueprint" below.
- The `LoadoutScreen` opens from the select screen's Start (skipped for a character without talents); Begin starts the run. Each talent is one row (name, level, "Equipped"); the details box beside the list shows the selected talent at one level (name, picture, "Level X / Y", that level's description), with `<` `[button]` `>` at the bottom. Left/right (or the arrows) browses the bought levels and the next one; the button (or Interact) does what it says for the viewed level: the Glory price buys it (red when Glory is short), "Unlocks at Mastery X" is disabled, "Equip" equips the talent at that level, "Equipped" unequips it. Clicking a row only shows it, so a click never spends Glory.
- `Glory` is the static currency kept between runs (PlayerPrefs, saved straight away because the web build only writes on `Save`). Persistent upgrades and unlocks will spend it.
- Menu windows use the scroll (`UI/Background Textures/Banner.png`) as a Tiled image with a dark title. Patterned backgrounds must be Tiled, not Sliced: slicing stretches the middle of the texture across the window.
- UI should match the cartoonish Tiny Swords style. Text uses TextMeshPro.

### Talent blueprint

Follow this for every new talent, on every character. Reference implementations: `ClusterBombsTalent` (all three levels, Plenty hook), `BlastDashTalent` (count per level) and `BombBagTalent` (a temporary attack speed bonus through `Stat.AddBonus` instead of its own throwing, and a HUD bar on its own overlay canvas inside the talent prefab, sorting order -1 so the HUD canvas's screens cover it, which only exists while the talent is equipped).

**Design rules**
- Every character gets **6 talents × 3 levels**, plus the 2 extra slots, which fills mastery 1–20 with exactly one unlock per level. List the talents on `CharacterDefinition.talents` in unlock order (T1 first) and use this schedule:

  | | T1 | T2 | T3 | T4 | T5 | T6 | Slots |
  |---|---|---|---|---|---|---|---|
  | Level 1 (talent unlock) | 1 | 2 | 4 | 6 | 10 | 13 | 2nd at 8 |
  | Level 2 | 3 | 5 | 9 | 12 | 15 | 17 | 3rd at 18 |
  | Level 3 | 7 | 11 | 14 | 16 | 19 | 20 | |

- Glory costs: level 2 = 1500, level 3 = 5000.
- **Level 1** is the core effect: it changes *how* the character plays, not just a number. Talents can be sidegrades of each other, since the slots cap the power.
- **Level 2** is a fixed improvement of the same effect: bigger numbers (Cluster Bombs: blasts at 75% damage instead of 50%) or one extra piece (Blast Dash: a second bomb where the dash lands).
- **Level 3** adds a **stat hook**: from then on the talent reads one of the character's stats when it triggers, so upgrade cards picked during the run keep scaling it (Cluster Bombs: blasts = 3 + Plenty; planned for Blast Dash: bombs follow the dash length, which follows Move Speed). Prefer an existing stat; add a new one only if nothing fits, as a talents-only stat like Plenty (`ProjectileCount`). A talent with no natural hook may skip it.
- If the hooked stat's cards are only useful with the talent (Plenty), the cards get a requirement (`UpgradeRequirementType`) that is off on every character, and the talent switches it on in `SetLevel` when it is equipped at the hook level. Never offer a card that does nothing.
- Lower levels stay worth equipping where possible, since any bought level can be equipped.

**Build steps**
1. **Script:** `Player/Characters/<Character>/Talents/Scripts/<Name>Talent.cs`, a `MonoBehaviour` implementing `ILeveledTalent`.
   - Cache `ActorEvents` and `Stats` with `GetComponentInParent` in `Awake`, subscribe in `OnEnable`, unsubscribe in `OnDisable`.
   - React only to events (`OnShotFired`, `OnProjectileFired`, `OnBombExploded`, `OnDashStarted/Ended`, `OnAttackingChanged` from the player's movement state, ...). Never edit or reference the attack scripts. If the talent needs a moment no event covers, add the event to `ActorEvents` and raise it from the attack.
   - `SetLevel(int level)` stores the level and switches on the hook card requirement at the hook level.
   - Per-level numbers are Inspector arrays indexed with `Mathf.Clamp(level - 1, 0, length - 1)` (e.g. `damageFactorPerLevel = { 0.5, 0.75, 0.75 }`), and the hook level is a field (`plentyFromLevel = 3`).
   - Read hooked stats at trigger time (`_stats.ProjectileCount.Value`, rounded with `Mathf.RoundToInt` for counts), so cards picked mid-run apply at once.
2. **Prefab:** `<Name>Talent.prefab` next to the talent assets: an empty GameObject with the script and its fields set. It is spawned as a child of the player.
3. **Asset:** `<Name>.asset` (`Create > BulletHell > Talent`):
   - `displayName`, `description` (level 1), `unlockLevel`, `prefab`.
   - `levels` with two entries (mastery level, Glory cost, description).
   - Each description is a full sentence that stands on its own and states its numbers ("each dealing 75% of the bomb's damage"), because the details box shows one level at a time.
4. **Roster:** add the asset to the character's `CharacterDefinition.talents` at its unlock position.
5. **Docs:** update the talent's entry in the local FutureWork and anything in this file that names it.
6. **Test list for the user:**
   - The loadout shows the unlock and level masteries.
   - Each level can be bought, browsed and equipped, including a lower level.
   - A run behaves as described at each level.
   - The hook stat's cards appear only when the talent is equipped at the hook level, and each card point visibly scales the effect.

## Audio

- `MusicController` swaps looping clips on `GlobalEvents` (normal / boss / defeat / victory) and doesn't restart a clip that is already playing.
- Volumes come from `GameSettings`: master sets `AudioListener.volume`, music controllers scale their source by `MusicVolume` (and listen to `OnVolumeChanged`), and sound effects multiply their `PlayOneShot` volume by `EffectsVolume`. A new sound source must apply one of these.
- `ActorSoundController` sits on each actor, listens to `ActorEvents`, and plays a random clip from a `SoundPool` with `PlayOneShot`.
- **Hit sounds belong to what hits, not to what is hit:** `Projectile` and `Explosion` each have a `hitSound` pool (the arrow and the bomb sound different), played through the static `SoundEffects` (one shared 2D source, so a pooled or destroyed object's sound isn't cut off and no GameObject is made per sound). An explosion plays it once per blast, whether or not it hits anything. Enemies keep their own death sounds; the player keeps its own sound for taking damage, so enemy projectiles have no hit sound. New SFX = new pool field + subscription, with clips assigned in the Inspector. Audio files go in the owning entity's `AudioFiles/` folder.

## Code style

Match the newer files (`ActorEvents`, `BossMelee`, `UpgradeScreen`):

- File-scoped braces style `namespace X { ... }` matching the folder path. One public type per file, with the file named after the type. Enums end in `Enum` or `Type` (`EnemyMoveStateEnum`, `StatType`).
- Private fields use `_camelCase`. Serialized fields are `[SerializeField] private camelCase`. Public API uses PascalCase properties and methods. Constants use PascalCase (`BounceCooldown`). Some older scripts (`PlayerMovement`, `EnemyMelee`) use un-prefixed private fields. Don't mass-rename them, but use `_camelCase` in new code.
- Get components in `Awake`, find the player through `Players` in `Start`, and log a `Debug.LogWarning`/`LogError` with the class name prefix if a required piece is missing.
- Use expression-bodied members for one-line forwarders and subscriptions, and align `+=`/`-=` blocks vertically.
- Use guard clauses with early `return`. Write few comments: only explain non-obvious *why* (see the wall-orientation comment in `Projectile`).
- Events: `public event Action OnSomething;` raised with `?.Invoke()`.
- A handler that has to match a typed event (`Action<Vector2>`) but doesn't use the value names the parameter `_` (`private void HideHealthBar(Vector2 _)`). Don't use a lambda instead, because it can't be unsubscribed in `OnDisable`.

## Known quirks and debt (be aware, don't "fix" silently)

- Static state and `Physics2D.IgnoreLayerCollision` settings survive scene loads. Reset them at run start or in `OnDisable` (see `CameraWalls.Start`, which resets every wall collision). Avoid adding static run state; the camera walls watch the player's Bounce stat instead of a global flag.
- The player's health and gold changes go through its `ActorEvents` (`OnHealthChanged`, `OnGoldChanged`); HUD bars find `Players.Local` and subscribe in `Start`, with the `OnEnable`/`OnDisable` null-check pattern from `StaminaBar`.
- `CameraFollow`/`CameraWalls` subscribe in `Start`/`OnDestroy` instead of `OnEnable`/`OnDisable`.
- A few namespaces don't match their folder (`BossTimer` → `GameObjects.Enemy.Common.Spawner`, the UI buttons → `GameObjects.Common.UI.Scripts`, `UpgradeDefinition` → `GameObjects.Common.Upgrades`). `GameInput` is in the global namespace.

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
