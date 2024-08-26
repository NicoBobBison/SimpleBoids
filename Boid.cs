﻿using Microsoft.Xna.Framework;
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
    internal class Boid
    {
        Vector2 _position;
        Vector2 _velocity;
        Texture2D _texture;
        public Boid(Texture2D texture, Vector2 position, Vector2 initialVelocity)
        {
            _texture = texture;
            _velocity = initialVelocity;
            _position = position;
        }
        public void Update(GameTime gameTime)
        {
            _position += _velocity * Helper.GetDeltaTime(gameTime);
            WrapAroundScreen();
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _position, null, Color.White, Helper.GetRotationAroundZero(_velocity) + Helper.DegToRad(90f), Vector2.Zero, 1.0f, SpriteEffects.None, 0);
        }
        /// <summary>
        /// Wraps boid around screen by teleporting it to the other side when it hits an edge. Uses padding to hide teleportation
        /// </summary>
        void WrapAroundScreen()
        {
            if(_position.X < -Game1.ScreenPadding.X)
            {
                _position.X = Game1.ScreenSize.X + Game1.ScreenPadding.X;
            }
            if(_position.Y < -Game1.ScreenPadding.Y)
            {
                _position.Y = Game1.ScreenSize.Y + Game1.ScreenPadding.Y;
            }
            if(_position.X > Game1.ScreenSize.X + Game1.ScreenPadding.X)
            {
                _position.X = -Game1.ScreenPadding.X;
            }
            if(_position.Y > Game1.ScreenSize.Y + Game1.ScreenPadding.Y)
            {
                _position.Y = -Game1.ScreenPadding.Y;
            }
        }
    }
}
