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
    class MapScreen : Screen, IInputRetainer
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

        private static float zoom = MaxZoom / 2;
        private const float zoomSpeedFactor = 0.005f;
        private const int MaxZoom = 10;

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
        { set; private get; }
        internal Texture2D LargePath
        { set; private get; }

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
        bool touching;
        public (bool updateBelow, bool shouldClose) Update(InputSet input)
        {
            if (mapManager is null)
                return (false, false);

            mapManager.Update(input);

            if (!touching)
            {
                if (input.Consume(out InputItem ii, new GestureIdentifier(GestureType.FreeDrag)))
                {
                    inertia = ((GestureInput)ii).Gesture.Delta;
                    ScreenManager.screenManager.RetainInput(this);
                    touching = true;
                }
            }

            //zoom block
            {
                if (input.Consume(out InputItem ii, new GestureIdentifier(GestureType.Pinch)))
                {
                    GestureSample pinch = ((GestureInput)ii).Gesture;
                    Vector2 center = (pinch.Position + pinch.Position2) / 2;
                    Vector2 logicalCenter = ConvertToLogicalSpace(center);

                    float origDist = (pinch.Position - pinch.Position2).Length();
                    float newDist = ((pinch.Position + pinch.Delta) - (pinch.Position2 + pinch.Delta2)).Length();

                    zoom = zoom * origDist / newDist;

                    //the map is tall and the screen is wide, so only the width should be a problem
                    //but, yanno, checking both doesn't hurt?
                    Vector2 projectedMapSize = MapSize * zoom;
                    if (projectedMapSize.X < ScreenSize.X)
                        zoom = ScreenSize.X / MapSize.X;

                    projectedMapSize = MapSize * zoom;
                    if (projectedMapSize.Y < ScreenSize.Y)
                        zoom = ScreenSize.Y / MapSize.Y;

                    if (zoom > MaxZoom)
                        zoom = MaxZoom;

                    //keep it centered where we are, not on the top-left of the map
                    offset = center - (logicalCenter * zoom);
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

        public void HandleRetainedInput(InputSet input)
        {
            if (input.IsEmpty())
            {
                ScreenManager.screenManager.EndRetainedInput(this);
                touching = false;
            }


            if (input.Consume(out InputItem ii, new GestureIdentifier(GestureType.FreeDrag)))
                inertia = ((GestureInput)ii).Gesture.Delta;
        }

        public void Draw(SpriteBatch drawer)
        {
            if (mapManager is null)
                return;

            //draw map
            DrawSpriteNormally(map, new Vector2(0, 0), 0);

            //draw cities, including neighbor paths & faction shields
            foreach (City city in mapManager.Cities)
            {
                DrawSpriteNormally(CityTex, city.Position, 0.5f);

                Vector2 home = city.Position + CityGate;
                foreach(City neighbor in city.GetNeighbors())
                {
                    //to prevent two paths drawing over each other
                    if(city.Position.X >= neighbor.Position.X)
                    {
                        Vector2 destination = neighbor.Position + CityGate;

                        DrawPath(SmallPath, home, destination, 0.25f);
                    }
                }

                if(city.Faction != null)
                {
                    DrawSpriteTinted(FactionShieldTex, city.Position + ShieldOffset, city.Faction.Color, 0.55f);
                }
            }

            Hero activeHero = mapManager.activeHero;

            //draw heroes, including dislocation for dragged hero
            foreach(Hero hero in mapManager.Heroes)
            {
                Vector2 position = hero.Location.Position - HeroOffset;

                if (hero == activeHero)
                    DrawSpriteOffset(HeroTex, position, mapManager.activeHeroDelta, 0.61f);
                else
                    DrawSpriteNormally(HeroTex, position, 0.6f);

            }

            //draw possible move paths for dragged hero
            if(activeHero != null)
            {
                List<City> completed = new List<City>();
                Queue<City> pathOut = new Queue<City>();

                pathOut.Enqueue(activeHero.Location);

                //breadth-first exploring
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

                        Vector2 destination = neighbor.Position + CityGate;

                        DrawPath(LargePath, home, destination, 0.26f);

                        if (neighbor.Faction == activeHero.Faction)
                            pathOut.Enqueue(neighbor);
                        else
                            completed.Add(neighbor);
                    }
                }
            }

            //these methods allow for simpler draw calls, where you can see what's going on
            void DrawSpriteNormally(Texture2D sprite, Vector2 logicalPosition, float depth)
            {
                //TODO: add SourceLocation
                drawer.Draw(sprite, ConvertToScreenSpace(logicalPosition), null, Color.White, 0, new Vector2(), zoom, SpriteEffects.None, depth);
            }

            void DrawSpriteTinted(Texture2D sprite, Vector2 logicalPosition, Color color, float depth)
            {
                //TODO: add SourceLocation
                drawer.Draw(sprite, ConvertToScreenSpace(logicalPosition), null, color, 0, new Vector2(), zoom, SpriteEffects.None, depth);
            }

            void DrawSpriteOffset(Texture2D sprite, Vector2 logicalPosition, Vector2 screenOffset, float depth)
            {
                //TODO: add SourceLocation
                drawer.Draw(sprite, ConvertToScreenSpace(logicalPosition) + screenOffset, null, Color.White, 0, new Vector2(), zoom, SpriteEffects.None, depth);
            }

            //this one also encapsulates the math for drawing a path between two points
            void DrawPath(Texture2D pattern, Vector2 logicalSource, Vector2 logicalDestination, float depth)
            {
                SpriteEffects flip = SpriteEffects.None;
                if (logicalSource.X < logicalDestination.X)
                    flip = SpriteEffects.FlipHorizontally;

                Vector2 pathOffset = new Vector2(pattern.Width / 2, 0);

                Vector2 route = logicalSource - logicalDestination;
                float angle = (float)Math.Atan2(route.Y, route.X);
                //initial angle is off by a quarter circle, so
                angle += .5f * (float)Math.PI;

                Rectangle pathBox = new Rectangle(0, 0, pattern.Width, (int)Math.Floor(route.Length()));
                drawer.Draw(pattern, ConvertToScreenSpace(logicalSource - pathOffset), pathBox, Color.White, angle, pathOffset, zoom, flip, depth);
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