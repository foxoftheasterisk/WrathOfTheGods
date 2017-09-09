using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace WrathOfTheGods.XMLLibrary.EditingExtension
{

    public class EditableCityData : CityData
    {
        public EditableCityData() : base()
        { }

        public EditableCityData(string name, string region, Vector2 _position) : base (name, region, _position)
        { }

        public void AddNeighbor(CityData neighbor)
        {
            neighbors.Add(neighbor);
        }

        public void RemoveNeighbor(CityData neighbor)
        {
            neighbors.Remove(neighbor);
        }

        public void Move(Vector2 vector)
        {
            position += vector;
        }
    }
}
