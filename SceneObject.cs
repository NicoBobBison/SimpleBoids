﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace BoidsSimulator
{
    /// <summary>
    /// Parent class for Boid and Predatoid
    /// </summary>
    public abstract class SceneObject
    {
        public Vector2 Position;
        public int ID = 0;
    }
}
