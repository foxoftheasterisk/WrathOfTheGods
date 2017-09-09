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
        {
            set
            {
                MapSize = new Vector2(value.Width, value.Height);
                map = value;
            }
            private get => map;
        }
        private Texture2D map;
        private Vector2 offset;
        private const int EdgeBuffer = 8;
        private const int EdgeSpeed = 1;

        private static Vector2 ScreenSize;
        private static Vector2 MapSize;

        private static float zoom = MaxZoom;
        private const float zoomSpeedFactor = 0.1f;
        private const int MaxZoom = 5;

        internal Texture2D CityTex
        {
            set
            {
                CitySize = value.Height;
                CityGate = new Vector2(CitySize / 2, CitySize - 2);
                cityTex = value;
            }
            private get => cityTex; }
        private Texture2D cityTex;
        internal static int CitySize
        { get; private set; }
        private static Vector2 CityGate;   //the offset from the top-left (where the sprite is drawn from) to the center-bottom(ish), where paths are desired to come from

        internal Texture2D SmallPath
        {
            set
            {
                smallPathOffset = new Vector2(value.Width / 2, 0);
                smallPath = value;
            }
            private get => smallPath; }
        private static Vector2 smallPathOffset;
        private Texture2D smallPath;

        internal Texture2D LargePath
        {
            set
            {
                largePathOffset = new Vector2(value.Width / 2, 0);
                largePath = value;
            }
            private get => largePath;
        }
        private static Vector2 largePathOffset;
        private Texture2D largePath;


        internal Texture2D HeroTex
        {
            set
            {
                HeroSize = new Vector2(value.Width, value.Height);
                HeroOffset = HeroSize - new Vector2(CitySize) - new Vector2(5, 10);
                heroTex = value;
            }
            private get => heroTex;
        }
        private Texture2D heroTex;
        internal static Vector2 HeroSize
        { get; private set; }
        internal static Vector2 HeroOffset
        { get; private set; }

        internal Texture2D FactionShieldTex
        { set; private get; }
        private static Vector2 ShieldOffset = new Vector2(-2, 20);
        

        MapManager mapManager;
        

        public MapScreen()
        {
            offset = new Vector2(0, 0);
        }

        /// <summary>
        /// Sets the size of the screen.
        /// Important for scrolling & zoom limits.
        /// </summary>
        /// <param name="width">The screen's horizontal size</param>
        /// <param name="height">The screen's vertical size</param>
        public void SetScreenSize(int width, int height)
        {
            ScreenSize = new Vector2(width, height);
        }

        public void SetCities(List<City> cities)
        {
            mapManager = new MapManager(cities, new Func<Vector2, Vector2>(ConvertToLogicalSpace));
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
                Vector2 actualMapSize = MapSize * zoom;
                Vector2 edges = ConvertToScreenSpace(MapSize);

                if (offset.X > 0)
                    offset.X = 0;
                if (offset.Y > 0)
                    offset.Y = 0;
                if (edges.X < ScreenSize.X)
                    offset.X = ScreenSize.X - actualMapSize.X;
                if (edges.Y < ScreenSize.Y)
                    offset.Y = ScreenSize.Y - actualMapSize.Y;

                edges = ConvertToScreenSpace(MapSize);

                if (offset.X > -EdgeBuffer * zoom)
                    inertia.X = -EdgeSpeed * zoom;
                if (offset.Y > -EdgeBuffer * zoom)
                    inertia.Y = -EdgeSpeed * zoom;
                if (edges.X < ScreenSize.X + EdgeBuffer * zoom)
                    inertia.X = EdgeSpeed * zoom;
                if (edges.Y < ScreenSize.Y + EdgeBuffer * zoom)
                    inertia.Y = EdgeSpeed * zoom;
            }

            return (false, false);
            //TODO: make returns actually do something.
        }


        public void Draw(SpriteBatch drawer)
        {
            if (mapManager is null)
                return;

            //drawer.Draw(Map, offset, Color.White);
            drawer.Draw(Map, offset, null, Color.White, 0, new Vector2(0,0), zoom, SpriteEffects.None, 0);
            //might actually be easier to do the rectangle version...

            foreach (City city in mapManager.Cities)
            {
                drawer.Draw(CityTex, ConvertToScreenSpace(city.Position), null, Color.White, 0, new Vector2(), zoom, SpriteEffects.None, 0.5f);

                Vector2 home = city.Position + CityGate;
                foreach(City neighbor in city.GetNeighbors())
                {
                    //to prevent two paths drawing over each other, and to make sure paths draw in the direction that looks better
                    if(city.Position.X >= neighbor.Position.X)
                    {
                        Vector2 destination = neighbor.Position + CityGate;

                        Vector2 route = home - destination;
                        float angle = (float)Math.Atan2(route.Y, route.X);
                        //initial angle is off by a quarter circle, so
                        angle += .5f * (float)Math.PI;

                        Rectangle pathBox = new Rectangle(0, 0, SmallPath.Width, (int)Math.Floor(route.Length()));
                        drawer.Draw(SmallPath, ConvertToScreenSpace(home - smallPathOffset), pathBox, Color.White, angle, smallPathOffset, zoom, SpriteEffects.None, 0.25f);
                        
                    }
                }

                if(city.Faction != null)
                {
                    drawer.Draw(FactionShieldTex, ConvertToScreenSpace(city.Position + ShieldOffset), null, city.Faction.Color, 0, new Vector2(), zoom, SpriteEffects.None, 0.55f);
                }
            }

            Hero activeHero = mapManager.activeHero;

            foreach(Hero hero in mapManager.Heroes)
            {
                Vector2 position = ConvertToScreenSpace(hero.Location.Position - HeroOffset);

                if (hero == activeHero)
                    position += mapManager.activeHeroDelta;

                drawer.Draw(HeroTex, position, null, Color.White, 0, new Vector2(), zoom, SpriteEffects.None, 0.6f);
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

                    Vector2 home = current.Position + CityGate;

                    foreach(City neighbor in current.GetNeighbors())
                    {
                        if (completed.Contains(neighbor) || pathOut.Contains(neighbor))
                            continue;  //continues the inner loop

                        SpriteEffects flip = SpriteEffects.None;
                        if(current.Position.X < neighbor.Position.X)
                        {
                            flip = SpriteEffects.FlipHorizontally;
                        }

                        Vector2 destination = neighbor.Position + CityGate;

                        Vector2 route = home - destination;
                        float angle = (float)Math.Atan2(route.Y, route.X);
                        //initial angle is off by a quarter circle, so
                        angle += .5f * (float)Math.PI;

                        Rectangle pathBox = new Rectangle(0, 0, LargePath.Width, (int)Math.Floor(route.Length()));
                        drawer.Draw(LargePath, ConvertToScreenSpace(home - largePathOffset), pathBox, Color.White, angle, largePathOffset, zoom, flip, 0.26f);

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
            point = point / zoom;
            return point;
        }

        private Vector2 ConvertToScreenSpace(Vector2 point)
        {
            point = point * zoom;
            point = point + offset;
            return point;
        }
        

        public void Close()
        {
            //TODO: this
            throw new NotImplementedException();
        }
    }
}