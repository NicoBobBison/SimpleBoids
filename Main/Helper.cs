using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoidsSimulator.Main
{
    internal static class Helper
    {
        public static float GetDeltaTime(GameTime gameTime)
        {
            return (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        public static float GetRotationAroundZero(Vector2 vector)
        {
            float angle = (float)Math.Atan2(vector.Y, vector.X);
            // We want the rotation to always be a positive number
            if (angle >= 0)
            {
                return angle;
            }
            return 2 * (float)Math.PI + angle;
        }
        public static float DegToRad(float deg)
        {
            return deg * (float)Math.PI / 180f;
        }
        public static float GetMagnitude(Vector2 vector)
        {
            return Vector2.Distance(Vector2.Zero, vector);
        }
        public static Vector2 VectorBetweenPoints(Vector2 point1, Vector2 point2)
        {
            float x = point2.X - point1.X;
            float y = point2.Y - point1.Y;
            return new Vector2(x, y);
        }
        public static Vector2 InvertVector(Vector2 vector)
        {
            return new Vector2(-vector.X, -vector.Y);
        }
        public static Vector2 AverageOfVectors(Vector2 point1, Vector2 point2)
        {
            return new Vector2((point1.X + point2.X) / 2, (point1.Y + point2.Y) / 2);
        }
        public static Vector2 ClampVectorMagnitude(Vector2 vector, float minMagnitude, float maxMagnitude)
        {
            if (Vector2.Distance(Vector2.Zero, vector) > maxMagnitude)
            {
                vector.Normalize();
                return vector * maxMagnitude;
            }
            if (Vector2.Distance(Vector2.Zero, vector) < minMagnitude)
            {
                vector.Normalize();
                return vector * minMagnitude;
            }
            return vector;
        }
    }
}
