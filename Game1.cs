using Microsoft.Xna.Framework;
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
        public static readonly Vector2 ScreenPadding = new Vector2(30, 30); // How far boids can travel outside of the screen size

        public const float BoidVisionRange = 100f;
        public const float BoidSeparationMultiplier = 0.3f;
        public const float BoidAlignmentMultiplier = 0.3f;
        public const float BoidCohesionMultiplier = 0.3f;

        public const float BoidMinSpeed = 10f;
        public const float BoidMaxSpeed = 300f;
        public const float BoidMaxAcceleration = 40f;
        #endregion

        private Texture2D _boidTexture;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        public static readonly List<Boid> AllBoids = new List<Boid>();
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
            SpawnBoids(70);
            GetRandomBoid().DebugEnabled = true;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            foreach(Boid boid in AllBoids)
            {
                boid.Update(gameTime);
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();
            foreach(Boid boid in AllBoids)
            {
                boid.Draw(_spriteBatch);
            }
            _spriteBatch.End();
            base.Draw(gameTime);
        }
        void SpawnBoids(int numBoids)
        {
            Random random = new Random();
            for(int i = 0; i < numBoids; i++)
            {
                Vector2 randPos = new Vector2(random.Next(0, (int)ScreenSize.X), random.Next(0, (int)ScreenSize.Y));
                Vector2 randVel = new Vector2(random.Next((int)BoidMaxSpeed / 4, (int)BoidMaxSpeed), random.Next((int)BoidMaxSpeed / 4, (int)BoidMaxSpeed));
                AllBoids.Add(new Boid(_boidTexture, randPos, randVel));
            }
        }
        Boid GetRandomBoid()
        {
            Random random = new Random();
            return AllBoids[random.Next(0, AllBoids.Count)];
        }
    }
}
