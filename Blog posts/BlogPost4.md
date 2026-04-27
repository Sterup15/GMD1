# Blog post #4
## Milestone #2: Core game concept systems

This time I'm writing the blog post after having implemented all the work, instead of while I'm doing it, and then highlighting the most notable parts here. Partly because I got caught up actually building stuff, and partly because writing a blog post mid-flow feels like stopping a car on the highway to admire the scenery.

Looking at Milestone #2 from the design document, the goals were:
* Enemy spawner implementation
* Projectile implementation
* Player attributes implementation (Damage, Move speed, Attack speed, Health)
* Run progression system — enemies spawn faster over time

I ended up shipping all of that, plus a Gold & XP system, an upgrade screen, a ranged enemy type, proper NavMesh 2D pathfinding, and a tilemap with actual graphics. So either I was very efficient or I very much lost track of scope. Probably the latter.

## A note on architecture

Before getting into the individual systems, it's worth saying something about how the whole thing is structured, because it informed pretty much every decision in this milestone.

Unity is built around a **component-based architecture**, instead of deep inheritance hierarchies where a `RangedEnemy` extends `Enemy` which extends `Actor` and so on, you compose behaviour by attaching small, focused scripts (components) to GameObjects. A melee enemy and a ranged enemy don't share a base class; they share components like `Stats`, `EnemyPathfinder`, and `EnemyHealth`.

This is often called **composition over inheritance**, and it pairs well with the **Single Responsibility Principle** (SRP) from SOLID — the idea that each class should have one job. In practice this means keeping scripts small and focused, and letting the GameObject act as the glue. When something goes wrong it's much easier to isolate which component is at fault, and adding a new enemy type is a matter of mixing and matching existing components rather than extending a class and overriding a pile of methods.

The rest of the post is basically a tour through where I tried to apply this, and where it paid off.

## Stats system

The foundation of basically everything in this milestone is a universal `Stats` component, shared by both the player and all enemy types. Instead of hardcoding damage or move speed on each script, every actor just has a `Stats` component with a handful of `Stat` fields:

```csharp
public class Stats : MonoBehaviour
{
    public Stat MoveSpeed;
    public Stat FireRate;
    public Stat Damage;
    public Stat MaxHealth;
    public Stat ShootRange;
    public Stat PickupRange;
}
```

Each `Stat` holds a base value plus a bonus, exposes a `Value` property combining them, and fires an `OnValueChanged` event whenever either changes:

```csharp
public class Stat
{
    [SerializeField] private float baseValue;
    private float _bonus;

    public float Value => baseValue + _bonus;
    public event Action OnValueChanged;

    public void AddBonus(float amount) { _bonus += amount; OnValueChanged?.Invoke(); }
}
```

That event is what makes the system genuinely useful rather than just a tidy container. This is the **Observer pattern** — instead of every consumer polling `stat.Value` each frame to check if something changed, they subscribe to `OnValueChanged` once and react only when it actually does. For example, the health bar updates itself when `MaxHealth` changes, and the NavMesh agent's stopping distance updates when `ShootRange` changes. Neither of those consumers needs to know what triggered the change — they just react.

The base+bonus split is also worth calling out. It's a classic RPG stat model: a `SetBaseValue` call is used by the spawner to configure difficulty, while `AddBonus` is used by the upgrade system. Keeping them separate means an upgrade can never accidentally be confused with a difficulty scaling, and removing a bonus is always safe.

## Projectile implementation

The `Projectile` script is deliberately simple: it gets a direction, sets velocity via `Rigidbody2D`, rotates itself to match, and destroys itself on impact or after a lifetime:

```csharp
public void Launch(Vector2 direction)
{
    direction = direction.normalized;
    rb.linearVelocity = direction * speed;
    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    transform.rotation = Quaternion.Euler(0f, 0f, angle);
    Destroy(gameObject, lifetime);
}

private void OnTriggerEnter2D(Collider2D other)
{
    if (other.TryGetComponent<IDamageable>(out var target))
    {
        target.TakeDamage(damage);
        Destroy(gameObject);
    }
}
```

The interesting bit here is `IDamageable`. Rather than the projectile checking `if (other.CompareTag("Player"))` or `if (other.CompareTag("Enemy"))` and calling different methods on each, it just asks: does this thing implement `IDamageable`? If yes, deal damage to it. That's it.

This is the **Dependency Inversion Principle** at work — the `Projectile` depends on an abstraction (`IDamageable`) rather than concrete classes (`PlayerHealth`, `EnemyHealth`). The practical benefit is that adding a new damageable type (a destructible barrel, a boss) requires zero changes to `Projectile`. You just implement the interface on the new thing and it works.

The `ProjectileSpawner` component handles targeting and instantiation, and it supports two target modes: `Player` (used by enemies) and `NearestEnemy` (used by the player). Both the player and ranged enemies share the same spawner component — meaning upgrades to damage automatically apply to both, because both read from the same `Stats.Damage` stat that the spawner passes through at fire time.

## Enemy spawner and run progression

The enemy spawner is where the run progression system lives. Spawn rate, enemy health, speed, and damage all scale over time using Unity's `AnimationCurve`, which lets me shape the difficulty ramp visually in the inspector rather than hardcoding formulas:

```csharp
private void ScheduleNextSpawn()
{
    float rate = Mathf.Max(0.1f, spawnRateCurve.Evaluate(_runTime));
    _nextSpawnTime = Time.time + baseSpawnInterval / rate;
}
```

This is a good example of separating **configuration from logic**. The code doesn't know or care what the curve looks like, it just evaluates it at the current run time and acts on the result. Tuning the game feel is now purely a data problem, solvable by adjusting curve handles.

