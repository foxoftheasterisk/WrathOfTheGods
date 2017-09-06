using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace WrathOfTheGods.XMLLibrary
{
    public class City
    {
        [ContentSerializer]
        public string Name
        { get; protected set; }


        [ContentSerializer]
        public string Region
        { get; protected set; }


        public Vector2 Position
        {
            get
            {
                return new Vector2(position.X, position.Y);
            }
            protected set
            {
                position = value;
            }
        }
        [ContentSerializer]
        protected Vector2 position;

        [ContentSerializer]
        protected List<int> neighbors;

        //public Vector2 spriteVector
        //TODO: draw individual sprites

        //this awkwardness brought to you by the linker crashing when attempting to add content.pipeline to an android project!
        //...i guess it might actually be smaller storage, though?  or at least the serialization is
        protected List<City> cityList;

        public City()
        {
            neighbors = new List<int>();
        }

        //I want this to be protected, but if it is then EditableCity can't actually call it :/
        public City(City city)
        {
            Name = city.Name;
            Region = city.Region;
            position = city.Position;
            neighbors = city.GetNeighborIndices();
        }

        protected City(string name, string region, Vector2 _position, List<City> parent)
        {
            Name = name;
            Region = region;
            position = _position;
            cityList = parent;
            neighbors = new List<int>();
        }

        /// <summary>
        /// Adds a reference to the list of cities; must be called before neighbors can be referenced.
        /// </summary>
        /// <param name="parent"></param>
        public void AddParent(List<City> parent)
        {
            cityList = parent;
        }

        public List<City> GetNeighbors()
        {
            List<City> list = new List<City>();
            foreach (int neighbor in neighbors)
                list.Add(cityList[neighbor]);
            return list;
        }

        public List<int> GetNeighborIndices()
        {
            return new List<int>(neighbors);
        }

        public bool HasNeighbor(City other)
        {
            return neighbors.Contains(cityList.IndexOf(other));
        }
    }
}