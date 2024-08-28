﻿using Microsoft.Xna.Framework;
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
        public const float BoidVisionRange = 130f;
        public const float BoidSeparationRange = 30f;
        public const float BoidSeparationMultiplier = 0.15f;
        public const float BoidAlignmentMultiplier = 0.15f;
        public const float BoidCohesionMultiplier = 0.0015f;

        public const float BoidMinSpeed = 450f;
        public const float BoidMaxSpeed = 600f;
        public const float BoidMaxAcceleration = 40f;
        public const float BoidEdgeTurnSpeed = 2500f;
        public const float BoidGravityAcceleration = 10f;
        #endregion

        public Vector2 Position;
        public Vector2 Velocity = Vector2.Zero;
        public int ID = 0;
        public static int IDCount = 0;

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
            _acceleration = RecalculateAcceleration() * BoidMaxAcceleration;
            Velocity = Helper.ClampVectorMagnitude(Velocity, BoidMinSpeed, BoidMaxSpeed);
            _acceleration.Y += BoidGravityAcceleration * Helper.GetDeltaTime(gameTime);
            KeepWithinBounds(gameTime);

            //WrapAroundScreen();
            Velocity += _acceleration * Helper.GetDeltaTime(gameTime);
            Position += Velocity * Helper.GetDeltaTime(gameTime);
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
                spriteBatch.DrawCircle(Position, BoidSeparationRange, 20, Color.Red, 3);
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
            foreach (Boid boid in nearbyBoids)
            {
                // Skip boids we aren't going to collide with
                if(Vector2.Distance(Position, boid.Position) > BoidSeparationRange)
                {
                    continue;
                }
                Vector2 distance = Helper.VectorBetweenPoints(Position, boid.Position);
                totalDistance += distance;
            }
            return Helper.InvertVector(totalDistance);
        }
        Vector2 CalculateAlignmentAcceleration(List<Boid> nearbyBoids)
        {
            Vector2 totalVel = Vector2.Zero;
            int validBoidCount = 0;
            foreach(Boid boid in nearbyBoids)
            {
                if(Vector2.Distance(Position, boid.Position) <= BoidSeparationRange)
                {
                    continue;
                }
                totalVel += boid.Velocity;
                validBoidCount++;
            }
            if (validBoidCount == 0)
                return Vector2.Zero;
            totalVel /= validBoidCount;
            return Helper.VectorBetweenPoints(Velocity, totalVel);
        }
        Vector2 CalculateCohesionAcceleration(List<Boid> nearbyBoids)
        {
            Vector2 totalPos = Vector2.Zero;
            int validBoidCount = 0;
            foreach (Boid boid in nearbyBoids)
            {
                if (Vector2.Distance(Position, boid.Position) <= BoidSeparationRange)
                {
                    continue;
                }
                totalPos += boid.Position;
                validBoidCount++;
            }
            if (validBoidCount == 0)
                return Vector2.Zero;
            totalPos /= validBoidCount;
            return Helper.VectorBetweenPoints(Position, totalPos);
        }
        /// <summary>
        /// Obsolete. Wraps boid around screen by teleporting it to the other side when it hits an edge. Uses padding to hide teleportation
        /// </summary>
        /*void WrapAroundScreen()
        {
            if(Position.X < -Game1.ScreenMargin.X)
            {
                Position.X = Game1.ScreenSize.X + Game1.ScreenMargin.X;
            }
            if(Position.Y < -Game1.ScreenMargin.Y)
            {
                Position.Y = Game1.ScreenSize.Y + Game1.ScreenMargin.Y;
            }
            if(Position.X > Game1.ScreenSize.X + Game1.ScreenMargin.X)
            {
                Position.X = -Game1.ScreenMargin.X;
            }
            if(Position.Y > Game1.ScreenSize.Y + Game1.ScreenMargin.Y)
            {
                Position.Y = -Game1.ScreenMargin.Y;
            }
        }*/
        void KeepWithinBounds(GameTime gameTime)
        {
            if (Position.X < Game1.ScreenMargin.X)
            {
                Velocity.X += BoidEdgeTurnSpeed * Helper.GetDeltaTime(gameTime);
            }
            else if (Position.X > Game1.ScreenSize.X - Game1.ScreenMargin.X)
            {
                Velocity.X -= BoidEdgeTurnSpeed * Helper.GetDeltaTime(gameTime);
            }
            if (Position.Y < Game1.ScreenMargin.Y)
            {
                Velocity.Y += BoidEdgeTurnSpeed * Helper.GetDeltaTime(gameTime);
            }
            else if (Position.Y > Game1.ScreenSize.Y - Game1.ScreenMargin.Y)
            {
                Velocity.Y -= BoidEdgeTurnSpeed * Helper.GetDeltaTime(gameTime);
            }
        }
        List<Boid> GetBoidsWithinVisionRange()
        {
            List<Boid> foundBoids = new List<Boid>();
            foreach(Boid boid in Game1.AllBoids)
            {
                if(Vector2.Distance(Position, boid.Position) <= BoidVisionRange && !Equals(boid))
                {
                    // TODO: Prevent boids from seeing other boids behind them?
                    foundBoids.Add(boid);
                }
            }
            return foundBoids;
        }
        public bool Equals(Boid other)
        {
            return ID == other.ID;
        }
    }
}
