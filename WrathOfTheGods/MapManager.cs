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

using Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace WrathOfTheGods
{
    class MapManager : IInputRetainer
    {

        private static int CitySize = MapScreen.CitySize;
        private static Vector2 HeroSize = MapScreen.HeroSize;

        public List<City> Cities
        { get; private set; }
        

        public List<Hero> Heroes
        { get; private set; }

        public MapManager(List<City> cities, Func<Vector2, Vector2> convertToLogical)
        {
            Cities = cities;

            ConvertToLogicalSpace = convertToLogical;

            Heroes = new List<Hero>();

            //TODO: remove test code
            Hero achilles = new Hero();
            achilles.Location = Cities[5];
            Heroes.Add(achilles);

            Faction achaea = new Faction("Achaea", Color.Firebrick);

            achilles.Faction = achaea;

            achaea.AddCity(Cities[5]);
            achaea.AddCity(Cities[7]);
            achaea.AddCity(Cities[8]);
            achaea.AddCity(Cities[2]);
        }


        internal Hero activeHero = null;
        internal Vector2 activeHeroDelta;
        private Vector2 lastScreenPoint;
        public void Update(InputSet input)
        {
            if(activeHero is null)
            {
                if(input.Consume(out InputItem ii, new GestureOnHero(GestureType.FreeDrag, new Func<Vector2, Hero>(GetHeroAtScreenPoint))))
                {
                    activeHero = GetHeroAtScreenPoint(((GestureInput)ii).Gesture.Position);
                    activeHeroDelta = new Vector2(0);
                    ScreenManager.screenManager.RetainInput(this);
                }
            }

        }

        public void HandleRetainedInput(InputSet input)
        {
            if (input.Consume(out InputItem ii, new GestureIdentifier(GestureType.FreeDrag)))
            {
                GestureInput gi = (GestureInput)ii;

                activeHeroDelta += gi.Gesture.Delta;
                lastScreenPoint = gi.Gesture.Position;
            }
            else if (input.IsEmpty())
            {
                City destination = GetCityAtScreenPoint(lastScreenPoint);
                if (destination != null)
                    TryMove(activeHero, destination);

                activeHero = null;
                ScreenManager.screenManager.EndRetainedInput(this);
            }
        }

        private Func<Vector2, Vector2> ConvertToLogicalSpace;

        /// <summary>
        /// Finds a city, if there is one, at the designated point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns>A city if there is one at the point, or null.</returns>
        private City GetCityAtLogicalPoint(Vector2 point)
        {
            foreach(City city in Cities)
            {
                if (city.Position.X < point.X && city.Position.Y < point.Y
                    && city.Position.X + CitySize > point.X && city.Position.Y + CitySize > point.Y)
                    return city;
            }
            return null;
        }

        private City GetCityAtScreenPoint(Vector2 point)
        {
            return GetCityAtLogicalPoint(ConvertToLogicalSpace(point));
        }

        private Hero GetHeroAtLogicalPoint(Vector2 point)
        {
            foreach(Hero hero in Heroes)
            {
                Vector2 position = hero.GetLogicalPosition();

                if (position.X < point.X && position.Y < point.Y
                    && position.X + HeroSize.X > point.X && position.Y + HeroSize.Y > point.Y)
                    return hero;
            }
            return null;
        }

        private Hero GetHeroAtScreenPoint(Vector2 point)
        {
            return GetHeroAtLogicalPoint(ConvertToLogicalSpace(point));
        }

        private void TryMove(Hero hero, City city)
        {
            hero.Location = city;
        }

        private class GestureOnHero : IInputIdentifier
        {
            GestureType type;
            Func<Vector2, Hero> findHero;

            public GestureOnHero(GestureType _type, Func<Vector2, Hero> _findHero)
            {
                type = _type;
                findHero = _findHero;
            }

            public bool Matches(InputItem input)
            {
                if(input is GestureInput gi)
                    if (gi.Gesture.GestureType == type)
                        if (findHero(gi.Gesture.Position) != null)
                            return true;

                return false;
            }
        }
    }
}