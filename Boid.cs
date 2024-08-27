using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame;
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
        public const float BoidSeparationMultiplier = 0.4f;
        public const float BoidAlignmentMultiplier = 0.1f;
        public const float BoidCohesionMultiplier = 0.02f;

        public const float BoidMinSpeed = 100f;
        public const float BoidMaxSpeed = 300;
        public const float BoidMaxAcceleration = 30f;
        #endregion

        public Vector2 Position;
        public Vector2 Velocity = Vector2.Zero;

        #region Debug
        public bool VisionDebug;
        public bool SeparationDebug;
        public bool AlignmentDebug;
        public bool CohesionDebug;
        #endregion

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
            Velocity += _acceleration * Helper.GetDeltaTime(gameTime);
            _acceleration = RecalculateAcceleration() * BoidMaxAcceleration;
            Velocity = Helper.ClampVectorMagnitude(Velocity, BoidMinSpeed, BoidMaxSpeed);
            WrapAroundScreen();
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            Color color = VisionDebug ? Color.Red : Color.White;
            spriteBatch.Draw(_texture, Position, null, color, Helper.GetRotationAroundZero(Velocity) + Helper.DegToRad(90f),
                new Vector2(_texture.Width / 2, _texture.Height / 2), 1.0f, SpriteEffects.None, 0);
            if(VisionDebug)
            {
                spriteBatch.DrawCircle(Position, BoidVisionRange, 20, Color.Green, 3);
            }
            if (SeparationDebug)
            {
                List<Boid> nearbyBoids = GetBoidsWithinVisionRange();
                spriteBatch.DrawLine(Position, Position + CalculateSeparationAcceleration(nearbyBoids), Color.White, 5);
            }
            if (AlignmentDebug)
            {
                List<Boid> nearbyBoids = GetBoidsWithinVisionRange();
                spriteBatch.DrawLine(Position, Position + CalculateAlignmentAcceleration(nearbyBoids), Color.Red, 5);
            }
            if (CohesionDebug)
            {
                List<Boid> nearbyBoids = GetBoidsWithinVisionRange();
                spriteBatch.DrawLine(Position, Position + CalculateCohesionAcceleration(nearbyBoids), Color.Green, 5);
            }

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
        // TODO: Figure out why this isn't giving a very strong velocity
        Vector2 CalculateSeparationAcceleration(List<Boid> nearbyBoids)
        {
            // Take the inverse of the vectors from this boid to all nearby boids and average them
            Vector2 totalDistance = Vector2.Zero;
            foreach(Boid boid in nearbyBoids)
            {
                Vector2 distance = Helper.VectorBetweenPoints(Position, boid.Position);
                float magOfDistance = Helper.GetMagnitude(distance);
                float weight = 1 - (magOfDistance / BoidVisionRange);
                totalDistance += distance * weight;
            }
            totalDistance /= nearbyBoids.Count;
            return Helper.InvertVector(totalDistance);
        }
        Vector2 CalculateAlignmentAcceleration(List<Boid> nearbyBoids)
        {
            float totalRotation = 0;
            foreach(Boid boid in nearbyBoids)
            {
                totalRotation += Helper.GetRotationAroundZero(boid.Velocity);
            }
            totalRotation /= nearbyBoids.Count;
            Vector2 desiredDirection = new Vector2((float)Math.Cos(totalRotation), (float)Math.Sin(totalRotation));
            desiredDirection *= Helper.GetMagnitude(Velocity);
            return Helper.VectorBetweenPoints(Velocity, desiredDirection);
        }
        Vector2 CalculateCohesionAcceleration(List<Boid> nearbyBoids)
        {
            Vector2 totalPos = Vector2.Zero;
            foreach (Boid boid in nearbyBoids)
            {
                totalPos += boid.Position;
            }

            totalPos /= nearbyBoids.Count;
            return Helper.VectorBetweenPoints(Position, totalPos);
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
                if(Vector2.Distance(Position, boid.Position) <= BoidVisionRange && !Equals(boid))
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
