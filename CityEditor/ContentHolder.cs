using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Content;

namespace EditorSuite
{
    /// <summary>
    /// wrapper for MonoGame's ContentManager that:
    /// *is accessible from static
    /// </summary>
    class ContentHolder
    {
        private static ContentHolder contentHolder;
        private ContentManager content;

        private ContentHolder(ContentManager _content)
        {
            content = _content;
        }

        public static void Initialize(ContentManager content)
        {
            contentHolder = new ContentHolder(content);
        }

        public static T Load<T>(string contentName)
        {
            if (contentHolder == null)
                throw new InvalidOperationException("ContentHolder has not been initialized!");
            return contentHolder.content.Load<T>(contentName);
        }

    }
}
