# Intro
My first experience with the Unity game engine, consisted of following the engine's own "RollABall" tutorial, which is a compact introduction to the full game development loop offered by the engine.

The tutorial has you installing Unity and setting up a project as the first few steps.

# Core features
After setting up my project, I was introduced to GameObject, which is Unity's way of defining all objects present in the scene.
The following steps entailed manipulating these GameObjects, by changing their Transform properties (altering scale, rotation and position).

Next up was making the player character, which is also just another GameObject, but with added Rigidbody and Collider components, allowing for Unity's built-in physics engine to simulate collisions and forces upon the object. A C# script was also made to allow for the player to control the character, which was easy, as Unity integrates very cleanly with Rider, allowing for a quick feedback loop.

Along the way other core subjects were also introduced: Camera controlling, Prefabs (A big boost to maintainability and scalability), Collission detection and triggering, AI Navigation via NavMesh, Basic UI and at the end: Deployment of the project to both web and .exe formats allowing the game to be played locally on Windows and on Unity's website.

# Short comparison to Unreal Engine 5
Before the semester started, I dabbled a bit in gamemaking in Unreal Engine 5, which has helped me a bit in understanding the different components and tools also in Unity. But I must say that the learning curve for Unity seems alot less step, when compared to Unreal Engine 5. This could be for several reasons, probably a mix of them all: 
* My project in UE5 was way to ambitious
* UE5 does scripting in C++, a language I had never worked with before, compared to C# which I'm very familiar with.
* UE5 has more applications than Unity, at least to my knowledge, which makes it harder to learn.

# Summation
I have now learned the basics of game development in Unity, via the RollABall tutorial. This will help me develop my own game, which will be explained in detail in the next Blog post.
