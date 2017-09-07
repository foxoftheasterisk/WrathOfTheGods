using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace WrathOfTheGods.XMLLibrary.EditingExtension
{
    [ContentTypeWriter]
    class SharedResourceListWriter<T> : ContentTypeWriter<SerializableList<T>>
    {
        protected override void Write(ContentWriter output, SerializableList<T> value)
        {
            output.Write(value.Count);

            foreach (T item in value)
            {
                output.WriteSharedResource(item);
            }
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(SharedResourceListReader<T>).AssemblyQualifiedName;
        }
    }
}
