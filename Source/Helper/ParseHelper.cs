using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace LinkUtilities.Helper
{
    /// <summary>
    /// Helper functions for parsing strings
    /// </summary>
    internal static class ParseHelper
    {
        /// <summary>
        /// Converts a string to a stream.
        /// </summary>
        /// <param name="this">String to convert</param>
        /// <returns>Stream from the string</returns>
        public static Stream ToStream(this string @this)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(@this);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Deserializes an XML string into an object
        /// </summary>
        /// <typeparam name="T">Object the XML will be deserualized to.</typeparam>
        /// <param name="this">XML string</param>
        /// <returns>The deserialized XML</returns>
        public static T ParseXML<T>(this string @this) where T : class
        {
            XmlReader reader = XmlReader.Create(@this.Trim().ToStream(), new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Document });
            return new XmlSerializer(typeof(T)).Deserialize(reader) as T;
        }
    }
}
