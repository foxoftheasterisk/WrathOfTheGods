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

using WrathOfTheGods.XMLLibrary;

namespace WrathOfTheGods
{
    class City : CityGameData
    {
        public Faction Faction
        { get; internal set; }
        //the only reason this is even an INTERNAL set is to make it work with Faction's add/remove functions
        //ALWAYS CALL THE ADDTO OR LEAVE FACTION FUNCTIONS

        //this will need expanding, probably
        public City(CityData data) : base(data)
        { }

        public void AddToFaction(Faction f)
        {
            f.AddCity(this);
        }

        public void LeaveFaction()
        {
            Faction.RemoveCity(this);
        }

        public List<City> GetNeighbors()
        {
            List<City> cities = new List<City>();
            List<CityData> neighborData = GetBaseNeighbors();
            foreach(CityData neighborDatum in neighborData)
            {
                cities.Add((City)neighborDatum.Parent);
            }
            return cities;
        }
    }
}