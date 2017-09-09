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
using Microsoft.Xna.Framework;

namespace WrathOfTheGods
{
    class Hero
    {
        //TODO: this

        public City Location
        { get; set; }

        public Faction Faction
        { get; internal set; }

        public Vector2 GetLogicalPosition()
        {
            return Location.Position - MapScreen.HeroOffset;
        }
    }
}