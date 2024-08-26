using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace BoidsSimulator
{
    public class Game1 : Game
    {
        #region Constants
        public static readonly Vector2 ScreenSize = new Vector2(1600, 900);
        public static readonly Vector2 ScreenPadding = new Vector2(20, 20); // How far boids can travel outside of the screen size
        #endregion

        private Texture2D _boidTexture;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private List<Boid> _boids;
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
            _boids = new List<Boid>();
            SpawnBoids(30);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            foreach(Boid boid in _boids)
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
            foreach(Boid boid in _boids)
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
                _boids.Add(new Boid(_boidTexture, randPos, new Vector2(250, 0)));
            }
        }
    }
}
