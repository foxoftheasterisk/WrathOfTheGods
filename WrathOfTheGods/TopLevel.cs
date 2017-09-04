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
            ScreenManager.screenManager.push(mapScreen);

            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.DragComplete | GestureType.Pinch | GestureType.DoubleTap | GestureType.Hold;

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

            Texture2D greece;
            greece = Content.Load<Texture2D>("greece");
            mapScreen.Map = greece;
            mapScreen.SetScreenSize(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            mapScreen.CityTex = Content.Load<Texture2D>("basiccity");
            mapScreen.Path = Content.Load<Texture2D>("path");

            //TODO: fucking XML import instead of this nonsense
            List<City> cities = new List<City>();
            cities.Add(new City("Corinth", "Corinthia", new Vector2(586, 820), cities));
            cities.Add(new City("Megara", "Megaris", new Vector2(661, 785), cities));
            cities.Add(new City("Athens", "Attica", new Vector2(735, 803), cities));
            cities.Add(new City("Argos", "Argolis", new Vector2(563, 890), cities));
            cities.Add(new City("Megalopolis", "Arcadia", new Vector2(437, 895), cities));
            cities.Add(new City("Corinth", "Corinthia", new Vector2(533, 1017), cities));
            City temp = cities[0];
            temp.AddNeighbor(cities[1]);
            temp.AddNeighbor(cities[3]);
            temp.AddNeighbor(cities[4]);
            temp = cities[1];
            temp.AddNeighbor(cities[0]);
            temp.AddNeighbor(cities[2]);
            temp = cities[2];
            temp.AddNeighbor(cities[1]);
            temp = cities[3];
            temp.AddNeighbor(cities[0]);
            temp.AddNeighbor(cities[4]);
            temp.AddNeighbor(cities[5]);
            temp = cities[4];
            temp.AddNeighbor(cities[3]);
            temp.AddNeighbor(cities[0]);
            temp.AddNeighbor(cities[5]);
            temp = cities[5];
            temp.AddNeighbor(cities[4]);
            temp.AddNeighbor(cities[3]);

            mapScreen.Cities = cities;


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

            ScreenManager.screenManager.update();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            ScreenManager.screenManager.draw(spriteBatch, SpriteSortMode.FrontToBack, SamplerState.PointWrap);

            base.Draw(gameTime);
        }
    }
}
