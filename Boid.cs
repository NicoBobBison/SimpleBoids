using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BoidsSimulator
{
    public class Boid : SceneObject, IEquatable<Boid>
    {
        #region Constants
        public const float BoidVisionRange = 130f;
        public const float BoidFleeMultiplier = 1f;
        public const float BoidSeparationRange = 30f;
        public const float BoidSeparationMultiplier = 0.15f;
        public const float BoidAlignmentMultiplier = 0.15f;
        public const float BoidCohesionMultiplier = 0.0015f;

        public const float BoidMinSpeed = 450f;
        public const float BoidMaxSpeed = 600f;
        public const float BoidMaxAcceleration = 60f;
        public const float BoidEdgeTurnSpeed = 100f;
        public const float BoidGravityAcceleration = 10f;

        readonly Color _color = new Color(124, 129, 196);
        #endregion

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
        public override void Update(GameTime gameTime)
        {
            // TODO: Figure out when I actually have to multiply by delta time
            _acceleration = RecalculateAcceleration() * BoidMaxAcceleration;
            Velocity = Helper.ClampVectorMagnitude(Velocity, BoidMinSpeed, BoidMaxSpeed);
            _acceleration.Y += BoidGravityAcceleration;
            KeepWithinBounds(gameTime);

            //WrapAroundScreen();
            Velocity += _acceleration * Helper.GetDeltaTime(gameTime);
            Position += Velocity * Helper.GetDeltaTime(gameTime);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, Position, null, _color, Helper.GetRotationAroundZero(Velocity) + Helper.DegToRad(90f),
                new Vector2(_texture.Width / 2, _texture.Height / 2), 1.0f, SpriteEffects.None, 0);
            if(VisionDebug)
            {
                spriteBatch.DrawCircle(Position, BoidVisionRange, 20, Color.Green, 3);
            }
            if (SeparationDebug)
            {
                // Inefficient to call this three times, but it's only used for debugging which won't be in the final version
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
            List<Predatoid> nearbyPred = GetPredatoidsWithinVisionRange();
            if(nearbyBoids.Count == 0 && nearbyPred.Count == 0)
            {
                return Vector2.Zero;
            }
            Vector2 fleeAcceleration = CalculateFleeAcceleration(nearbyPred) * BoidFleeMultiplier;
            Vector2 separationAcceleration = CalculateSeparationAcceleration(nearbyBoids) * BoidSeparationMultiplier;
            Vector2 alignmentAcceleration = CalculateAlignmentAcceleration(nearbyBoids) * BoidAlignmentMultiplier;
            Vector2 cohesionAcceleration = CalculateCohesionAcceleration(nearbyBoids) * BoidCohesionMultiplier;

            AcceleratorAccumulator accumulator = new AcceleratorAccumulator(BoidMaxAcceleration);
            accumulator.AddAccelerationRequest(fleeAcceleration);
            accumulator.AddAccelerationRequest(separationAcceleration);
            accumulator.AddAccelerationRequest(alignmentAcceleration);
            accumulator.AddAccelerationRequest(cohesionAcceleration);

            return accumulator.Value;
        }
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
        Vector2 CalculateFleeAcceleration(List<Predatoid> nearbryPredatoids)
        {
            Vector2 totalDistance = Vector2.Zero;
            foreach (Predatoid pred in nearbryPredatoids)
            {
                // Skip boids we aren't going to collide with
                if (Vector2.Distance(Position, pred.Position) > BoidVisionRange)
                {
                    continue;
                }
                Vector2 distance = Helper.VectorBetweenPoints(Position, pred.Position);
                totalDistance += distance;
            }
            return Helper.InvertVector(totalDistance);

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
                Velocity.X += BoidEdgeTurnSpeed;
            }
            else if (Position.X > Game1.ScreenSize.X - Game1.ScreenMargin.X)
            {
                Velocity.X -= BoidEdgeTurnSpeed;
            }
            if (Position.Y < Game1.ScreenMargin.Y)
            {
                Velocity.Y += BoidEdgeTurnSpeed;
            }
            else if (Position.Y > Game1.ScreenSize.Y - Game1.ScreenMargin.Y)
            {
                Velocity.Y -= BoidEdgeTurnSpeed;
            }
        }
        List<Boid> GetBoidsWithinVisionRange()
        {
/*            if (VisionDebug)
            {
                Debug.WriteLine(Game1.Space.QueryNearbyObjects(Position, BoidVisionRange).Count);
            }
*/            List<Boid> foundBoids = new List<Boid>();
            foreach(SceneObject obj in Game1.Space.QueryNearbyObjects(Position, BoidVisionRange))
            {
                if (obj is Boid)
                {
                    if (Vector2.Distance(Position, obj.Position) <= BoidVisionRange && !Equals(obj))
                    {
                        foundBoids.Add((Boid)obj);
                    }
                }
            }
            return foundBoids;
        }
        List<Predatoid> GetPredatoidsWithinVisionRange()
        {
            List<Predatoid> foundPred = new List<Predatoid>();
            foreach (SceneObject obj in Game1.Space.QueryNearbyObjects(Position, BoidVisionRange))
            {
                if(obj is Predatoid)
                {
                    if (Vector2.Distance(Position, obj.Position) <= BoidVisionRange)
                    {
                        foundPred.Add((Predatoid)obj);
                    }
                }
            }
            return foundPred;
        }
        public bool Equals(Boid other)
        {
            return ID == other.ID;
        }
    }
}
