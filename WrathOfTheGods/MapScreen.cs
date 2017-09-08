using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;


using Microsoft.Xna.Framework.Graphics;
using Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using WrathOfTheGods.XMLLibrary;

namespace WrathOfTheGods
{

    /// <summary>
    /// Handles the graphical elements of the Map-level game.
    /// </summary>
    class MapScreen : Screen
    {
        internal Texture2D Map
        { set; private get; }
        internal Texture2D CityTex
        { set; private get; }
        internal Texture2D Path
        { set; private get; }
        internal Texture2D HeroTex
        { set; private get; }

        private Vector2 offset;

        MapManager mapManager;

        private const int EDGE_BUFFER = 40;
        private const int EDGE_SPEED = 4;
        private static int rightEdge;
        private static int bottomEdge;

        internal const int Scale = 5;
        internal const int CityTexSize = 30;
        private const int CitySize = CityTexSize * Scale;
        private static Vector2 cityGate = new Vector2(CitySize / 2, CitySize - 2 * Scale);  //the offset from the top-left (where the sprite is drawn from) to the center-bottom, where paths are desired to come from
        private static Vector2 pathOffset;
        internal static Vector2 HeroTexSize
        { get; private set; } = new Vector2(30, 35);
        private static Vector2 HeroSize = HeroTexSize * Scale;
        internal static Vector2 HeroOffset
        { get; private set; } = HeroSize - new Vector2(CitySize) - new Vector2(5, 10) * Scale;

        public MapScreen()
        {
            offset = new Vector2(0, 0);
        }

        /// <summary>
        /// Sets the scrolling bounds based on the size of the screen
        /// Should be called AFTER setting Map
        /// </summary>
        /// <param name="width">The screen's horizontal size</param>
        /// <param name="height">The screen's vertical size</param>
        public void SetScreenSize(int width, int height)
        {
            rightEdge = width - (Map.Width * Scale);
            bottomEdge = height - (Map.Height * Scale);
        }

        public void SetCities(SerializableList<City> cities)
        {
            mapManager = new MapManager(cities, new Func<Vector2, Vector2>(ConvertToLogicalSpace));
        }

        public void Draw(SpriteBatch drawer)
        {
            if (mapManager is null)
                return;

            //drawer.Draw(Map, offset, Color.White);
            drawer.Draw(Map, offset, null, Color.White, 0, new Vector2(0,0), Scale, SpriteEffects.None, 0);
            //might actually be easier to do the rectangle version...

            if (pathOffset.X == 0 && Path != null)
                pathOffset = new Vector2(Path.Width / 2, 0);

            foreach(City city in mapManager.Cities)
            {
                drawer.Draw(CityTex, ConvertToScreenSpace(city.Position), null, Color.White, 0, new Vector2(), Scale, SpriteEffects.None, 0.5f);

                Vector2 home = ConvertToScreenSpace(city.Position) + cityGate;
                foreach(City neighbor in city.GetNeighbors())
                {
                    //to prevent two paths drawing over each other, and to make sure paths draw in the direction that looks better
                    if(city.Position.X >= neighbor.Position.X)
                    {
                        Vector2 destination = ConvertToScreenSpace(neighbor.Position) + cityGate;

                        Vector2 route = home - destination;
                        float angle = (float)Math.Atan2(route.Y, route.X);
                        //initial angle is off by a quarter circle, so
                        angle += .5f * (float)Math.PI;

                        Rectangle pathBox = new Rectangle(0, 0, Path.Width, (int)Math.Floor(route.Length() / Scale));
                        drawer.Draw(Path, home - pathOffset * Scale, pathBox, Color.White, angle, pathOffset, Scale, SpriteEffects.None, 0.25f);
                        
                    }
                }
            }

            Hero activeHero = mapManager.activeHero;

            foreach(Hero hero in mapManager.Heroes)
            {
                Vector2 position = ConvertToScreenSpace(hero.Location.Position) - HeroOffset;

                if (hero == activeHero)
                    position += mapManager.activeHeroDelta;

                drawer.Draw(HeroTex, position, null, Color.White, 0, new Vector2(), Scale, SpriteEffects.None, 0.6f);
            }
        }

        public bool DrawUnder()
        {
            return false;
        }

        public bool ShouldClose()
        {
            return false;
            //TODO: it's possible there are times it's not updating it should close
            //like, if the player quicksaves from a battle or something
        }


        private Vector2 ConvertToLogicalSpace(Vector2 point)
        {
            point = point - offset;
            point = point / Scale;
            return point;
        }

        private Vector2 ConvertToScreenSpace(Vector2 point)
        {
            point = point * Scale;
            point = point + offset;
            return point;
        }


        private Vector2 inertia = new Vector2(0,0);
        private bool touching;
        public (bool updateBelow, bool shouldClose) Update(InputSet input)
        {
            if (mapManager is null)
                return (false, false);

            //if actively touching, take input before passing it on
            if (touching)
            {
                if (input.IsEmpty())
                    touching = false;

                GestureInput gesture = input.Consume(new GestureIdentifier(GestureType.FreeDrag)) as GestureInput;

                if (gesture != null)
                    inertia = gesture.Gesture.Delta;

                gesture = input.Consume(new GestureIdentifier(GestureType.DragComplete)) as GestureInput;
                if (gesture != null)
                    touching = false;
                
            }

            mapManager.Update(input);

            if(!touching)
            {
                GestureInput gesture = input.Consume(new GestureIdentifier(GestureType.FreeDrag)) as GestureInput;

                if (gesture != null)
                {
                    inertia = gesture.Gesture.Delta;
                    touching = true;
                }
            }


            //panning block
            if (inertia.Length() != 0 && !float.IsNaN(inertia.X) && !float.IsNaN(inertia.Y))
            {
                offset += inertia;

                if (!touching)
                {
                    Vector2 reduct = inertia;
                    reduct.Normalize();

                    float length = inertia.Length();
                    if (length > 20)
                        reduct *= length / 20;
                    if (length < 1)
                    {
                        inertia = new Vector2(0, 0);
                        reduct = new Vector2(0, 0);
                    }

                    inertia -= reduct;
                }
            }

            //Edge detect block
            {
                if (offset.X > 0)
                    offset.X = 0;
                if (offset.Y > 0)
                    offset.Y = 0;
                if (offset.X < rightEdge)
                    offset.X = rightEdge;
                if (offset.Y < bottomEdge)
                    offset.Y = bottomEdge;

                if (offset.X > -EDGE_BUFFER)
                    inertia.X = -EDGE_SPEED;
                if (offset.Y > -EDGE_BUFFER)
                    inertia.Y = -EDGE_SPEED;
                if (offset.X < rightEdge + EDGE_BUFFER)
                    inertia.X = EDGE_SPEED;
                if (offset.Y < bottomEdge + EDGE_BUFFER)
                    inertia.Y = EDGE_SPEED;
            }

            return (false, false);
            //TODO: make returns actually do something.
        }

        public void Close()
        {
            //TODO: this
            throw new NotImplementedException();
        }
    }
}