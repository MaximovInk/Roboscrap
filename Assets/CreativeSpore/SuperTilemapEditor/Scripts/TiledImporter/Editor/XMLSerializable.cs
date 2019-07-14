using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Diagnostics;

namespace CreativeSpore.TiledImporter
{
    public class XMLSerializer
    {

        public XMLSerializer() { }


        public void SaveToXMLFile<T>( T obj_, string sFilePath_ )
        {
            var sXml = Serialize<T>(obj_);

            using (var textWriter = new StreamWriter(sFilePath_, false) )
            {
                textWriter.Write(sXml);
            }
        }

        public T LoadFromXMLFile<T>(string sFilePath_)
        {
            string sXml;

            using (var textReader = new StreamReader(sFilePath_))
            {
                sXml = textReader.ReadToEnd();
            }

            return DeserializeObject<T>(sXml);
        }

        /// <summary>
        /// Method to convert a custom Object to XML string
        /// </summary>
        /// <param name="pObject">Object that is to be serialized to XML</param>
        /// <returns>XML string</returns>
        public String Serialize<T>(T pObject)
        {
            try
            {
                String XmlizedString = null;
                var memoryStream = new MemoryStream();
                var xs = new XmlSerializer(typeof(T));
                var xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
                xmlTextWriter.Formatting = Formatting.Indented;

                xs.Serialize(xmlTextWriter, pObject);
                memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
                XmlizedString = UTF8ByteArrayToString(memoryStream.ToArray());
                return XmlizedString;
            }
            catch (Exception e)
            {
                Debug.Assert(false, e.InnerException +"\n"+ e.Message);
                return null;
            }
        }

        /// <summary>
        /// Method to reconstruct an Object from XML string
        /// </summary>
        /// <param name="pXmlizedString"></param>
        /// <returns></returns>
        public T DeserializeObject<T>(String pXmlizedString)
        {
            var xs = new XmlSerializer(typeof(T));
            var memoryStream = new MemoryStream(StringToUTF8ByteArray(pXmlizedString));

            return (T)xs.Deserialize(memoryStream);
        }

        /// <summary>
        /// To convert a Byte Array of Unicode values (UTF-8 encoded) to a complete String.
        /// </summary>
        /// <param name="characters">Unicode Byte Array to be converted to String</param>
        /// <returns>String converted from Unicode Byte Array</returns>
        private String UTF8ByteArrayToString(Byte[] characters)
        {
            var encoding = new UTF8Encoding();
            var constructedString = encoding.GetString(characters);
            return (constructedString);
        }

        /// <summary>
        /// Converts the String to UTF8 Byte array and is used in De serialization
        /// </summary>
        /// <param name="pXmlString"></param>
        /// <returns></returns>
        private Byte[] StringToUTF8ByteArray(String pXmlString)
        {
            var encoding = new UTF8Encoding();
            var byteArray = encoding.GetBytes(pXmlString);
            return byteArray;
        }
    }
}
