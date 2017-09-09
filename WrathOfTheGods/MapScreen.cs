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
        private Vector2 offset;
        private const int EDGE_BUFFER = 40;
        private const int EDGE_SPEED = 4;
        private static int rightEdge;
        private static int bottomEdge;

        internal Texture2D CityTex
        { set; private get; }
        internal const int CityTexSize = 30;
        private const int CitySize = CityTexSize * Scale;

        private static Vector2 cityGate = new Vector2(CitySize / 2, CitySize - 2 * Scale);  //the offset from the top-left (where the sprite is drawn from) to the center-bottom, where paths are desired to come from

        internal Texture2D SmallPath
        { set; private get; }
        private static Vector2 smallPathOffset;

        internal Texture2D LargePath
        { set; private get; }
        private static Vector2 largePathOffset;


        internal Texture2D HeroTex
        { set; private get; }
        internal static Vector2 HeroTexSize
        { get; private set; } = new Vector2(30, 35);
        private static Vector2 HeroSize = HeroTexSize * Scale;
        internal static Vector2 HeroOffset
        { get; private set; } = HeroSize - new Vector2(CitySize) - new Vector2(5, 10) * Scale;

        internal Texture2D FactionShieldTex
        { set; private get; }
        private static Vector2 ShieldOffset = new Vector2(-2, 20) * Scale;
        

        MapManager mapManager;




        internal const int Scale = 5;

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

        public void SetCities(List<City> cities)
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

            if (smallPathOffset.X == 0 && SmallPath != null)
                smallPathOffset = new Vector2(SmallPath.Width / 2, 0);
            if (largePathOffset.X == 0 && LargePath != null)
                largePathOffset = new Vector2(LargePath.Width / 2, 0);

            foreach (City city in mapManager.Cities)
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

                        Rectangle pathBox = new Rectangle(0, 0, SmallPath.Width, (int)Math.Floor(route.Length() / Scale));
                        drawer.Draw(SmallPath, home - smallPathOffset * Scale, pathBox, Color.White, angle, smallPathOffset, Scale, SpriteEffects.None, 0.25f);
                        
                    }
                }

                if(city.Faction != null)
                {
                    drawer.Draw(FactionShieldTex, ConvertToScreenSpace(city.Position) + ShieldOffset, null, city.Faction.Color, 0, new Vector2(), Scale, SpriteEffects.None, 0.55f);
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

            if(activeHero != null)
            {
                List<City> completed = new List<City>();
                Queue<City> pathOut = new Queue<City>();

                pathOut.Enqueue(activeHero.Location);

                

                while (pathOut.Count > 0)
                {
                    City current = pathOut.Dequeue();
                    if (completed.Contains(current))
                        continue;

                    completed.Add(current);

                    Vector2 home = ConvertToScreenSpace(current.Position) + cityGate;

                    foreach(City neighbor in current.GetNeighbors())
                    {
                        if (completed.Contains(neighbor) || pathOut.Contains(neighbor))
                            continue;  //continues the inner loop

                        SpriteEffects flip = SpriteEffects.None;
                        if(current.Position.X < neighbor.Position.X)
                        {
                            flip = SpriteEffects.FlipHorizontally;
                        }

                        Vector2 destination = ConvertToScreenSpace(neighbor.Position) + cityGate;

                        Vector2 route = home - destination;
                        float angle = (float)Math.Atan2(route.Y, route.X);
                        //initial angle is off by a quarter circle, so
                        angle += .5f * (float)Math.PI;

                        Rectangle pathBox = new Rectangle(0, 0, LargePath.Width, (int)Math.Floor(route.Length() / Scale));
                        drawer.Draw(LargePath, home - largePathOffset * Scale, pathBox, Color.White, angle, largePathOffset, Scale, flip, 0.26f);

                        if (neighbor.Faction == activeHero.Faction)
                            pathOut.Enqueue(neighbor);
                        else
                            completed.Add(neighbor);
                    }
                }
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


                if (input.Consume(out InputItem ii, new GestureIdentifier(GestureType.FreeDrag)))
                    inertia = ((GestureInput)ii).Gesture.Delta;
                
            }

            mapManager.Update(input);

            if(!touching)
            {
                if (input.Consume(out InputItem ii, new GestureIdentifier(GestureType.FreeDrag)))
                {
                    inertia = ((GestureInput)ii).Gesture.Delta;
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