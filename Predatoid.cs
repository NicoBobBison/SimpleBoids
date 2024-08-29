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
    public class Predatoid
    {
        #region Constants
        public const float PredatoidVisionRange = 130f;
        public const float PredatoidChaseMultiplier = 0.5f;
        public const float PredatoidSeparationRange = 30f;
        public const float PredatoidSeparationMultiplier = 0.15f;

        public const float PredatoidMinSpeed = 150f;
        public const float PredatoidMaxSpeed = 300f;
        public const float PredatoidMaxAcceleration = 60f;
        public const float PredatoidEdgeTurnSpeed = 80f;
        public const float PredatoidGravityAcceleration = 10f;
        #endregion

        public Vector2 Position;
        public Vector2 Velocity = Vector2.Zero;
        public int ID = 0;
        public static int IDCount = 0;

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
        public void Update(GameTime gameTime)
        {
            _acceleration = RecalculateAcceleration() * PredatoidMaxAcceleration;
            Velocity = Helper.ClampVectorMagnitude(Velocity, PredatoidMinSpeed, PredatoidMaxSpeed);
            _acceleration.Y += PredatoidGravityAcceleration;
            KeepWithinBounds(gameTime);

            Velocity += _acceleration * Helper.GetDeltaTime(gameTime);
            Position += Velocity * Helper.GetDeltaTime(gameTime);
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            Color color = VisionDebug ? Color.Red : Color.White;
            spriteBatch.Draw(_texture, Position, null, color, Helper.GetRotationAroundZero(Velocity) + Helper.DegToRad(90f),
                new Vector2(_texture.Width / 2, _texture.Height / 2), 1.0f, SpriteEffects.None, 0);
            if (VisionDebug)
            {
                spriteBatch.DrawCircle(Position, PredatoidVisionRange, 20, Color.Green, 3);
            }
            if (SeparationDebug)
            {
                List<Predatoid> nearbyPredatoids = GetPredatoidsWithinVisionRange();
                //spriteBatch.DrawLine(Position, Position + CalculateSeparationAcceleration(nearbyBoids), Color.White, 5);
                spriteBatch.DrawCircle(Position, PredatoidSeparationRange, 20, Color.Red, 3);
            }
        }
        Vector2 RecalculateAcceleration()
        {
            return Vector2.Zero;
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
            foreach(Predatoid p in Game1.AllPredatoids)
            {
                if (Vector2.Distance(Position, p.Position) <= PredatoidVisionRange)
                {
                    foundPredatoids.Add(p);
                }
            }
            return foundPredatoids;
        }
        List<Boid> GetBoidsWithinVisionRange()
        {
            List<Boid> foundBoids = new List<Boid>();
            foreach (Boid boid in Game1.AllBoids)
            {
                if (Vector2.Distance(Position, boid.Position) <= PredatoidVisionRange)
                {
                    foundBoids.Add(boid);
                }
            }
            return foundBoids;
        }
    }
}
