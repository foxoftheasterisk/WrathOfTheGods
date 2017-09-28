using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Windows.Forms;

using WrathOfTheGods.XMLLibrary;
using WrathOfTheGods.XMLLibrary.EditingExtension;
using Screens;
using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using System.IO;

namespace EditorSuite
{
    class CityEditor : IEditor
    {
        private static readonly Texture2D map;
        private static readonly Texture2D cityTex;
        private static readonly Texture2D path;
        private static readonly SpriteFont font;

        private static readonly int citySize;
        private static readonly Vector2 cityGate;

        private SerializableList<CityData> cities;

        public int Width
        { get; }
        public int Height
        { get; }

        private int vertOffset;
        private readonly int bottomEdge;
        //to be reusable, there should be a rightEdge and a horizontalOffset... but i don't actually need them for this so fuckit

        //static constructors are a thing, apparently
        //this is maybe not entirely stable (it relies on ContentHolder being initialized first) but...
        //if it works, it works; if not, i'll make it a more explicit singleton type thing
        static CityEditor()
        {
            map = ContentHolder.Load<Texture2D>("greece");
            cityTex = ContentHolder.Load<Texture2D>("basiccity");
            path = ContentHolder.Load<Texture2D>("path");
            font = ContentHolder.Load<SpriteFont>("somefont");

            citySize = cityTex.Width;
            cityGate = new Vector2(citySize / 2, citySize);
        }

        public CityEditor(int maxWidth, int maxHeight)
        {
            cities = new SerializableList<CityData>();

            Width = Math.Min(map.Width, maxWidth);
            Height = Math.Min(map.Height, maxHeight);

            bottomEdge = Height - map.Height;
        }

        public CityEditor(string filename, int maxWidth, int madHeight)
        {
            //TODO: this


        }

        TopLevel.ClickType actionType = TopLevel.ClickType.None;
        EditableCityData activeCity = null;
        Point lastMousePos;

        public (bool updateBelow, bool shouldClose) Update(InputSet input)
        {
            if (input.Consume(out InputItem ii, new MouseInputIdentifier(TopLevel.ClickType.All)))
            {
                MouseInput mouse = (MouseInput)ii;

                Point mousePos = new Point(mouse.Position.X, mouse.Position.Y - vertOffset);

                switch (mouse.ClickType)
                {
                    case TopLevel.ClickType.Double:
                        DoubleClickFunctions();
                        break;

                    case TopLevel.ClickType.Left:
                        if (activeCity != null && actionType == TopLevel.ClickType.Left)
                        {
                            Point delta = mousePos - lastMousePos;
                            activeCity.Move(delta.ToVector2());
                            break;
                        }

                        foreach (EditableCityData city in cities)
                        {
                            if (city.Position.X < mousePos.X && city.Position.Y < mousePos.Y
                               && city.Position.X + citySize > mousePos.X && city.Position.Y + citySize > mousePos.Y)
                            {
                                activeCity = city;
                                actionType = mouse.ClickType;
                            }
                        }
                        break;

                    case TopLevel.ClickType.Right:
                        if (activeCity != null && actionType == TopLevel.ClickType.Right)
                            break;

                        foreach (EditableCityData city in cities)
                        {
                            if (city.Position.X < mousePos.X && city.Position.Y < mousePos.Y
                               && city.Position.X + citySize > mousePos.X && city.Position.Y + citySize > mousePos.Y)
                            {
                                activeCity = city;
                                actionType = mouse.ClickType;
                            }
                        }
                        break;

                    case TopLevel.ClickType.None:
                        if (activeCity != null && actionType == TopLevel.ClickType.Right)
                        {
                            foreach (EditableCityData city in cities)
                            {
                                if (city.Position.X < mousePos.X && city.Position.Y < mousePos.Y
                                   && city.Position.X + citySize > mousePos.X && city.Position.Y + citySize > mousePos.Y)
                                {
                                    if (activeCity != city)
                                    {
                                        if (activeCity.HasNeighbor(city))
                                        {
                                            city.RemoveNeighbor(activeCity);
                                            activeCity.RemoveNeighbor(city);
                                        }
                                        else
                                        {
                                            city.AddNeighbor(activeCity);
                                            activeCity.AddNeighbor(city);
                                        }
                                    }
                                }
                            }
                        }

                        actionType = TopLevel.ClickType.None;
                        activeCity = null;
                        break;
                }

                if (mouse.Scroll != 0)
                {
                    vertOffset += mouse.Scroll;
                    if (vertOffset > 0)
                        vertOffset = 0;
                    if (vertOffset < bottomEdge)
                        vertOffset = bottomEdge;
                }

                lastMousePos = mousePos;

                void DoubleClickFunctions()
                {
                    //only declared now to prevent scope clashing
                    DialogResult result;

                    //first, try to delete
                    foreach (EditableCityData city in cities)
                    {
                        if (city.Position.X < mousePos.X && city.Position.Y < mousePos.Y
                           && city.Position.X + citySize > mousePos.X && city.Position.Y + citySize > mousePos.Y)
                        {
                            result = MessageBox.Show($"Are you sure you want to remove {city.Name}, {city.Region}?", "Are you sure?", MessageBoxButtons.YesNo);

                            if (result == DialogResult.Yes)
                            {
                                foreach (EditableCityData c in cities)
                                {
                                    c.RemoveNeighbor(city);
                                }
                                cities.Remove(city);
                            }

                            return;
                        }
                    }

                    //then, try to create
                    CityNameEntryBox box = new CityNameEntryBox();
                    result = box.ShowDialog();

                    if (result == DialogResult.Cancel)
                        return;

                    cities.Add(new EditableCityData(box.CityNameBox.Text, box.RegionNameBox.Text, mousePos.ToVector2()));
                }
            }

            if(input.Consume(out _, new KeyboardInputIdentifier(Microsoft.Xna.Framework.Input.Keys.Escape)))
            {
                return (false, true);
            }

            return (false, false);

            
        }

