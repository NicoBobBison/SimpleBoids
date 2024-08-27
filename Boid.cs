using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoidsSimulator
{
    public class Boid : IEquatable<Boid>
    {
        #region Constants
        public const float BoidVisionRange = 100f;
        public const float BoidSeparationMultiplier = 0.003f;
        public const float BoidAlignmentMultiplier = 0.001f;
        public const float BoidCohesionMultiplier = 0.001f;

        public const float BoidMinSpeed = 0f;
        public const float BoidMaxSpeed = 300f;
        public const float BoidMaxAcceleration = 40f;
        #endregion

        public Vector2 Position;
        public Vector2 Velocity = Vector2.Zero;
        public bool DebugEnabled;

        Vector2 _acceleration = Vector2.Zero;
        Texture2D _texture;
        public Boid(Texture2D texture, Vector2 position, Vector2 initialVelocity)
        {
            _texture = texture;
            Velocity = initialVelocity;
            Position = position;
        }
        public void Update(GameTime gameTime)
        {
            Position += Velocity * Helper.GetDeltaTime(gameTime);
            Velocity += _acceleration;
            _acceleration = RecalculateAcceleration() * BoidMaxAcceleration;
            Velocity = Helper.ClampVectorMagnitude(Velocity, BoidMinSpeed, BoidMaxSpeed);
            WrapAroundScreen();
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            Color color = DebugEnabled ? Color.Red : Color.White;
            spriteBatch.Draw(_texture, Position, null, color, Helper.GetRotationAroundZero(Velocity) + Helper.DegToRad(90f), Vector2.Zero, 1.0f, SpriteEffects.None, 0);
        }
        // TODO: Use an accumulator to prioritize certain actions (steering away from collision) over others (cohesion)
        Vector2 RecalculateAcceleration()
        {
            List<Boid> nearbyBoids = GetBoidsWithinVisionRange();
            if(nearbyBoids.Count <= 1)
            {
                return Vector2.Zero;
            }
            Vector2 separationAcceleration = CalculateSeparationAcceleration(nearbyBoids) * BoidSeparationMultiplier;
            Vector2 alignmentAcceleration = CalculateAlignmentAcceleration(nearbyBoids) * BoidAlignmentMultiplier;
            Vector2 cohesionAcceleration = CalculateCohesionAcceleration(nearbyBoids) * BoidCohesionMultiplier;

            AcceleratorAccumulator accumulator = new AcceleratorAccumulator(BoidMaxAcceleration);
            accumulator.AddAccelerationRequest(separationAcceleration);
            accumulator.AddAccelerationRequest(alignmentAcceleration);
            accumulator.AddAccelerationRequest(cohesionAcceleration);

            return accumulator.Value;
        }
        Vector2 CalculateSeparationAcceleration(List<Boid> nearbyBoids)
        {
            // Take the inverse of the vectors from this boid to all nearby boids and average them
            Vector2 totalDistance = Vector2.Zero;
            foreach(Boid boid in nearbyBoids)
            {
                Vector2 distance = Helper.VectorBetweenPoints(Position, boid.Position);
                totalDistance += distance;
            }
            totalDistance /= nearbyBoids.Count;
            if(Vector2.Distance(Vector2.Zero, totalDistance) == 0)
            {
                return Vector2.Zero;
            }
            return Helper.InvertVector(totalDistance);
        }
        Vector2 CalculateAlignmentAcceleration(List<Boid> nearbyBoids)
        {
            Vector2 totalVel = Vector2.Zero;
            foreach(Boid boid in nearbyBoids)
            {
                totalVel += boid.Velocity;
            }
            totalVel /= nearbyBoids.Count;
            Vector2 differenceBetweenCurrentVel = Helper.VectorBetweenPoints(Velocity, totalVel);
            if (Vector2.Distance(Vector2.Zero, differenceBetweenCurrentVel) == 0)
            {
                return Vector2.Zero;
            }
            return differenceBetweenCurrentVel;

        }
        Vector2 CalculateCohesionAcceleration(List<Boid> nearbyBoids)
        {
            Vector2 totalPos = Vector2.Zero;
            foreach (Boid boid in nearbyBoids)
            {
                totalPos += boid.Position;
            }
            totalPos /= nearbyBoids.Count;
            Vector2 differenceBetweenCurrentPos = Helper.VectorBetweenPoints(Position, totalPos);
            if (Vector2.Distance(Vector2.Zero, differenceBetweenCurrentPos) == 0)
            {
                return Vector2.Zero;
            }
            return differenceBetweenCurrentPos;

        }
        /// <summary>
        /// Wraps boid around screen by teleporting it to the other side when it hits an edge. Uses padding to hide teleportation
        /// </summary>
        void WrapAroundScreen()
        {
            if(Position.X < -Game1.ScreenPadding.X)
            {
                Position.X = Game1.ScreenSize.X + Game1.ScreenPadding.X;
            }
            if(Position.Y < -Game1.ScreenPadding.Y)
            {
                Position.Y = Game1.ScreenSize.Y + Game1.ScreenPadding.Y;
            }
            if(Position.X > Game1.ScreenSize.X + Game1.ScreenPadding.X)
            {
                Position.X = -Game1.ScreenPadding.X;
            }
            if(Position.Y > Game1.ScreenSize.Y + Game1.ScreenPadding.Y)
            {
                Position.Y = -Game1.ScreenPadding.Y;
            }
        }
        List<Boid> GetBoidsWithinVisionRange()
        {
            List<Boid> foundBoids = new List<Boid>();
            foreach(Boid boid in Game1.AllBoids)
            {
                if(Vector2.Distance(Position, boid.Position) <= BoidVisionRange)
                    foundBoids.Add(boid);
            }
            return foundBoids;
        }
        public bool Equals(Boid other)
        {
            return Position == other.Position && Velocity == other.Velocity;
        }
    }
}
