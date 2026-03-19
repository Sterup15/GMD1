# Game Design Document
## Overview
### Project: Bullet Hell (Name/theme to come)
### Genre: Bullet hell / roguelite hybrid
### Platform: Via's windows based arcade machine

## Intro to the game concept
The bullet hell genre (also called shoot 'em up), is shortly defined as one core idea: The screen is flooded with projectiles, which the player must avoid to win the game.
Introducing the roguelite genre into this, gives players the opportunity of doing several "runs", each starting from scratch, to then rapidly increase in challenge and intensity, pleasing the Aesthetics:
* Sensation: Avoiding/firing a barrage of projectiles across the screen. Who doesn't love the dopamine hit of a multikill?
* Challenge: With runs being short, the intensity and challenge must rise fast to satisfy the attention spans of our TikTok brains.
* Mastery: Learning to effectively combat the different enemy types, and trying out the different upgrade combos to see what works well together.

## Core loop
* Short loop: run around the map, killing enemies for points/gold
* Mid loop: Survive a run until the end, upgrading ones character along the way
* Long loop: Complete runs, earning rewards that persist between runs (cosmetics, QoL upgrades and so on) 

## Interactivity
The player controls the player characters movements, and what upgrades to choose when prompted. Otherwise the targeting and aiming of the character is done automatically, targeting the closest enemy.
# Milestones:
## #1
* Early concept
  * Game Design Document
  * Genres and basic game concept
  * Milestones (Ooh very meta)
* Setup the basics
  * Unity project setup
  * GitHub repo setup
  * Moving player character
  * Enemy prefab targeting the player
  * Basic UI

## #2
* Core game concept systems
  * Enemy spawner implementation
  * Projectile implementation
  * Player attributes implementations
    * Damage
    * Move speed
    * Attack speed
    * Health
  * Run progression system
    * Enemies spawn faster
    * More projectiles??

## #3
* Player upgrade system
  * Upgrade player attributes
  * Unique upgrade: Player projectiles bounces of the walls.
* Win condition
  * Kill a number of enemies within a timeframe
  * Optionally: Boss fight after set timeframe, fight to the death
* If time allows: Polishing and tuning, making the game more balanced


## Future work/roadmap
* Persistent upgrades in between runs
* More player characters
* More unique upgrades
* More enemies
* More bosses