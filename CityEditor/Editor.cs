using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorSuite
{
    interface IEditor : Screens.Screen
    {
        int Width
        { get; }
        int Height
        { get; }
    }
}
