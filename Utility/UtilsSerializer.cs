using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Ecm.Utility
{
    /// <summary>
    ///   This class contain methods that help serialize and deserialize xml
    /// </summary>
    public static class UtilsSerializer
    {
        public static T Deserialize<T>(string xml)
        {
            var serializer = new XmlSerializer(typeof(T));
            XmlReader xmlStream = XmlReader.Create(new StringReader(xml + string.Empty));
            var data = (T)serializer.Deserialize(xmlStream);
            xmlStream.Close();

            return data;
        }

        public static T Deserialize<T>(string xml, string rootName)
        {
            var serializer = new XmlSerializer(typeof(T), new XmlRootAttribute(rootName));
            XmlReader xmlStream = XmlReader.Create(new StringReader(xml + string.Empty));
            var data = (T)serializer.Deserialize(xmlStream);
            xmlStream.Close();

            return data;
        }

        public static string Serialize<T>(T data)
        {
            var serializer = new XmlSerializer(typeof(T));
            var stringBuilder = new StringBuilder();
            XmlWriter xmlStream = XmlWriter.Create(stringBuilder);
            serializer.Serialize(xmlStream, data);

            return stringBuilder.ToString();
        }

        public static string Serialize<T>(T data, string rootName)
        {
            var serializer = new XmlSerializer(typeof(T), new XmlRootAttribute(rootName));
            var stringBuilder = new StringBuilder();
            XmlWriter xmlStream = XmlWriter.Create(stringBuilder);
            serializer.Serialize(xmlStream, data);

            return stringBuilder.ToString();
        }

        public static T UnicodeDeserialize<T>(string xmlString)
        {
            try
            {
                T value;
                var xmlSerializer = new XmlSerializer(typeof(T));
                Byte[] array = StringToEncodingArray(xmlString);

                using (var stream = new MemoryStream(array))
                {
                    value = (T)xmlSerializer.Deserialize(stream);
                    stream.Close();
                }
                return value;
            }
            catch (SerializationException)
            {
                return default(T);
            }
        }

        public static string UnicodeSerialize<T>(T data)
        {
            try
            {
                string value;
                var xml = new XmlSerializer(typeof(T));

                using (var memStream = new MemoryStream())
                {
                    TextWriter textWriter = new StreamWriter(memStream, Encoding.Unicode);
                    xml.Serialize(textWriter, data);
                    var count = (int)memStream.Length; // saves object in memory stream

                    var arr = new byte[count];
                    memStream.Seek(0, SeekOrigin.Begin);

                    memStream.Read(arr, 0, count);
                    var utf = new UnicodeEncoding();

                    value = utf.GetString(arr).Trim();
                }
                return value;
            }
            catch (InvalidDataContractException)
            {
                return string.Empty;
            }
        }

        private static Byte[] StringToEncodingArray(String xmlString)
        {
            var encoding = new UnicodeEncoding();
            Byte[] byteArray = encoding.GetBytes(xmlString);
            return byteArray;
        }
    }
}