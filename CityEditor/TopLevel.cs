using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using XMLL = WrathOfTheGods.XMLLibrary;
using Screens;

namespace EditorSuite
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class TopLevel : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public TopLevel()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            ContentHolder.Initialize(Content);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            IsMouseVisible = true;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
           
            //nope... it's loaded on demand instead
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }


        //double click handling code mostly copied from https://stackoverflow.com/questions/16111555/how-to-subscribe-to-double-clicks

        double ClickTimer = 0;
        internal enum ClickType { Left = 0b0001, Double = 0b0010, Right = 0b0100, None = 0b1000, All = 0b1111 }
        bool clickHeld = false;
        int lastScroll = 0;
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            ClickType ct = ClickType.None;

            MouseState mouse = Mouse.GetState();

            ClickTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (mouse.LeftButton == ButtonState.Pressed)
            {
                if (ClickTimer < System.Windows.Forms.SystemInformation.DoubleClickTime && !clickHeld)
                    ct = ClickType.Double;
                else
                {
                    ct = ClickType.Left;
                }
                ClickTimer = 0;
                clickHeld = true;
            }
            else
                clickHeld = false;
            if(mouse.RightButton == ButtonState.Pressed)
            {
                ct = ClickType.Right;
            }

            int scroll = mouse.ScrollWheelValue - lastScroll;
            lastScroll = mouse.ScrollWheelValue;

            MouseInput mi = new MouseInput(ct, mouse.Position, scroll);
            List<InputItem> inputList = new List<InputItem>();
            inputList.Add(mi);

            foreach(Keys key in Keyboard.GetState().GetPressedKeys())
            {
                inputList.Add(new KeyboardInput(key));
            }

            InputSet inputSet = new InputSet(inputList);

            ScreenManager.screenManager.Update(inputSet);

            if (ScreenManager.screenManager.IsEmpty())
            {
                
                IEditor editor;

                //TODO: editor choice window
                editor = new CityEditor(GraphicsDevice.DisplayMode.Width - 50, GraphicsDevice.DisplayMode.Height - 150);

                graphics.PreferredBackBufferWidth = editor.Width;
                graphics.PreferredBackBufferHeight = editor.Height;
                graphics.ApplyChanges();

                ScreenManager.screenManager.Push(editor);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            ScreenManager.screenManager.Draw(spriteBatch, SpriteSortMode.FrontToBack, SamplerState.LinearWrap);

            base.Draw(gameTime);
        }
    }
}
