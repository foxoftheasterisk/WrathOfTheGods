﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using XMLL = WrathOfTheGods.XMLLibrary;

namespace CityEditor
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class TopLevel : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Editor editor;

        public TopLevel()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            editor = new Editor();
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

            Texture2D map = Content.Load<Texture2D>("greece");

            editor.Map = map;
            editor.CityTex = Content.Load<Texture2D>("basiccity");
            editor.Path = Content.Load<Texture2D>("path");
            editor.Font = Content.Load<SpriteFont>("somefont");

            editor.cities = Content.Load<XMLL.SerializableList<XMLL.City>>("citiesEditable");

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            //bug check
            using (XmlWriter writer = XmlWriter.Create("reflect.xml", settings))
            {
                IntermediateSerializer.Serialize<XMLL.SerializableList<XMLL.City>>(writer, editor.cities, null);
            }

            graphics.PreferredBackBufferWidth = map.Width;
            int height = GraphicsDevice.DisplayMode.Height - 150;
            graphics.PreferredBackBufferHeight = height;
            graphics.ApplyChanges();

            editor.BottomEdge = height - map.Height;
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
        internal enum ClickType { Left, Double, Right, None}
        bool clickHeld = false;
        int lastScroll = 0;
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;

                //written as editable
                using (XmlWriter writer = XmlWriter.Create("citiesEditable.xml", settings))
                {
                    IntermediateSerializer.Serialize<XMLL.SerializableList<XMLL.City>>(writer, editor.cities, null);
                }

                
                //but we want a version where they're all just Cities, for actual deployment
                //so 

                //just have no exception handling
                //it's fine, probably
                StreamReader input;
                input = new StreamReader("citiesEditable.xml");
                StreamWriter output = new StreamWriter("cities.xml", false);

                while (true)
                {
                    string line = input.ReadLine();
                    if (line is null)
                        break;

                    //since they have all the same serializable members,
                    //we can just replace the type name
                    //and it's fine
                    line = line.Replace("WrathOfTheGods.XMLLibrary.EditingExtension.EditableCity", "XMLLibrary:City");
                    output.WriteLine(line);
                }

                input.Close();
                input.Dispose();
                output.Close();
                output.Dispose();

                Exit();
            }
            
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

            editor.Update(ct, mouse.Position, scroll);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            editor.Draw(spriteBatch);

            base.Draw(gameTime);
        }
    }
}
