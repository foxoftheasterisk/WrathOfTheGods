using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace WrathOfTheGods.XMLLibrary.EditingExtension
{

    public class EditableCity : City
    {
        public EditableCity() : base()
        { }

        public EditableCity(City city) : base(city)
        { }

        public EditableCity(string name, string region, Vector2 _position) : base (name, region, _position)
        { }

        public City Base()
        {
            return new City(this);
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
