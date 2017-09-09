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

namespace WrathOfTheGods
{
    class Faction
    {
        string name;

        public Color Color
        { get; private set; }

        internal List<City> Cities
        { get => new List<City>(cities); private set => cities = value; }
        List<City> cities;
        List<Hero> heroes;

        public Faction(string _name, Color _color)
        {
            name = _name;
            Color = _color;

            cities = new List<City>();
            heroes = new List<Hero>();
        }

        public void AddCity(City city)
        {
            if (city.Faction != this)
            {
                if (city.Faction != null)
                    city.LeaveFaction();
                cities.Add(city);
                city.Faction = this;
            }
        }

        public void RemoveCity(City city)
        {
            if(city.Faction == this)
            {
                city.Faction = null;
                cities.Remove(city);
            }
        }
    }
}