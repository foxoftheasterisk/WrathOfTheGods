using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace WrathOfTheGods.XMLLibrary
{
    public class Terrain
    {
        [ContentSerializer]
        public string Name
        { get; private set; }

        public Texture2D Texture
        { get; private set; }

        [ContentSerializer]
        public Vector2 Offset
        { get; private set; }

        public Terrain()
        { }

        public Terrain(string name)
        {
            Name = name;
        }

        public void LoadTexture(ContentManager content, string path)
        {
            content.Load<Texture2D>(path + Name);
        }

        //for now, at least, that's all we need

    }
}
