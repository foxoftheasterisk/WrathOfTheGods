using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Screens;
using Microsoft.Xna.Framework;

namespace EditorSuite
{
    class MouseInput : Screens.InputItem
    {
        public TopLevel.ClickType ClickType
        { get; private set; }

        public Point Position
        { get; private set; }

        public int Scroll
        { get; private set; }

        public MouseInput(TopLevel.ClickType clickType, Point position, int scroll)
        {
            ClickType = clickType;
            Position = position;
            Scroll = scroll;
        }
    }

    class MouseInputIdentifier : Screens.IInputIdentifier
    {
        TopLevel.ClickType type;

        public MouseInputIdentifier(TopLevel.ClickType _type)
        {
            type = _type;
        }

        public bool Matches(InputItem input)
        {
            if(input is MouseInput mi)
                if ((mi.ClickType | type) > 0)
                    return true;

            return false;
        }
    }
}
