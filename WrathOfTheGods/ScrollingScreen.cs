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
using Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

using Microsoft.Xna.Framework.Graphics;

namespace WrathOfTheGods
{
    abstract class ScrollingScreen : Screen, IInputRetainer
    {

        private Vector2 offset;

        protected float EdgeBuffer
        { private get; set; }
        protected float EdgeSpeed
        { private get; set; }


        private float zoom;

        protected float ZoomSpeedFactor
        { private get; set; }
        protected float MaxZoom
        { private get; set; }
        //minimum zoom is automatically determined by screen size & limits

        /// <summary>
        /// The bottom-right point allowed to be drawn.
        /// It's assumed the top-left is (0,0).
        /// </summary>
        protected Vector2 Limits
        { private get; set; }

        protected Vector2 ScreenSize
        { private get; set; }

        private Vector2 inertia;

        protected ScrollingScreen()
        {
            offset = new Vector2(0, 0);
        }

        //constructor with all required arguments, to make it easier not to forget one
        protected ScrollingScreen(Vector2 limits, Vector2 screenSize, float edgeBuffer, float edgeSpeed, float maxZoom, float zoomSpeedFactor)
        {
            offset = new Vector2(0, 0);

            Limits = limits;
            ScreenSize = screenSize;
            EdgeBuffer = edgeBuffer;
            EdgeSpeed = edgeSpeed;
            MaxZoom = maxZoom;
            zoom = maxZoom / 2;
            ZoomSpeedFactor = zoomSpeedFactor;
        }

        public virtual (bool updateBelow, bool shouldClose) Update(InputSet input)
        {
            if (!ScreenManager.screenManager.HasRetainer())
            {
                if (input.Consume(out InputItem ii, new GestureIdentifier(GestureType.FreeDrag)))
                {
                    inertia = ((GestureInput)ii).Gesture.Delta;
                    ScreenManager.screenManager.RetainInput(this);
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

                    zoom = zoom * newDist / origDist;

                    Vector2 projectedLimits = Limits * zoom;
                    if (projectedLimits.X < ScreenSize.X)
                        zoom = ScreenSize.X / Limits.X;

                    //redoing so it it's over both, but the width is more restrictive than the height, it doesn't try to go to height
                    //a nearly impossible situation anyway, but
                    projectedLimits = Limits * zoom;
                    if (projectedLimits.Y < ScreenSize.Y)
                        zoom = ScreenSize.Y / Limits.Y;

                    if (zoom > MaxZoom)
                        zoom = MaxZoom;

                    //keep it centered where we are, not on the top-left
                    offset = center - (logicalCenter * zoom);
                }
            }

            //panning block
            if (inertia.Length() != 0 && !float.IsNaN(inertia.X) && !float.IsNaN(inertia.Y))
            {
                offset += inertia;

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

            //Edge detect block
            {
                Vector2 actualSize = Limits * zoom;
                Vector2 edges = ConvertToScreenSpace(Limits);

                if (offset.X > 0)
                    offset.X = 0;
                if (offset.Y > 0)
                    offset.Y = 0;
                if (edges.X < ScreenSize.X)
                    offset.X = ScreenSize.X - actualSize.X;
                if (edges.Y < ScreenSize.Y)
                    offset.Y = ScreenSize.Y - actualSize.Y;

                edges = ConvertToScreenSpace(Limits);

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
        }

        public void HandleRetainedInput(InputSet input)
        {
            if (input.IsEmpty())
            {
                ScreenManager.screenManager.EndRetainedInput(this);
            }


            if (input.Consume(out InputItem ii, new GestureIdentifier(GestureType.FreeDrag)))
                inertia = ((GestureInput)ii).Gesture.Delta;
        }


        protected Vector2 ConvertToLogicalSpace(Vector2 point)
        {
            point = point - offset;
            point = point / zoom;
            return point;
        }

        protected Vector2 ConvertToScreenSpace(Vector2 point)
        {
            point = point * zoom;
            point = point + offset;
            return point;
        }

        /// <summary>
        /// Given a logical position, draws the given sprite at the correct screen position and scale.
        /// </summary>
        /// <param name="drawer"></param>
        /// <param name="sprite"></param>
        /// <param name="logicalPosition"></param>
        /// <param name="depth"></param>
        protected void DrawSprite(SpriteBatch drawer, Texture2D sprite, Vector2 logicalPosition, float depth)
        {
            //TODO: add SourceLocation
            drawer.Draw(sprite, ConvertToScreenSpace(logicalPosition), null, Color.White, 0, new Vector2(), zoom, SpriteEffects.None, depth);
        }

        protected void DrawSprite(SpriteBatch drawer, Texture2D sprite, Vector2 logicalPosition, Color color, float depth)
        {
            //TODO: add SourceLocation
            drawer.Draw(sprite, ConvertToScreenSpace(logicalPosition), null, color, 0, new Vector2(), zoom, SpriteEffects.None, depth);
        }

        protected void DrawSprite(SpriteBatch drawer, Texture2D sprite, Vector2 logicalPosition, Vector2 screenOffset, float depth)
        {
            //TODO: add SourceLocation
            drawer.Draw(sprite, ConvertToScreenSpace(logicalPosition) + screenOffset, null, Color.White, 0, new Vector2(), zoom, SpriteEffects.None, depth);
        }

        /// <summary>
        /// Draws a path using the specified sprite between two logical points
        /// </summary>
        /// <param name="drawer"></param>
        /// <param name="pattern"></param>
        /// <param name="logicalSource"></param>
        /// <param name="logicalDestination"></param>
        /// <param name="depth"></param>
        protected void DrawPath(SpriteBatch drawer, Texture2D pattern, Vector2 logicalSource, Vector2 logicalDestination, float depth)
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

        public abstract bool ShouldClose();
        public abstract bool DrawUnder();
        public abstract void Draw(SpriteBatch drawer);
        public abstract void Close();
    }
}