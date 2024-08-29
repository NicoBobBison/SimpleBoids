﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BoidsSimulator
{
    public class Game1 : Game
    {
        #region Constants
        public static readonly Vector2 ScreenSize = new Vector2(1600, 900);
        public static readonly Vector2 ScreenMargin = new Vector2(125, 125); // How close boids can get to the screen edge before being pushed inwards
        public static readonly Color BackgroundColor = new Color(50, 53, 89);

        public const int NumberOfBoids = 200;
        public const int NumberOfPredatoids = 2;
        #endregion

        // If true, will choose a random boid to debug
        bool _debugBoid = false;
        bool _debugPredatoid = false;

        private Texture2D _boidTexture;
        private Texture2D _predatoidTexture;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public static readonly List<Boid> AllBoids = new List<Boid>();
        public static readonly List<Predatoid> AllPredatoids = new List<Predatoid>();

        private KeyboardState _currentState;
        private KeyboardState _previousState;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferWidth = (int)ScreenSize.X;
            _graphics.PreferredBackBufferHeight = (int)ScreenSize.Y;
            _graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            _boidTexture = Content.Load<Texture2D>("Sprites/boid");
            _predatoidTexture = Content.Load<Texture2D>("Sprites/predatoid");

            RestartSimulation();

            _currentState = Keyboard.GetState();
            _previousState = _currentState;
        }

        protected override void Update(GameTime gameTime)
        {
            _previousState = _currentState;
            _currentState = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            foreach(Boid boid in AllBoids)
            {
                boid.Update(gameTime);
            }
            foreach(Predatoid predatoid in AllPredatoids)
            {
                predatoid.Update(gameTime);
            }
            if(_currentState.IsKeyDown(Keys.R) && _previousState.IsKeyUp(Keys.R))
            {
                // Restart simulation
                RestartSimulation();
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(BackgroundColor);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();
            foreach(Boid boid in AllBoids)
            {
                boid.Draw(_spriteBatch);
            }
            foreach(Predatoid predatoid in AllPredatoids)
            {
                predatoid.Draw(_spriteBatch);
            }
            _spriteBatch.End();
            base.Draw(gameTime);
        }
        void RestartSimulation()
        {
            AllBoids.Clear();
            Boid.IDCount = 0;
            SpawnBoids(NumberOfBoids);
            SpawnPredatoids(NumberOfPredatoids);

            if (_debugBoid)
            {
                Boid debuggedBoid = GetRandomBoid();
                debuggedBoid.VisionDebug = true;
                debuggedBoid.SeparationDebug = true;
                debuggedBoid.AlignmentDebug = true;
                debuggedBoid.CohesionDebug = true;
            }
            if (_debugPredatoid)
            {
                Predatoid debuggedPredatoid = GetRandomPredatoid();
                debuggedPredatoid.VisionDebug = true;
                debuggedPredatoid.SeparationDebug = true;
                debuggedPredatoid.ChaseDebug = true;
            }
        }
        void SpawnBoids(int numBoids)
        {
            Random random = new Random();
            for(int i = 0; i < numBoids; i++)
            {
                Vector2 randPos = new Vector2(random.Next(0, (int)ScreenSize.X), random.Next(0, (int)ScreenSize.Y));
                Vector2 randVel = new Vector2(random.Next((int)-Boid.BoidMaxSpeed, (int)Boid.BoidMaxSpeed), random.Next((int)-Boid.BoidMaxSpeed, (int)Boid.BoidMaxSpeed));
                Boid b = new Boid(_boidTexture, randPos, randVel);
                b.ID = Boid.IDCount;
                AllBoids.Add(b);
                Boid.IDCount++;
            }
        }
        void SpawnPredatoids(int numPredatoids)
        {
            Random random = new Random();
            for (int i = 0; i < numPredatoids; i++)
            {
                Vector2 randPos = new Vector2(random.Next(0, (int)ScreenSize.X), random.Next(0, (int)ScreenSize.Y));
                Vector2 randVel = new Vector2(random.Next((int)-Predatoid.PredatoidMaxSpeed, (int)Predatoid.PredatoidMaxSpeed),
                                              random.Next((int)-Predatoid.PredatoidMaxSpeed, (int)Predatoid.PredatoidMaxSpeed));
                Predatoid p = new Predatoid(_predatoidTexture, randPos, randVel);
                p.ID = Predatoid.IDCount;
                AllPredatoids.Add(p);
                Predatoid.IDCount++;
            }
        }
        Boid GetRandomBoid()
        {
            Random random = new Random();
            return AllBoids[random.Next(0, AllBoids.Count)];
        }
        Predatoid GetRandomPredatoid()
        {
            Random random = new Random();
            return AllPredatoids[random.Next(0, AllPredatoids.Count)];
        }
    }
}
