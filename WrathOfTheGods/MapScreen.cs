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

namespace WrathOfTheGods
{
    class MapScreen : Screen
    {
        public Texture2D Map
        {
            private get;
            set;
        }
        private Vector2 offset;

        private const int EDGE_BUFFER = 40;
        private const int EDGE_SPEED = 4;
        private int rightEdge;
        private int bottomEdge;

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
        public void setScreenSize(int width, int height)
        {
            rightEdge = width - Map.Width;
            bottomEdge = height - Map.Height;
        }

        public void draw(SpriteBatch drawer)
        {
            drawer.Draw(Map, offset, Color.White);
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