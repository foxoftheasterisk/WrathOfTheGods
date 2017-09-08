using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Screens;
using System.Collections.Generic;

namespace WrathOfTheGods
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class TopLevel : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        MapScreen mapScreen;

        public TopLevel()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 480;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            mapScreen = new MapScreen();
            ScreenManager.screenManager.Push(mapScreen);

            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.Pinch | GestureType.DoubleTap | GestureType.Hold;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            mapScreen.Map = Content.Load<Texture2D>("greece");
            mapScreen.CityTex = Content.Load<Texture2D>("basiccity");
            mapScreen.Path = Content.Load<Texture2D>("path");
            mapScreen.HeroTex = Content.Load<Texture2D>("achilles");

            mapScreen.SetScreenSize(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            XMLLibrary.SerializableList<XMLLibrary.CityData> cityData = Content.Load<XMLLibrary.SerializableList<XMLLibrary.CityData>>("cities");
            List<City> cities = new List<City>();

            foreach(XMLLibrary.CityData citydatum in cityData)
            {
                cities.Add(new City(citydatum));
            }

            mapScreen.SetCities(cities);


        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {


            //TODO: regulate timing probably
            List<InputItem> input = new List<InputItem>();

            while (TouchPanel.IsGestureAvailable)
            {
                input.Add(new GestureInput(TouchPanel.ReadGesture()));
            }

            //this makes it so the InputSet is only empty if the user is actually not touching the screen
            //(or ConsumeAll was called)
            TouchCollection touches = TouchPanel.GetState();
            if (touches.Count > 0)
                input.Add(new GestureInput(new GestureSample(GestureType.None, System.TimeSpan.Zero, touches[0].Position, new Vector2(), new Vector2(), new Vector2())));

            ScreenManager.screenManager.Update(new InputSet(input));

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            ScreenManager.screenManager.Draw(spriteBatch, SpriteSortMode.FrontToBack, SamplerState.PointWrap);

            base.Draw(gameTime);
        }
    }
}
