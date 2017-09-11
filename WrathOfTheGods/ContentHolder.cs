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

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using WrathOfTheGods.XMLLibrary;

namespace WrathOfTheGods
{
    /// <summary>
    /// simple static class to hold all the content
    /// </summary>
    static class ContentHolder
    {
        public static bool IsLoaded
        { get; private set; } = false;

        public static Texture2D GreeceMap
        { get; private set; }

        public static Texture2D CityTex
        { get; private set; }
        public static Texture2D HeroTex
        { get; private set; }
        public static Texture2D FactionShieldTex
        { get; private set; }

        public static Texture2D SmallPathTex
        { get; private set; }
        public static Texture2D LargePathTex
        { get; private set; }

        public static SerializableList<CityData> CityData
        { get; private set; }

        public static void LoadContent(ContentManager content)
        {
            GreeceMap = content.Load<Texture2D>("greece");

            CityTex = content.Load<Texture2D>("basiccity");
            HeroTex = content.Load<Texture2D>("achilles");
            FactionShieldTex = content.Load<Texture2D>("shield");

            SmallPathTex = content.Load<Texture2D>("smallpath");
            LargePathTex = content.Load<Texture2D>("path");
            
            CityData = content.Load<SerializableList<CityData>>("cities");
        }

    }
}