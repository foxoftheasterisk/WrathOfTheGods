using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrathOfTheGods.XMLLibrary
{
    class Battlefield
    {
        public Tile this[Position p]
        {
            get
            {
                return layout[p.x, p.y];
            }
        }

        private Tile[,] layout;

        public enum Direction { Northwest, West, Southwest, Northeast, East, Southeast };

        public struct Position
        {
            public int x;
            public int y;

            public Position(int _x, int _y)
            {
                x = _x;
                y = _y;
            }

            public Position Neighbor(Direction d)
            {
                switch (d)
                {
                    case Direction.East:
                        return new Position(x + 1, y);
                    case Direction.West:
                        return new Position(x - 1, y);
                    case Direction.Northwest when (y % 2 == 0):
                    case Direction.Northeast when (y % 2 != 0):
                        return new Position(x, y - 1);
                    case Direction.Northwest:
                        return new Position(x - 1, y - 1);
                    case Direction.Northeast:
                        return new Position(x + 1, y - 1);
                    case Direction.Southeast when (y % 2 != 0):
                    case Direction.Southwest when (y % 2 == 0):
                        return new Position(x, y + 1);
                    case Direction.Southeast:
                        return new Position(x + 1, y + 1);
                    case Direction.Southwest:
                        return new Position(x - 1, y + 1);
                    default:
                        throw new Exception("Nonexistant direction???");
                }
            }

            public bool IsNeighbor(Position p)
            {
                if (Math.Abs(p.x - x) > 1)
                    return false;

                if (Math.Abs(p.y - y) > 1)
                    return false;

                //we are within one tile, we just need to make sure it's not one of the "hopping" diagonals
                
                if(y % 2 == 0)
                    if (p.x < x && p.y != y)
                        return false;
                else //y % 2 != 0
                    if (p.x > x && p.y != y)
                        return false;

                return true;
            }


        }

        public struct Tile
        {
            [ContentSerializer(SharedResource = true)]
            Terrain terrain;

            int height;
            Position position;

            public bool IsNeighbor(Tile t)
            {
                return position.IsNeighbor(t.position);
            }
        }


    }
}
