using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoidsSimulator
{
    internal static class Helper
    {
        public static float GetDeltaTime(GameTime gameTime)
        {
            return (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        public static float GetRotationAroundZero(Vector2 vector)
        {
            return (float)Math.Atan2(vector.Y, vector.X);
        }
        public static float DegToRad(float deg)
        {
            return deg * (float)Math.PI / 180f;
        }
    }
}
