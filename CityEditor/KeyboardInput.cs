using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Screens;

using Microsoft.Xna.Framework.Input;

namespace EditorSuite
{

    //if I was going to be thorough, I'd check for ctrl and such in combination with these
    //but... I'm not
    //not now, anyway, since it probably doesn't matter.
    class KeyboardInput : Screens.InputItem
    {
        public Keys Key
        { get; private set; }

        public KeyboardInput(Keys key)
        {
            Key = key;
        }
    }

    class KeyboardInputIdentifier : Screens.IInputIdentifier
    {
        private Keys key;

        public KeyboardInputIdentifier(Keys _key)
        {
            key = _key;
        }

        public bool Matches(InputItem input)
        {
            if(input is KeyboardInput ki)
                if (ki.Key == key)
                    return true;

            return false;
        }
    }
}
