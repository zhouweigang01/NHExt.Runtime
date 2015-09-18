using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace NHExt.Runtime.Serialize
{
    public class XmlSerialize
    {
        public static string Serialize<T>(T obj)
        {
            if (obj == null) return string.Empty;
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(stream, obj);
            return UnicodeEncoding.UTF8.GetString(stream.GetBuffer());
        }
        public static string Serialize(object obj)
        {
            if (obj == null) return string.Empty;
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            XmlSerializer serializer = new XmlSerializer(obj.GetType());
            serializer.Serialize(stream, obj);
            return UnicodeEncoding.UTF8.GetString(stream.GetBuffer());
        }
        public static T DeSerialize<T>(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return default(T);
            }
            System.IO.MemoryStream stream = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(xml));
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            object obj = serializer.Deserialize(stream);
            return (T)obj;
        }
    }
}
