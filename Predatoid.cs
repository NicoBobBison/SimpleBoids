using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame;

namespace BoidsSimulator
{
    public class Predatoid : SceneObject, IEquatable<Predatoid>
    {
        #region Constants
        public const float PredatoidVisionRange = 250f;
        public const float PredatoidChaseMultiplier = 0.2f;
        public const float PredatoidSeparationRange = 100f;
        public const float PredatoidSeparationMultiplier = 0.2f;

        public const float PredatoidMinSpeed = 250f;
        public const float PredatoidMaxSpeed = 500f;
        public const float PredatoidMaxAcceleration = 60f;
        public const float PredatoidEdgeTurnSpeed = 100f;
        public const float PredatoidGravityAcceleration = 10f;

        readonly Color _color = new Color(227, 138, 146);
        #endregion

        public Vector2 Velocity = Vector2.Zero;

        #region Debug
        public bool VisionDebug;
        public bool SeparationDebug;
        public bool ChaseDebug;
        #endregion

        Vector2 _acceleration = Vector2.Zero;
        Texture2D _texture;
        public Predatoid(Texture2D texture, Vector2 position, Vector2 initialVelocity)
        {
            _texture = texture;
            Velocity = initialVelocity;
            Position = position;
        }
        public override void Update(GameTime gameTime)
        {
            _acceleration = RecalculateAcceleration() * PredatoidMaxAcceleration;
            Velocity = Helper.ClampVectorMagnitude(Velocity, PredatoidMinSpeed, PredatoidMaxSpeed);
            _acceleration.Y += PredatoidGravityAcceleration;
            KeepWithinBounds(gameTime);

            Velocity += _acceleration * Helper.GetDeltaTime(gameTime);
            Position += Velocity * Helper.GetDeltaTime(gameTime);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, Position, null, _color, Helper.GetRotationAroundZero(Velocity) + Helper.DegToRad(90f),
                new Vector2(_texture.Width / 2, _texture.Height / 2), 1.0f, SpriteEffects.None, 0);
            if (VisionDebug)
            {
                spriteBatch.DrawCircle(Position, PredatoidVisionRange, 20, Color.Green, 3);
            }
            if (SeparationDebug)
            {
                List<Predatoid> nearbyPredatoids = GetPredatoidsWithinVisionRange();
                spriteBatch.DrawLine(Position, Position + CalculateSeparationAcceleration(nearbyPredatoids), Color.White, 5);
                spriteBatch.DrawCircle(Position, PredatoidSeparationRange, 20, Color.Red, 3);
            }
        }
        Vector2 RecalculateAcceleration()
        {
            List<Boid> nearbyBoids = GetBoidsWithinVisionRange();
            List<Predatoid> nearbyPred = GetPredatoidsWithinVisionRange();
            if (nearbyBoids.Count == 0 && nearbyPred.Count == 0)
            {
                return Vector2.Zero;
            }

            Vector2 chaseAcceleration = CalculateChaseAcceleration(nearbyBoids) * PredatoidChaseMultiplier;
            Vector2 separationAcceleration = CalculateSeparationAcceleration(nearbyPred) * PredatoidSeparationMultiplier;

            AcceleratorAccumulator accumulator = new AcceleratorAccumulator(PredatoidMaxAcceleration);
            accumulator.AddAccelerationRequest(chaseAcceleration);
            accumulator.AddAccelerationRequest(separationAcceleration);

            return accumulator.Value;
        }
        Vector2 CalculateChaseAcceleration(List<Boid> nearbyBoids)
        {
            Vector2 totalPos = Vector2.Zero;
            int validBoidCount = 0;
            foreach (Boid boid in nearbyBoids)
            {
                totalPos += boid.Position;
                validBoidCount++;
            }
            if (validBoidCount == 0)
                return Vector2.Zero;
            totalPos /= validBoidCount;
            return Helper.VectorBetweenPoints(Position, totalPos);
        }
        Vector2 CalculateSeparationAcceleration(List<Predatoid> nearbyPred)
        {
            // Take the inverse of the vectors from this boid to all nearby boids and average them
            Vector2 totalDistance = Vector2.Zero;
            foreach (Predatoid p in nearbyPred)
            {
                // Skip boids we aren't going to collide with
                if (Vector2.Distance(Position, p.Position) > PredatoidSeparationRange)
                {
                    continue;
                }
                Vector2 distance = Helper.VectorBetweenPoints(Position, p.Position);
                totalDistance += distance;
            }
            return Helper.InvertVector(totalDistance);
        }

        void KeepWithinBounds(GameTime gameTime)
        {
            if (Position.X < Game1.ScreenMargin.X)
            {
                Velocity.X += PredatoidEdgeTurnSpeed;
            }
            else if (Position.X > Game1.ScreenSize.X - Game1.ScreenMargin.X)
            {
                Velocity.X -= PredatoidEdgeTurnSpeed;
            }
            if (Position.Y < Game1.ScreenMargin.Y)
            {
                Velocity.Y += PredatoidEdgeTurnSpeed;
            }
            else if (Position.Y > Game1.ScreenSize.Y - Game1.ScreenMargin.Y)
            {
                Velocity.Y -= PredatoidEdgeTurnSpeed;
            }
        }
        List<Predatoid> GetPredatoidsWithinVisionRange()
        {
            List<Predatoid> foundPredatoids = new List<Predatoid>();
            foreach (SceneObject obj in Game1.Space.QueryNearbyObjects(Position, PredatoidVisionRange))
            {
                if(obj is Predatoid)
                {
                    if (Vector2.Distance(Position, obj.Position) <= PredatoidVisionRange)
                    {
                        foundPredatoids.Add((Predatoid)obj);
                    }
                }
            }
            return foundPredatoids;
        }
        List<Boid> GetBoidsWithinVisionRange()
        {
            List<Boid> foundBoids = new List<Boid>();
            foreach (SceneObject obj in Game1.Space.QueryNearbyObjects(Position, PredatoidVisionRange))
            {
                if(obj is Boid)
                {
                    if (Vector2.Distance(Position, obj.Position) <= PredatoidVisionRange)
                    {
                        foundBoids.Add((Boid)obj);
                    }
                }
            }
            return foundBoids;
        }
        public bool Equals(Predatoid other)
        {
            return ID == other.ID;
        }
    }
}
