# Review guide

How to review a feature, fix or refactor before it is handed over for playtest and committed. Every change gets a
review, however small. A compile check is not a review. The rules being checked live in `AGENTS.md`; this guide is
the checklist for applying them, plus the mistakes that keep coming back.

## When

1. Build the change.
2. Compile-check it.
3. **Review it with this guide** and fix what it finds.
4. Hand it over with a **Review** section and a test list.
5. Commit only after the playtest is confirmed (code first, docs in their own commit).

## Checklist

### Correctness
- Every `+=` on an event has a matching `-=` (`OnEnable`/`OnDisable`; static events always).
- Static state, static references and `Physics2D` settings survive scene loads: reset on scene load or in `OnDisable`.
- Pause (`GamePause`), restart, main menu, death mid-action (dash, burst, hit window), boss spawn and boss death
  have each been walked through for the changed code.
- Spawner-injected values are read in `Start`, not `Awake`.
- Pooled objects reset everything they change in `OnEnable` or in their spawner's setup call, and release
  themselves instead of `Destroy`.
- Nothing silently does the wrong thing when a piece is missing (a layer, a prefab, a registry entry): it logs a
  warning or error with the class name and falls back to something harmless.

### Unity wiring (hand-edited YAML)
- New assets have fresh GUIDs; new objects have fileIDs unique in their file; every `{fileID: X}` points at an
  object in the same file (or is 0 / an external GUID).
- Serialized field names in YAML match the C# names; renamed or removed fields are updated on every prefab.
- The same new object written into two copies of a file (e.g. the working scene and the clean HEAD copy) uses the
  same IDs in both.
- Layers: a new layer is ticked in the collision matrix for everything that should touch it, including all three
  player layers (`Player`, `PlayerHitSafe`, `PlayerDashSafe`).
- Scene and ProjectSettings changes are staged without the editor noise (see `AGENTS.md`, Workflow).

### Performance
- No allocations, `GetComponent`, `Find*`, `Camera.main` or LINQ in `Update`/`FixedUpdate` or in per-hit,
  per-shot or per-death paths. Cache in `Awake`.
- Strings are only rebuilt when the value changes (`SetText("{0}", n)` for numbers in TextMeshPro).
- **Anything spawned per shot, hit, blast or death goes through `PrefabPool`** or an existing shared object
  (`SoundEffects` for one-shot sounds), never `Instantiate`/`Destroy` or `new GameObject`.
- Per-frame work that an event could replace is replaced.
- Optimisations that only change the WebGL build are skipped: the web build is for demoing, not production.

### Simplicity (rule 7)
- No code, fields, events or abstractions the change doesn't use. Remove what a refactor made unused.
- The simplest working solution, unless it would be wrong or fragile.

### Modularity (rule 8)
- The feature talks to the rest through `ActorEvents`/`GlobalEvents`, `GameInput`, registries, data assets or a
  small public API, and could be removed without touching unrelated scripts.
- `Common` code does not depend on `Player` or `Enemy` code. If it needs something from them, the dependency points
  the other way (they implement a `Common` interface or raise a `Common` event). The menus for player features
  (character select, loadout) are the exception, since they are those features' UI.
- Content is data (assets, enum values), not lists or `if` branches in code.
- Repeated boilerplate is shared instead of copied (e.g. menu screens derive from `MenuScreen`).

### Docs and comments
- Comments still describe the code after the change, including comments in files the change made stale.
- `AGENTS.md` describes the new behaviour; FutureWork (local only) is updated.

## Reporting

The hand-off message gets a **Review** section before the test list:
- **Fixed:** what the review found and changed.
- **Checked, fine:** the risky areas that were walked through.
- **Trade-offs / left as is:** what was seen but deliberately not changed, and why.
- Or "nothing needed changing".

If a finding matches one in the list below, say so, and add to the list when a new kind of mistake shows up twice.

## Known recurring findings

Patterns found more than once. Check these first.

| Pattern | Seen in | What to do |
|---|---|---|
| Objects created per event instead of pooled or shared | Bombs, explosions, cluster blasts, Blast Dash bombs, enemy death sounds | `PrefabPool.Spawn` / `PooledObject.Release`, or `SoundEffects.Play` |
| Comments left stale after a refactor | "found by tag" comments after the `Players` registry (health bar, player spawner, enemy retreat) | Grep for the old concept's name after a refactor and fix every comment |
| `Common` depending on `Player`/`Enemy` | Health bar and steak pickup → `PlayerHealth`, `ProjectileSpawner` → `EnemyTargetSelector`, `RunPoints` → `CharacterSelection` | Invert it: a `Common` interface or event that the player/enemy side implements or raises |
| Copy-pasted boilerplate | Six menu screens with their own open/close/input code; two music controllers with the same volume code | Share it in one place (`MenuScreen`, `MusicVolume`) |
| Hand-edited YAML IDs | The same object written with different random IDs into the working scene and its clean copy | Generate IDs once and reuse them for every copy |
| Missing layer falls back to Default | Arena walls would have blocked enemies and arrows on the Default layer | Check `NameToLayer` and switch the feature off with an error instead |
| Compile check treated as the review | Five commits in one session went out with only a compile check | Run this checklist every time |
