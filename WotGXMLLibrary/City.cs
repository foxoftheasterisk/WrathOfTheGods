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

        [ContentSerializer(SharedResource = true)]
        private SerializableList<City> neighbors;

        //public Vector2 spriteVector
        //TODO: draw individual sprites

        public City()
        {
            neighbors = new SerializableList<City>();
        }

        public City(string name, string region, Vector2 _position)
        {
            Name = name;
            Region = region;
            position = _position;
            neighbors = new SerializableList<City>();
        }

        public List<City> GetNeighbors()
        {
            return new List<City>(neighbors);
        }

        public void AddNeighbor(City neighbor)
        {
            neighbors.Add(neighbor);
        }

        public void RemoveNeighbor(City neighbor)
        {
            neighbors.Remove(neighbor);
        }

        public void Move(Vector2 vector)
        {
            position += vector;
        }
    }
}