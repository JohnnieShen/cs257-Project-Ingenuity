# Mission Ingenuity

- **Jiaming Shen**, Github ID: [JohnnieShen](https://github.com/JohnnieShen)
- **Liam Baca**, Github ID: [Racecar13](https://github.com/Racecar13)
- **Jay Kim**, Github ID: [jaykim2022](https://github.com/jaykim2022)

## Game Summary

You are a command module of a defunct rover and you found yourself powering on in the middle of nowhere on an alien planet with no parts attached to you. You need to scavenge for parts scattered around and upgrade your rover, fight hostile enemies of various kinds and build a rocket that will get you out of here to continue your quest for exploration.

## Genres

This game will be a TPS game with characteristics taken from rogue-lite games. It will also feature a handcrafted openworld that allows the player to explore freely, but the overall game structure will still be linear.

## Inspiration games

**TerraTech**

![TerraTech](Images/terratech.png)

"TerraTech is an open-world, sandbox adventure game, where you design and build your own creations through a mix of crafting, combat and discovery. Design vehicles from a huge library of blocks. Scavenge, craft and buy new parts to survive and become the ultimate planetary prospector."

We plan to take inspiration from the general gameplay loop of TerraTech, where the player is able to explore an open-ish world and scavenge for new building blocks of their vehicle. However, we plan on having “levels” instead of a big open world so that the project is going to scale a bit better and we could add more mechanics and elements into different levels. We also plan on adding more engaging gameplay other than exploration, for example roguelike mechanics.

**Slay the Spire**

![Slay the Spire](Images/slaythespire.png)

“Slay the Spire is a game in which you climb The Spire, ascending its floors through three acts, encountering many enemies, bosses, and events along the way. The paths through each act all lead to a final floor where a challenging boss encounter awaits. The content of each floor and the available paths through each act are procedurally-generated, so each run will be a different experience.”

We aim to incorporate the roguelite deck-building aspect of Slay the Spire into our game. Rather than having “cards” that players can pick up, our game will offer different parts, weapons, and general customization to the player’s car which can enhance the current build. An overarching goal would be to develop a diverse skill tree in which players can build up their vehicles depending on an archetype they want to follow, with some added randomization. 

## Gameplay

Players will explore an alien planet from a third-person perspective, controlling a modular rover that evolves as they collect new parts. Interaction and game loop will be driven by scavenging, combat, and various upgrades, encouraging experimentation with different builds. 

A customization inventory system will allow players to part swap on the fly, which rewards creativity, while an in-game map provides navigation assistance. 

Control-wise, the game will feature control schemes similar to other arcade-style vehicle driving/third-person shooter games such as *Robocraft*.

![Robocraft](Images/robocraft.png)

## Development Plan

### Project Checkpoint 1-2: Basic Mechanics and Scripting (Ch 5-9)

~~Sketch out a rough idea of what parts of your game you will implement for the next submission, Project Checkpoint 1-2: Basic Mechanics and Scripting involving Unity textbook Chapters 5 through 9. You will come back to update this for each submission based on which things you've accomplished and which need to be prioritized next. This will help you practice thinking ahead as well as reflecting on the progress you've made throughout the semester.~~

~~Our initial goal will be to implement basic car and tire physics mechanics into our game. One approach to this is treating cars as a single rigid body attached to 4 additional rigid bodies which represent the wheels. The forces will be acting on the wheels, and consist of three major components:~~

1. ~~**Suspension**~~ 
   - ~~Based on classical spring mechanics, with an added damping force to account for an infinite oscillation.~~
   - ~~A basic calculation would be:~~
     ```math
     Force = (Offset × Strength) - (Velocity × Damping)
     ```

2. ~~**Acceleration**~~
   - ~~Depends on the amount of applied torque.~~
   - ~~Depending on the current direction of steering, different acceleration limits will be hit.~~
   - ~~A basic implementation is to set a top speed value, and then based on the applied torque, follow a semi-Gaussian distribution for situational acceleration limits.~~

3. ~~**Steering**~~
   - ~~Two different components to velocity:~~
     - ~~The direction the tires are facing.~~
     - ~~The direction perpendicular to that.~~
   - ~~Steering arises from that perpendicular direction, and in order to provide a sense of traction/steering, a counteracting opposite velocity is needed.~~  
   - ~~Depending on the magnitude of this opposite velocity, the amount of sliding will be adjusted.~~

~~A higher-reaching goal will be to work on a basic implementation for the car-building mechanic.~~

~~Possibly adding in some “parts” that can be added onto the car and a simple UI selection screen for those parts.~~

### Additions

There were three main areas we completed that we did not plan on:

1. 3D models
    - Johnny made blender models for each of our parts (hull, command module, turret, wheel)
    - We still do not have textures and are just using basic colors which we hope to improve
2. Turrets
    - We implemented the turret piece which attaches to a block. The turret armature is fully articulated and animated to point towards the player’s cursor. It currently shoots rigidbody bullets before having to reload.
3. Mode switching
    - We implemented build mode and drive mode as well as different camera controls and transitions for each.

### Project Part 2: 3D Scenes and Models (Ch 3+4, 10)

For the next submission, we would like to improve on our 3D models by adding textures. This will allow us to make blocks with different strengths or properties in addition to visual differences. We would also want to create more models for the blocks e.g. different cannon designs for different types, shield generator, armor blocks etc. We would also want to create assets for props on the terrain and a better terrain in general. 

We would also want to improve the shooting aspect of the game, for the next milestone, a conservative estimate would be to have a stationary enemy that the player can shoot at and eventually destroy, and a more optimistic estimate would be a primitive enemy AI that would allow some interaction. We would also want to improve the depth of the gunplay by e.g. introducing more damage types (e.g. energy vs ballistic) and an energy shield. 

Game element wise we also want to incorporate a looting game loop where parts will be scattered around and the player has to use a certain block to “vacuum” it up and put it on in the building mode.

We would also like to work on world generation and 3D models for the environment the player drives around in. Jay has worked on a primitive version of procedural generation of the terrain and we would want to keep working on it and incorporate this into the game. This is tentative as we are unsure whether it would work well performance wise.

To summarize:

- More 3D models, texture and improve terrain
- Incorporate the actual asset for the wheel and replace current placeholder asset
- Enemy target (Tentative: primitive enemy AI)
- Better, more in depth gunplay and shooting mechanics
- Looting
- Tentative: Procedural generation of terrain

## Development

### Project Checkpoint 1-2

#### Initial Vehicle Building Systems and Physics

Going into this checkpoint, based on our feedback, our main goals were to implement a rudimentary vehicle building system, focusing on core mechanics and systems over polishing. We started out with a primitive block-building system utilizing proxy geometry in place of more-developed assets, inspired by the mechanics of established games such as Minecraft. As per our initial plan, we worked on establishing a more stylized physics system to give the game a more satisfying feel.

![Initial block building system utilizing primitive shapes and physics.](Images/primitivephysics.png)

#### Assets

A couple assets were created using the 3D Blender Engine, to add some more detail to our mechanics.

![Base connecting block asset modeled in Blender](Images/blenderhull.png)

![Car wheel asset modeled in Blender](Images/blenderwheel.png)

#### Further Development of Vehicle Building and Additional Features

At this point in time, we had a simplified version of input control implemented, where the player assumed a free-moving perspective controlled by WASD, and the car was controlled using Q/E to steer and the car went forward automatically. (This is a description of the design and implementation process, we have improved the controls from here).

Later, we implemented a more polished version of the initial system, by adding color-indicated blocks for the various parts of the car, as well as imported some additional assets, establishing a centralized control panel block for the player to build off of. At this point, we also utilized Unity’s new input system to integrate a more user-friendly input experience. We also began working on developing additional features such as turrets for eventually when an established gameplay loop is developed. So far, we have implemented drive mode and build mode along with four components to experiment with: hull, drive wheels, turn wheels and turrets.

![Polished block-building system with color indicators](Images/buildsystem.png)

![Example user constructed car with advanced physics](Images/car.png)

In build mode, the player can move in the xy-plane using WASD or the arrow keys. The player can also hover up or down using space for up and shift for down. These are similar to the Minecraft creative mode controls.

Additionally, the player can left click to place the currently selected block and right click to delete. The selected block type is displayed in a textbox in the top left of the screen. Currently the white text does not show up well on light backgrounds, so we hope to make it more visible in the next update. The player can cycle through available block types using the scroll wheel.

The four block types available in this update are:

- Hull - This is the most basic block and simply acts as a structural piece to build out your vehicle. It is red.
- Turn wheel - This block is a special variant of the hull block which has a wheel prefab attached to it. This wheel is configured to respond to the player's input during drive mode to turn left and right. It is green.
- Drive wheel - This is very similar to the turn wheel but the wheel prefab is configured to respond to forward and backwards input. It is blue to differentiate it from the drive wheel.
- Turret - This block only has one connection point on the bottom. Turrets can still be placed on the top, bottom or side of blocks. It fires when the player fires unless the line of sight is blocked by another component.

In drive mode, the player can accelerate either forwards or backwards using W/S. The player can turn any turn wheels using A/D. Additionally, the player can fire using left click.

Building was implemented by using a raycast from the camera until a block is hit. Then, the normal of the collision is computed and a new block is instantiated at an integer offset from that location. Additionally, a fixed joint component is added between the new and old block. During build mode, all blocks are marked as kinematic. However, the player can press “b” to toggle build mode and transition into drive mode. When this occurs, the block manager iterates through each block and enables the physics causing the car to fall. When build mode is entered again, the physics are turned off and the car levitates back up to a build height allowing the player to access all angles of the vehicle.

![Turret asset modeled in Blender](Images/blenderturret.png)

![Turret tracking system demonstration](Images/turrettracking.png)

A first iteration model for the turret is modelled up and rigged in Blender. Model is imported to Unity to use with the Animation Rigging package to allow Unity to use inverse kinetics to calculate how the turret should behave when the aiming location changes. In this version, the turret follows a target location decided by a raycast from the camera. If there are obstacles along the way, e.g. other blocks, the turrets will emit a line rendered by a line renderer as a visual cue that it is blocked and won’t shoot. 

An early-stage procedural world generation was also created utilizing an icosahedron-mesh that has size and sub-division flexibility. A new physics system was developed to allow for planet-centric gravity rather than Unity’s default Rigidbody logic.

![Base planet](Images/world1.png)

![First procedurally generated through the use of randomized extrusion and indentation of different regions](Images/world2.png)

![Second procedurally generated through the use of randomized extrusion and indentation of different regions](Images/world3.png)