        //rotation code based on https://gamedev.stackexchange.com/a/44016
        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 offset = new Vector2(0, vertOffset);

            spriteBatch.Draw(map, offset, Color.White);

            foreach(EditableCityData city in cities)
            {
                spriteBatch.Draw(cityTex, city.Position + offset, null, Color.White, 0f, new Vector2(0), 1, SpriteEffects.None, 0.5f);

                Vector2 home = city.Position + offset + cityGate;
                foreach (EditableCityData neighbor in city.GetNeighbors())
                {
                    Vector2 destination = neighbor.Position + offset + cityGate;

                    Vector2 route = home - destination;
                    float angle = (float)Math.Atan2(route.Y, route.X);
                    angle += .5f * (float)Math.PI;

                    Vector2 pathOffset = new Vector2(path.Width / 2, 0);
                    Rectangle pathBox = new Rectangle(0, 0, path.Width, (int)Math.Floor(route.Length()));
                    spriteBatch.Draw(path, home - pathOffset, pathBox, Color.White, angle, pathOffset, 1, SpriteEffects.None, 0.25f);
                }

                spriteBatch.DrawString(font, city.Name, city.Position + offset + new Vector2(0, citySize), Color.Black);
                spriteBatch.DrawString(font, city.Region, city.Position + offset + new Vector2(0, citySize + 15), Color.Black);
            }
        }

        public bool ShouldClose()
        {
            return false;
        }

        public bool DrawUnder()
        {
            return false;
        }

        public void Close()
        {
            //TODO: make saving and closing not inextricably linked

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            //written as editable
            using (XmlWriter writer = XmlWriter.Create("citiesEditable.xml", settings))
            {
                IntermediateSerializer.Serialize<SerializableList<CityData>>(writer, cities, null);
            }

            //but we want a version where they're all just Cities, for actual deployment
            //so 

            //just have no exception handling
            //it's fine, probably
            //this tool is not intended for release
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
        }
    }
}
