using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Src.SerializationSurrogates
{
    public class Color32Surrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            Color32 color = (Color32)obj;
            info.AddValue("r", color.r);
            info.AddValue("g", color.g);
            info.AddValue("b", color.b);
            info.AddValue("a", color.a);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            Color32 color = (Color32)obj;
            color.r = (byte)info.GetValue("r", typeof(byte));
            color.g = (byte)info.GetValue("g", typeof(byte));
            color.b = (byte)info.GetValue("b", typeof(byte));
            color.a = (byte)info.GetValue("a", typeof(byte));
            obj = color;
            return obj;
        }
    }
}
