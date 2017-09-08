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

        [ContentSerializer(SharedResource = true)]
        protected SerializableList<City> neighbors;

        //public Vector2 spriteVector
        //TODO: draw individual sprites

        [ContentSerializerIgnore]
        public CityGameData data;
        //seperates out any runtime data from the XML-serialized permanent data

        public City()
        {
            neighbors = new SerializableList<City>();
        }

        //I want this to be protected, but if it is then EditableCity can't actually call it :/
        public City(City city)
        {
            Name = city.Name;
            Region = city.Region;
            position = city.Position;
            neighbors = city.GetNeighborsSerializable();
        }

        protected City(string name, string region, Vector2 _position)
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

        private SerializableList<City> GetNeighborsSerializable()
        {
            return new SerializableList<City>(neighbors);
        }

        public bool HasNeighbor(City other)
        {
            return neighbors.Contains(other);
        }
    }

    public abstract class CityGameData
    { }
}