The spawner supports multiple enemy types via an `EnemySpawnConfig` list, each with its own spawn weight, base stats, and per-stat difficulty curves. Weighted random selection picks which type spawns each time. Spawn positions are checked against the Water layer using `Physics2D.OverlapPoint` so enemies don't materialise inside tiles, with a fallback if 10 attempts all hit something, because sometimes the map just isn't cooperating.

## Gold, XP, and the upgrade system

The design doc listed "player attributes implementation" and "run progression", but I ended up combining those into a Gold & XP loop. Enemies drop gold pickups on death, the player walks near them to collect (driven by a `PickupRange` stat), gold fills a level bar, and on level-up the game pauses and presents 3 random upgrade cards.

The level thresholds are driven by an `AnimationCurve` too, so early levels are cheap and later ones progressively more expensive without writing any maths:

```csharp
private int GetThreshold() => Mathf.Max(1, Mathf.RoundToInt(goldThresholdCurve.Evaluate(Level)));
```

Upgrades are defined as `ScriptableObject` assets — Unity's way of storing data independently of any scene or GameObject. Each `UpgradeDefinition` just specifies which `StatType` to buff and by how much:

```csharp
[CreateAssetMenu(menuName = "BulletHell/Upgrade Definition")]
public class UpgradeDefinition : ScriptableObject
{
    public string displayName;
    public StatType statType;
    public float bonusAmount;
    public bool canRepeat = true;
}
```

The upgrade screen reads a pool of these assets, draws 3 at random, and on pick calls `stat.AddBonus(amount)` on the player's Stats. That's the whole pick logic: one line, because the Observer pattern on `Stat` handles propagating the change to everything that cares about it.

This is **data-driven design**, and it follows the **Open/Closed Principle**: the system is open for extension (add new upgrades) but closed for modification (no code changes needed). Want to add a "+20% fire rate" upgrade? Create a new ScriptableObject asset, point it at `StatType.FireRate`, set the bonus, drop it in the pool. Done.

## Ranged enemy type

Alongside the existing melee knight I added a ranged archer enemy. Rather than cramming both behaviours into one shared script with an `enemyType` enum and `if (enemyType == EnemyType.Ranged)` branches all over the place (which I knew would turn into a mess fast), I made separate `EnemyMelee` and `EnemyRanged` scripts.

This is SRP in practice: each script has one job, handles one type of enemy, and has no dead code paths (One could argue that the `ProjectileSpawner` should be split up as well). They share components (`Stats`, `EnemyPathfinder`, `EnemyHealth`, `ProjectileSpawner`) via composition rather than inheritance. The ranged enemy simply has a `ProjectileSpawner` attached; the melee enemy doesn't. No base class needed.

The ranged enemy chases the player until it's within `ShootRange`, then plants itself and starts the attack animation. The actual projectile firing happens via an **animation event**: a callback Unity can trigger at a specific frame of an animation clip:

```csharp
// Called by Unity at the right frame of the attack animation
public void OnShotFired()
{
    _spawner.Fire();
}
```

A second animation event (`OnAttackComplete`) at the end of the clip resets the attacking flag. This is a neat decoupling trick: the enemy script doesn't count down a timer or poll the animator state, the animation itself drives the gameplay, ensuring the projectile always fires at the exact visual moment the archer releases. If I slow the animation down, the shot timing follows automatically.

## NavMesh 2D pathfinding

The old enemy movement was just a straight line towards the player, fine for an empty test room, not fine once there are walls and water. So I integrated **NavMeshPlus**, a community extension that brings Unity's NavMesh system into 2D.

Setting it up required a bit of ceremony (yay tutorials): a `NavMeshSurface` with a `CollectSources2d` component rotated -90 on X (Unity's 3D NavMesh lives on the XZ plane, so 2D needs a rotation to lie flat), the ground tilemap tagged Walkable and the water tilemap tagged Not Walkable via `NavMeshModifier`.

The `EnemyPathfinder` component is where the interesting architecture is. Unity's `NavMeshAgent` is designed for 3D, it wants to own the GameObject's position, rotation, and up-axis. A 2D game drives movement via `Rigidbody2D` instead, so the two systems would fight each other if simply connected. `EnemyPathfinder` acts as a **bridge** between them: `updatePosition`, `updateRotation` and `updateUpAxis` are all disabled on the agent, and position is kept in sync manually each frame via `nextPosition`. The component exposes a single method that returns the desired steering direction, which the enemy scripts apply to their own Rigidbody:

```csharp
public Vector2 GetSteerDirection()
{
    _agent.SetDestination(_player.position);
    _agent.nextPosition = _rb.position;
    return new Vector2(_agent.desiredVelocity.x, _agent.desiredVelocity.y).normalized;
}
```

Neither `EnemyMelee` nor `EnemyRanged` needs to know anything about NavMesh internals — they just call `GetSteerDirection()` and move. That's the **Bridge pattern** in action: wrapping an incompatible system behind an interface that the rest of the code can use cleanly.

Stopping distance is wired to the `ShootRange` stat and updates live when the stat changes (courtesy of the Observer pattern again), so the ranged enemy automatically holds its preferred distance even after the player upgrades its shoot range.

## Tilemap graphics

The level now has an actual background instead of the void it was living in before. I brought the ground and water tilemaps in using sprites from the Tiny Swords asset pack. Z-positioning was set for all tilemaps to Z=0, with rendering order handled by sorting layers rather than Z offset, which plays nicer with the 2D NavMesh setup.

## Next up
* Win condition — kill X enemies within a time limit, or a boss fight at the end
* Proper run start/end flow
* More polish and balance tuning
* Sounds? Huh?
* Maybe a main menu? Wild concept, I know
