using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Windows.Forms;
using WrathOfTheGods.XMLLibrary;

namespace CityEditor
{
    class Editor
    {
        public Texture2D Map
        { set; private get; }
        public Texture2D CityTex
        { set; private get; }
        public Texture2D Path
        { set; private get; }
        public SpriteFont Font
        { set; private get; }

        public SerializableList<City> cities;

        private const int citySize = 30;
        private Vector2 cityGate = new Vector2(citySize / 2, citySize);

        public Editor()
        {
            cities = new SerializableList<City>();
        }

        private int vertOffset;
        public int BottomEdge
        { set; private get; }


        TopLevel.ClickType actionType = TopLevel.ClickType.None;
        City activeCity = null;
        Point lastMousePos;

        internal void Update(TopLevel.ClickType clickType, Point mouse, int scroll)
        {
            mouse.Y -= vertOffset;

            switch(clickType)
            {
                case TopLevel.ClickType.Double:

                    CityNameEntryBox box = new CityNameEntryBox();
                    DialogResult result = box.ShowDialog();

                    if (result == DialogResult.Cancel)
                        break;

                    cities.Add(new City(box.CityNameBox.Text, box.RegionNameBox.Text, mouse.ToVector2()));

                    break;
                case TopLevel.ClickType.Left:
                    if(activeCity != null && actionType == TopLevel.ClickType.Left)
                    {
                        Point delta = mouse - lastMousePos;
                        activeCity.Move(delta.ToVector2());
                        break;
                    }

                    foreach (City city in cities)
                    {
                        if(city.Position.X < mouse.X && city.Position.Y < mouse.Y
                           && city.Position.X + citySize > mouse.X && city.Position.Y + citySize > mouse.Y)
                        {
                            activeCity = city;
                            actionType = clickType;
                        }
                    }

                    break;
                case TopLevel.ClickType.Right:
                    if (activeCity != null && actionType == TopLevel.ClickType.Right)
                        break;

                    foreach (City city in cities)
                    {
                        if (city.Position.X < mouse.X && city.Position.Y < mouse.Y
                           && city.Position.X + citySize > mouse.X && city.Position.Y + citySize > mouse.Y)
                        {
                            activeCity = city;
                            actionType = clickType;
                        }
                    }

                    break;
                case TopLevel.ClickType.None:
                    if (activeCity != null && actionType == TopLevel.ClickType.Right)
                    {
                        foreach (City city in cities)
                        {
                            if (city.Position.X < mouse.X && city.Position.Y < mouse.Y
                               && city.Position.X + citySize > mouse.X && city.Position.Y + citySize > mouse.Y)
                            {
                                if(activeCity != city)
                                {
                                    city.AddNeighbor(activeCity);
                                    activeCity.AddNeighbor(city);
                                }
                            }
                        }
                    }

                    actionType = TopLevel.ClickType.None;
                    activeCity = null;
                    break;
            }

            if (scroll != 0)
            {
                vertOffset += scroll;
                if (vertOffset > 0)
                    vertOffset = 0;
                if (vertOffset < BottomEdge)
                    vertOffset = BottomEdge;
            }

            lastMousePos = mouse;
        }

        //rotation code based on https://gamedev.stackexchange.com/a/44016

        internal void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.FrontToBack, null, SamplerState.LinearWrap);

            Vector2 offset = new Vector2(0, vertOffset);

            spriteBatch.Draw(Map, offset, Color.White);

            foreach(City city in cities)
            {
                spriteBatch.Draw(CityTex, city.Position + offset, null, Color.White, 0f, new Vector2(0), 1, SpriteEffects.None, 0.5f);

                Vector2 home = city.Position + offset + cityGate;
                foreach (City neighbor in city.GetNeighbors())
                {
                    Vector2 destination = neighbor.Position + offset + cityGate;

                    Vector2 route = home - destination;
                    float angle = (float)Math.Atan2(route.Y, route.X);
                    angle += .5f * (float)Math.PI;

                    Vector2 pathOffset = new Vector2(Path.Width / 2, 0);
                    Rectangle pathBox = new Rectangle(0, 0, Path.Width, (int)Math.Floor(route.Length()));
                    spriteBatch.Draw(Path, home - pathOffset, pathBox, Color.White, angle, pathOffset, 1, SpriteEffects.None, 0.25f);
                }

                spriteBatch.DrawString(Font, city.Name, city.Position + offset + new Vector2(0, citySize), Color.Black);
                spriteBatch.DrawString(Font, city.Region, city.Position + offset + new Vector2(0, citySize + 15), Color.Black);
            }

            spriteBatch.End();
        }
    }
}
