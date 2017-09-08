using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace WrathOfTheGods.XMLLibrary
{
    public class CityData
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
        protected SerializableList<CityData> neighbors;

        [ContentSerializerIgnore]
        public CityGameData Parent
        { get; set; }
        //since this data includes the connections, we'll often want to reach the game data FROM this class
        //this link is for that.

        //public Vector2 spriteVector
        //TODO: draw individual sprites

        public CityData()
        {
            neighbors = new SerializableList<CityData>();
        }

        //I want this to be protected, but if it is then EditableCity can't actually call it :/
        public CityData(CityData city)
        {
            Name = city.Name;
            Region = city.Region;
            position = city.Position;
            neighbors = city.GetNeighborsSerializable();
        }

        protected CityData(string name, string region, Vector2 _position)
        {
            Name = name;
            Region = region;
            position = _position;
            neighbors = new SerializableList<CityData>();
        }

        public List<CityData> GetNeighbors()
        {
            return new List<CityData>(neighbors);
        }

        private SerializableList<CityData> GetNeighborsSerializable()
        {
            return new SerializableList<CityData>(neighbors);
        }

        public bool HasNeighbor(CityData other)
        {
            return neighbors.Contains(other);
        }
    }

    /// <summary>
    /// A wrapper class for CityData that exposes its public members
    /// -except GetNeighbors, which it is highly recommended you expose yourself, using CityData.Parent to convert the list to your derived class.
    /// </summary>
    public abstract class CityGameData
    {
        protected CityData cityData;

        public string Name => cityData.Name;
        public string Region => cityData.Region;
        public Vector2 Position => cityData.Position;

        protected CityGameData(CityData data)
        {
            cityData = data;
            cityData.Parent = this;
        }
    }
}