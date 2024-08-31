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
        int _hashTableSize = 10000;

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
            int[] cellCount = new int[_hashTableSize + 1];
            foreach(SceneObject obj in objects)
            {
                // Hash each object and increment hash table
                int xi = (int)obj.Position.X / _spacing;
                int yi = (int)obj.Position.Y / _spacing;
                int i = Hash(new Vector2(xi, yi));

                cellCount[i]++;
            }
            int partialSumCount = 0;
            for(int i = 0; i < _hashTableSize + 1; i++)
            {
                partialSumCount += cellCount[i];
                cellCount[i] = partialSumCount;
            }
            foreach(SceneObject obj in objects)
            {
                int xi = (int)obj.Position.X / _spacing;
                int yi = (int)obj.Position.Y / _spacing;
                int i = Hash(new Vector2(xi, yi));

                int denseIndex = cellCount[i];
                DenseObjects[denseIndex - 1] = obj;
                cellCount[i]--;
            }
        }
        public List<SceneObject> QueryNearbyObjects(Vector2 pos, float range)
        {
            List<SceneObject> foundObjects = new List<SceneObject>();



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
