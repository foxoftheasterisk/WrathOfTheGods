using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using WrathOfTheGods.XMLLibrary;

namespace CityEditor
{

    public class EditableCity : City
    {
        public EditableCity(City city) : base(city)
        { }

        public EditableCity(string name, string region, Vector2 _position, List<City> parent) : base (name, region, _position, parent)
        { }

        public City Base()
        {
            return new City(this);
        }

        public void AddNeighbor(City neighbor)
        {
            neighbors.Add(cityList.IndexOf(neighbor));
        }

        public void RemoveNeighbor(City neighbor)
        {
            neighbors.Remove(cityList.IndexOf(neighbor));
        }

        //more awkwardness caused by the friggin impossibility of making a SharedResourceList in Android
        public void RemoveCity(City city)
        {
            int index = cityList.IndexOf(city);
            List<int> newNeighbors = new List<int>();

            foreach(int neighbor in neighbors)
            {
                if (neighbor > index)
                    newNeighbors.Add(neighbor - 1);
                else if (neighbor < index)
                    newNeighbors.Add(neighbor);

                //neighbor == index deliberately omitted;
                //this is the city being removed, so it can no longer be a neighbor
            }

            neighbors = newNeighbors;
        }

        public void Move(Vector2 vector)
        {
            position += vector;
        }
    }
}
