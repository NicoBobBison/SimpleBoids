using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace BoidsSimulator
{
    public class SpatialPartioner
    {
        int[] _positionHash;
        public SceneObject[] DenseObjects;

        int _spacing;
        int _hashTableSize = 12497;

        /// <summary>
        /// Creates a new spatial partitioner based on spacing (side length of each partition)
        /// </summary>
        /// <param name="sceneObjects">List of all objects in scene</param>
        /// <param name="spacing">Length of each partition (squares)</param>
        /// <param name="hashTableSize">Size of hash table</param>
        public SpatialPartioner(int spacing, int hashTableSize)
        {
            _spacing = spacing;
            _hashTableSize = hashTableSize;
        }
        public void Update(List<SceneObject> objects)
        {
            DenseObjects = new SceneObject[objects.Count];
            // Add extra 1 storage as a guard in case last index is populated
            _positionHash = new int[_hashTableSize + 1];
            Insert(objects);
        }
        void Insert(List<SceneObject> objects)
        {
            _positionHash = new int[_hashTableSize + 1];
            foreach(SceneObject obj in objects)
            {
                // Hash each object and increment hash table
                int xi = (int)obj.Position.X / _spacing;
                int yi = (int)obj.Position.Y / _spacing;
                int i = Hash(new Vector2(xi, yi));

                _positionHash[i]++;
            }
/*            foreach (int i in _positionHash)
            {
                Debug.Write(i + " ");
            }
*/            int partialSumCount = 0;
            for(int i = 0; i < _hashTableSize + 1; i++)
            {
                partialSumCount += _positionHash[i];
                _positionHash[i] = partialSumCount;
            }
            foreach(SceneObject obj in objects)
            {
                int xi = (int)obj.Position.X / _spacing;
                int yi = (int)obj.Position.Y / _spacing;
                int i = Hash(new Vector2(xi, yi));

                int denseIndex = _positionHash[i];
                DenseObjects[denseIndex - 1] = obj;
                _positionHash[i]--;
            }
            
        }
        public List<SceneObject> QueryNearbyObjects(Vector2 pos, float range)
        {
            List<SceneObject> foundObjects = new List<SceneObject>();

            // Start at upper right of grid to find squares
            Vector2 initSquarePos = new Vector2(pos.X - range, pos.Y - range);
            Vector2 squarePos = initSquarePos;

            while(squarePos.Y < pos.Y + 2 * range)
            {
                while(squarePos.X < pos.X + 2 * range)
                {
                    // Hash the points in those squares to get the indeces to check for

                    int xi = (int)squarePos.X / _spacing;
                    int yi = (int)squarePos.Y / _spacing;
                    int i = Hash(new Vector2(xi, yi));

                    int numObjects = _positionHash[i+1] - _positionHash[i];
                    if (numObjects > 0)
                    {
                        // TODO: Figure out why this always finds waaaaaaaay more objects than it should
                        for(int denseIndex = _positionHash[i]; denseIndex < numObjects; denseIndex++)
                        {
                            foundObjects.Add(DenseObjects[denseIndex]);
                        }
                    }
                    squarePos.X += _spacing;
                }
                squarePos.X = initSquarePos.X;
                squarePos.Y += _spacing;
            }
            return foundObjects;
        }
        // Credit: Ten minute physics
        int Hash(Vector2 pos)
        {
            // Integers are arbitrary large prime numbers. Goal is to get a pseudo random hash based on position
            int hash = (int)Math.Pow((int)pos.X * 92837111, (int)pos.Y * 689287499);
            return Math.Abs(hash % _hashTableSize);
        }
    }
}
