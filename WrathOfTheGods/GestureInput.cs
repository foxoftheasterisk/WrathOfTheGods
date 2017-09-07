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
using Microsoft.Xna.Framework.Input.Touch;

namespace WrathOfTheGods
{
    class GestureInput : InputItem
    {
        public GestureSample Gesture
        { get; private set; }

        public GestureInput(GestureSample gesture)
        {
            Gesture = gesture;
        }

        
    }

    class GestureIdentifier : IInputIdentifier
    {
        GestureType seeking;

        public GestureIdentifier(GestureType type)
        {
            seeking = type;
        }

        public bool Matches(InputItem input)
        {
            if(input is GestureInput gInput)
            {
                return gInput.Gesture.GestureType == seeking;
            }
            return false;
        }
    }
}