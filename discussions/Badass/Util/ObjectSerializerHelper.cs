using System.IO;
using System.Xml.Serialization;

namespace Badass.Util
{
    public class ObjectSerializerHelper
    {
        public static void SerializeObjectToXmlFile(object serializableObject, string fileName)
        {
            if (serializableObject == null) { return; }

            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                var xmlSerializer = new XmlSerializer(serializableObject.GetType());
                xmlSerializer.Serialize(fs, serializableObject);
                fs.Flush(true);
            } 
        }

        public static T DeSerializeObjectFromFile<T>(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                return default(T);
            }

            T objectOut;

            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                objectOut = (T)xmlSerializer.Deserialize(fs);
            }

            return objectOut;
        }

        public static string SerializeObjectToString<T>(T toSerialize)
        {
            var xmlSerializer = new XmlSerializer(toSerialize.GetType());
            var stringWriter = new StringWriter();
            xmlSerializer.Serialize(stringWriter, toSerialize);
            return stringWriter.ToString();
        }

        public static T DeSerializeObjectFromString<T>(string toDeserialize)
        {
            T objectOut;

            using (var sr = new StringReader(toDeserialize))
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                objectOut = (T)xmlSerializer.Deserialize(sr);
            }

            return objectOut;
        }

        public static T DeSerializeObjectFromStream<T>(Stream xmlStream)
        {
            T objectOut;

            using (var sr = new StreamReader(xmlStream))
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                objectOut = (T)xmlSerializer.Deserialize(sr);
            }

            return objectOut;
        }
    }
}
