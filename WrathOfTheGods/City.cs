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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace WrathOfTheGods
{
    public class City
    {
        [ContentSerializer]
        public string Name
        { get; private set; }

        [ContentSerializer]
        public string Region
        { get; private set; }

        public Vector2 Position
        {
            get
            {
                return new Vector2(position.X, position.Y);
            }
        }
        [ContentSerializer]
        private Vector2 position;

        [ContentSerializer]
        private List<int> neighbors;

        //public Vector2 spriteVector
        //TODO: draw individual sprites

        //this awkwardness brought to you by the linker crashing when attempting to add content.pipeline to an android project!
        //...i guess it might actually be smaller storage, though?  or at least the serialization is
        private List<City> cityList;

        public City(string name, string region, Vector2 _position, List<City> parent)
        {
            Name = name;
            Region = region;
            position = _position;
            cityList = parent;
            neighbors = new List<int>();
        }

        public List<City> getNeighbors()
        {
            List<City> list = new List<City>();
            foreach (int neighbor in neighbors)
                list.Add(cityList[neighbor]);
            return list;
        }

        public void AddNeighbor(City neighbor)
        {
            neighbors.Add(cityList.IndexOf(neighbor));
        }

        public void RemoveNeighbor(City neighbor)
        {
            neighbors.Remove(cityList.IndexOf(neighbor));
        }

        public void Move(Vector2 vector)
        {
            position += vector;
        }
    }
}