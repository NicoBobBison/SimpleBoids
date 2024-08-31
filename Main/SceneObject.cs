using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BoidsSimulator.Main
{
    /// <summary>
    /// Parent class for Boid and Predatoid
    /// </summary>
    public abstract class SceneObject
    {
        public Vector2 Position;
        public int ID = 0;
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch spriteBatch);
    }
}
