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
using Microsoft.Xna.Framework.Graphics;
using Screens;

namespace WrathOfTheGods
{
    class BattleScreen : ScrollingScreen
    {
        public override (bool updateBelow, bool shouldClose) Update(InputSet input)
        {
            return base.Update(input);
        }

        public override void Close()
        {
            throw new NotImplementedException();
        }

        public override void Draw(SpriteBatch drawer)
        {
            throw new NotImplementedException();
        }

        public override bool DrawUnder()
        {
            throw new NotImplementedException();
        }

        public override bool ShouldClose()
        {
            throw new NotImplementedException();
        }
    }
}