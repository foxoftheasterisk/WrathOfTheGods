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
    {
        public SerializableList() : base()
        { }

        public SerializableList(SerializableList<T> cloneFrom) : base(cloneFrom)
        { }
    }

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

    public class SharedResourceListReader<T> : ContentTypeReader<SerializableList<T>>
    {
        protected override SerializableList<T> Read(ContentReader input, SerializableList<T> existingInstance)
        {
            if (existingInstance == null)
                existingInstance = new SerializableList<T>();

            int count = input.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                input.ReadSharedResource((T item) => existingInstance.Add(item));
            }

            return existingInstance;
        }
    }
}
