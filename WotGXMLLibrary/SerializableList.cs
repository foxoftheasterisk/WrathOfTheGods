using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

namespace WrathOfTheGods.XMLLibrary
{
    public class SerializableList<T> : List<T>
    { }


    [ContentTypeSerializer]
    class ListSerializer<T> : ContentTypeSerializer<SerializableList<T>>
    {
        static ContentSerializerAttribute itemFormat = new ContentSerializerAttribute()
        {
            ElementName = "Item"
        };



        protected override void Serialize(IntermediateWriter output,
                                          SerializableList<T> value,
                                          ContentSerializerAttribute format)
        {
            foreach (T item in value)
            {
                output.WriteSharedResource(item, itemFormat);
            }
        }


        protected override SerializableList<T> Deserialize(IntermediateReader input,
                                                             ContentSerializerAttribute format,
                                                             SerializableList<T> existingInstance)
        {
            if (existingInstance == null)
                existingInstance = new SerializableList<T>();

            while (input.MoveToElement(itemFormat.ElementName))
            {
                input.ReadSharedResource(itemFormat, (T item) => existingInstance.Add(item));
            }

            return existingInstance;
        }
    }
}
