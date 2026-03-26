# Blog post #3
## Milestone #1: The basics

The game project was officially started by making a Unity 2D universal project, doing an initial commit, and pushing it to my GMD repo.

I then set my eyes on the first actual goal/task that involved some game development: "Moving player character".
I knew that, to keep my primate brain entertained, I needed my character to be something more exciting than a moving rectangle, so I went online to find a character.

I ended to striking gold, and found a whole asset pack called "[Tiny Swords](https://pixelfrog-assets.itch.io/tiny-swords)". Which contains a bunch of animated top-down medieval assets, perfect for pleasing primate brains.
All rights go to the creator "Pixel Frog", I did not create any of these assets, bla bla bla...
![Tiny Swords GIF](BlogPost3Misc/TinySwordsGif.gif)

## Player character
As a beginning, I only wanted a player character, which needed to be able to shoot some projectiles/bullets. So I chose the archer unit and imported the animation sprites into Unity:

![Archer pngs screenshot](BlogPost3Misc/ArcherPngsScreenshot.png)

After some splicing (Making the different frames/cells of the animation the same size) and pacing/timing in the Animation window, I ended up with 3 different animations for my player character: Idle, Move and Shoot

![Archer idle gif](BlogPost3Misc/ArcherIdleGif.gif) ![Archer move gif](BlogPost3Misc/ArcherMoveGif.gif) ![Archer shoot gif](BlogPost3Misc/ArcherShootGif.gif)

After this, I made 2 scripts for controlling player movement and the animation state of the player. These allow the player to move in 8 different directions with the WASD or arrow keys, which will later be changed to the VIA Arcade controls, and for the different animations to play at the right time, although for now it is only move and idle, since the shooting has not been implemented yet.
The player movement for now looks something like this:

![Player movement gif](BlogPost3Misc/PlayerMovementGif.gif)

## Enemy prefab
I wanted a simple enemy to begin with, so I chose to make a melee enemy that follows the player. I chose a red knight character from the assets, and implemented the animations in the same way as the player character.

Next up I implemented the EnemyFollow and EnemyMovementState scripts, where the EnemyFollow script makes the enemy follow the player, and the MovementState script handles the animation states and flipping the sprite depending on which direction the character is running.
Now the enemy follows the player character relentlessly.

## Basic UI and basic hit system
I put the camera inside the player GameObject, to make the camera follow the player around the map. Then I added a basic player health counter at the top, to display the players hitpoints.
To actually make the hitpoints change, I made a new script/component following the Single Responsibility Principle that would handle the players health.

Colliders were added to both the enemy prefab and the player character, and a trigger was created, when the enemy collided with the "player" tag, calling the "TakeDamage" method on the players health component:
```csharp
private void OnTriggerStay2D(Collider2D other)
        {
            if (Time.time < nextDamageTime) return;
            if (!other.CompareTag("Player")) return;

            
            if (other.TryGetComponent<PlayerHealth>(out var health))
            {
                health.TakeDamage(damage);
                nextDamageTime = Time.time + damageCooldown;
                movementState.SetMoveState(EnemyMoveStateEnum.Hit);
            }
        }
```

And the finished result of this milestone looks something like this:

![Milestone 1 showcase gif](BlogPost3Misc/Milestone1ShowcaseGif.gif)

A good starting point for the basic mechanic of the game: Enemies chasing you and you having to avoid them. 

## Next up
* Enemy spawners
* Enemy health implementation
* Projectile implementation (yay we get to fight back)
* Dynamic player attributes
* Run progression system, make the runs harder the longer you get
* Maybe a real background?