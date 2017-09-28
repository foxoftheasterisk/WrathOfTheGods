using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Screens;

namespace EditorSuite
{
    class WindowClosedInput : InputItem
    { }

    class WindowClosedInputIdentifier : IInputIdentifier
    {
        public bool Matches(InputItem input)
        {
            if (input is WindowClosedInput)
                return true;
            return false;
        }
    }
}
