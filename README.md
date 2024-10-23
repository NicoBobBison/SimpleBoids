# Simple Boids by NicoBobBison

## Description
Simple Boids is a minimalistic game that simulates the flocking behavior of birds using the algorithm described in Craig Reynolds' 1987 paper "Flocks, Herds, and Schools: A Distributed Behavioral Model". By adjusting parameters like alignment, cohesion, and separation (configurable in Boid.cs and Predatoid.cs), users can observe and alter how these bird-like objects collectively exhibit realistic flocking dynamics.

When the scene starts, a specified number of boids (configurable in <code>Game1.cs</code>) spawn at random locations throughout the screen with random velocities. Over time, the boids begin to follow each other and form flocks (cohesion) while trying to maintain a bubble of space around them as to not crowd each other (separation). Also, each boid attempts to steer in the direction of other nearby boids within their vision range (alignment).

Each scene also spawns predators (1 by default, configurable in <code>Game1.cs</code>) that chase nearby boids in their vision range. Boids attempt to flee from nearby "predatoids".

To manage the high number of vision checks, Simple Boids uses a spatial partitioner that divides the scene into squares with side length equal to the vision range of a boid. This drastically reduces the number of checks necessary to get nearby boids, allowing for more boids to be simulated at once. While partitioning could be used to store predatoids as well, the program will usually brute force vision checks to find nearby predatoids (unless there are a large amount of them in the scene, configurable in Game1.cs) since, by default, the scene has only 1.


## Getting started
### Prerequisites
Install the [.NET framework](https://dotnet.microsoft.com/en-us/download).

### Installation
1. Clone the repository and navigate into its directory.

   ```sh
   cd SimpleBoids
   ```
2. Build the GUI version:

   ```sh
   dotnet build SimpleBoids -o [OUTPUT DIRECTORY]
   ```
   
3. Run the .exe file.
   
   ```sh
   start SimpleBoids.exe
   ```


### Controls
R - Restart simulation

ESC - Close program

## Acknowledgements:
Rendering windows and sprites: Monogame 3.8.1 (https://github.com/MonoGame/MonoGame)

Drawing primitives (for debugging): https://github.com/DoogeJ/MonoGame.Primitives2D

Boids (& Predatoids):
 - [Flocks, Herds, and Schools: A Distributed Behavioral Model by Craig W. Reynolds](https://dl.acm.org/doi/pdf/10.1145/37402.37406)
 - [Boids Wikipedia page](https://en.wikipedia.org/wiki/Boids)
 - [Helpful tips for dealing with screen boundaries] (https://people.ece.cornell.edu/land/courses/ece4760/labs/s2021/Boids/Boids.html#:~:text=Boids%20is%20an%20artificial%20life,very%20simple%20set%20of%20rules).

Spatial partitioning:
 - [Ten Minute Physics: 11 - Finding collisions among thousands of objects blazing fast](https://www.youtube.com/watch?v=D2M8jTtKi44)
 - [Spatial hash map logic](https://carmencincotti.com/2022-10-31/spatial-hash-maps-part-one/)
