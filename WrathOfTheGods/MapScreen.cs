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
    class MapScreen : Screen
    {
        public Texture2D Map
        { set; private get; }
        public Texture2D CityTex
        { set; private get; }
        public Texture2D Path
        { set; private get; }

        private Vector2 offset;


        public SerializableList<City> Cities
        { set; private get; }

        private const int EDGE_BUFFER = 40;
        private const int EDGE_SPEED = 4;
        private int rightEdge;
        private int bottomEdge;

        private const int scale = 5;
        private const int cityTexSize = 30;
        private const int citySize = cityTexSize * scale;
        private Vector2 cityGate = new Vector2(citySize / 2, citySize - 2*scale);  //the offset from the top-left (where the sprite is drawn from) to the center-bottom, where paths are desired to come from
        private Vector2 pathOffset;

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
            rightEdge = width - (Map.Width * scale);
            bottomEdge = height - (Map.Height * scale);
        }

        public void draw(SpriteBatch drawer)
        {
            //drawer.Draw(Map, offset, Color.White);
            drawer.Draw(Map, offset, null, Color.White, 0, new Vector2(0,0), scale, SpriteEffects.None, 0);
            //might actually be easier to do the rectangle version...

            if (pathOffset.X == 0 && Path != null)
                pathOffset = new Vector2(Path.Width / 2, 0);

            foreach(City city in Cities)
            {
                drawer.Draw(CityTex, offset + city.Position * scale, null, Color.White, 0, new Vector2(0, 0), scale, SpriteEffects.None, 0.5f);

                Vector2 home = (city.Position * scale) + offset + cityGate;
                foreach(City neighbor in city.GetNeighbors())
                {
                    //to prevent two paths drawing over each other, and to make sure paths draw in the direction that looks better
                    if(city.Position.X >= neighbor.Position.X)
                    {
                        Vector2 destination = neighbor.Position * scale + offset + cityGate;

                        Vector2 route = home - destination;
                        float angle = (float)Math.Atan2(route.Y, route.X);
                        //initial angle is off by a quarter circle, so
                        angle += .5f * (float)Math.PI;

                        Rectangle pathBox = new Rectangle(0, 0, Path.Width, (int)Math.Floor(route.Length() / scale));
                        drawer.Draw(Path, home - pathOffset * scale, pathBox, Color.White, angle, pathOffset, scale, SpriteEffects.None, 0.25f);
                        
                    }
                }
            }
        }

        public bool drawUnder()
        {
            return false;
        }

        public bool shouldClose()
        {
            return false;
            //TODO: maybe should close sometimes
            //actually not sure, yet
        }


        private Vector2 inertia = new Vector2(0,0);
        private bool touching;
        public bool update(bool useInput)
        {
            if(useInput)
            {

                while(TouchPanel.IsGestureAvailable)
                {
                    GestureSample gesture = TouchPanel.ReadGesture();

                    switch(gesture.GestureType)
                    {
                        case GestureType.FreeDrag:
                            inertia = gesture.Delta;
                            touching = true;
                            break;
                        case GestureType.DragComplete:
                            touching = false;
                            break;
                    }
                }

            }

            if (inertia.Length() != 0 && !float.IsNaN(inertia.X) && !float.IsNaN(inertia.Y))
            {
                offset += inertia;

                if (offset.X > 0)
                    offset.X = 0;
                if (offset.Y > 0)
                    offset.Y = 0;
                if (offset.X < rightEdge)
                    offset.X = rightEdge;
                if (offset.Y < bottomEdge)
                    offset.Y = bottomEdge;

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

                if (offset.X > -EDGE_BUFFER)
                    inertia.X = -EDGE_SPEED;
                if (offset.Y > -EDGE_BUFFER)
                    inertia.Y = -EDGE_SPEED;
                if (offset.X < rightEdge + EDGE_BUFFER)
                    inertia.X = EDGE_SPEED;
                if (offset.Y < bottomEdge + EDGE_BUFFER)
                    inertia.Y = EDGE_SPEED;
            }

            return false;
        }
    }
}