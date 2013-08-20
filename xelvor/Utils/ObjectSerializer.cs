using System;
using System.IO;				 // For reading/writing data to an XML/JSON file.
using System.IO.IsolatedStorage; // For accessing user isolated data.
using System.Runtime.Serialization.Formatters.Binary; // For serialization of an object to an XML Binary file.
using System.Runtime.Serialization.Json;    // For serialization of a JSON to an XML Document file.
using System.Text;	 // For serialization of an object to an XML Document file.
using System.Xml.Serialization;
using System.Collections.Generic;

namespace xelvor.Utils
{
    /// <summary>
    /// Serialization format types.
    /// </summary>
    public enum SerializedFormat
    {
        /// <summary>
        /// Binary serialization format.
        /// </summary>
        Binary,

        /// <summary>
        /// JSON serialization format.
        /// </summary>
        JSON,

        /// <summary>
        /// Document serialization format.
        /// </summary>
        Document
    }

    /// <summary>
    /// Facade to XML/JSON serialization and deserialization of strongly typed objects to/from an XML/JSON file.
    /// </summary>
    public static class ObjectSerializer<T> where T : class // Specify that T must be a class.
    {
        #region Load methods

        /// <summary>
        /// Loads an object from a JSON file in Document format.
        /// </summary>
        /// <example>
        /// <code>
        /// serializableObject = ObjectSerializer&lt;SerializableObject&gt;.Load(@"C:\JSONObjects.json");
        /// </code>
        /// </example>
        /// <param name="path">Path of the file to load the object from.</param>
        /// <returns>Object loaded from a JSON file in Document format.</returns>
        public static T Load(string path)
        {
            T serializableObject = LoadFromJSONFormat(null, path, null);
            return serializableObject;
        }

        /// <summary>
        /// Loads an object from an XML/JSON file using a specified serialized format.
        /// </summary>
        /// <example>
        /// <code>
        /// serializableObject = ObjectSerializer&lt;SerializableObject&gt;.Load(@"C:\XMLObjects.xml", SerializedFormat.Binary);
        /// </code>
        /// </example>		
        /// <param name="path">Path of the file to load the object from.</param>
        /// <param name="serializedFormat">XML/JSON serialized format used to load the object.</param>
        /// <returns>Object loaded from an XML/JSON file using the specified serialized format.</returns>
        public static T Load(string path, SerializedFormat serializedFormat)
        {
            T serializableObject = null;

            switch (serializedFormat)
            {
                case SerializedFormat.Binary:
                    serializableObject = LoadFromBinaryFormat(path, null);
                    break;
                case SerializedFormat.Document:
                    serializableObject = LoadFromDocumentFormat(null, path, null);
                    break;
                case SerializedFormat.JSON:
                default:
                    serializableObject = LoadFromJSONFormat(null, path, null);
                    break;
            }

            return serializableObject;
        }

        /// <summary>
        /// Loads an object from an XML file in Document format, supplying extra data types to enable deserialization of custom types within the object.
        /// </summary>
        /// <example>
        /// <code>
        /// serializableObject = ObjectSerializer&lt;SerializableObject&gt;.Load(@"C:\XMLObjects.xml", new Type[] { typeof(MyCustomType) });
        /// </code>
        /// </example>
        /// <param name="path">Path of the file to load the object from.</param>
        /// <param name="extraTypes">Extra data types to enable deserialization of custom types within the object.</param>
        /// <returns>Object loaded from an XML file in Document format.</returns>
        public static T Load(string path, System.Type[] extraTypes)
        {
            T serializableObject = LoadFromDocumentFormat(extraTypes, path, null);
            return serializableObject;
        }

        /// <summary>
        /// Loads an object from a JSON file in Document format, located in a specified isolated storage area.
        /// </summary>
        /// <example>
        /// <code>
        /// serializableObject = ObjectSerializer&lt;SerializableObject&gt;.Load("JSONObjects.json", IsolatedStorageFile.GetUserStoreForAssembly());
        /// </code>
        /// </example>
        /// <param name="fileName">Name of the file in the isolated storage area to load the object from.</param>
        /// <param name="isolatedStorageDirectory">Isolated storage area directory containing the JSON file to load the object from.</param>
        /// <returns>Object loaded from a JSON file in Document format located in a specified isolated storage area.</returns>
        public static T Load(string fileName, IsolatedStorageFile isolatedStorageDirectory)
        {
            T serializableObject = LoadFromJSONFormat(null, fileName, isolatedStorageDirectory);
            return serializableObject;
        }

        /// <summary>
        /// Loads an object from an XML/JSON file located in a specified isolated storage area, using a specified serialized format.
        /// </summary>
        /// <example>
        /// <code>
        /// serializableObject = ObjectSerializer&lt;SerializableObject&gt;.Load("XMLObjects.xml", IsolatedStorageFile.GetUserStoreForAssembly(), SerializedFormat.Binary);
        /// </code>
        /// </example>		
        /// <param name="fileName">Name of the file in the isolated storage area to load the object from.</param>
        /// <param name="isolatedStorageDirectory">Isolated storage area directory containing the XML/JSON file to load the object from.</param>
        /// <param name="serializedFormat">XML/JSON serialized format used to load the object.</param>        
        /// <returns>Object loaded from an XML/JSON file located in a specified isolated storage area, using a specified serialized format.</returns>
        public static T Load(string fileName, IsolatedStorageFile isolatedStorageDirectory, SerializedFormat serializedFormat)
        {
            T serializableObject = null;

            switch (serializedFormat)
            {
                case SerializedFormat.Binary:
                    serializableObject = LoadFromBinaryFormat(fileName, isolatedStorageDirectory);
                    break;

                case SerializedFormat.Document:
                    serializableObject = LoadFromDocumentFormat(null, fileName, isolatedStorageDirectory);
                    break;

                case SerializedFormat.JSON:
                default:
                    serializableObject = LoadFromJSONFormat(null, fileName, isolatedStorageDirectory);
                    break;
            }

            return serializableObject;
        }

        /// <summary>
        /// Loads an object from an XML file in Document format, located in a specified isolated storage area, and supplying extra data types to enable deserialization of custom types within the object.
        /// </summary>
        /// <example>
        /// <code>
        /// serializableObject = ObjectSerializer&lt;SerializableObject&gt;.Load("XMLObjects.xml", IsolatedStorageFile.GetUserStoreForAssembly(), new Type[] { typeof(MyCustomType) });
        /// </code>
        /// </example>		
        /// <param name="fileName">Name of the file in the isolated storage area to load the object from.</param>
        /// <param name="isolatedStorageDirectory">Isolated storage area directory containing the XML file to load the object from.</param>
        /// <param name="extraTypes">Extra data types to enable deserialization of custom types within the object.</param>
        /// <returns>Object loaded from an XML file located in a specified isolated storage area, using a specified serialized format.</returns>
        public static T Load(string fileName, IsolatedStorageFile isolatedStorageDirectory, System.Type[] extraTypes)
        {
            T serializableObject = LoadFromDocumentFormat(null, fileName, isolatedStorageDirectory);
            return serializableObject;
        }

        #endregion

        #region Save methods

        /// <summary>
        /// Saves an object to a JSON file in Document format.
        /// </summary>
        /// <example>
        /// <code>        
        /// SerializableObject serializableObject = new SerializableObject();
        /// 
        /// ObjectSerializer&lt;SerializableObject&gt;.Save(serializableObject, @"C:\JSONObjects.json");
        /// </code>
        /// </example>
        /// <param name="serializableObject">Serializable object to be saved to file.</param>
        /// <param name="path">Path of the file to save the object to.</param>
        public static void Save(T serializableObject, string path)
        {
            SaveToJSONFormat(serializableObject, null, path, null);
        }

        /// <summary>
        /// Saves an object to an XML/JSON file using a specified serialized format.
        /// </summary>
        /// <example>
        /// <code>
        /// SerializableObject serializableObject = new SerializableObject();
        /// 
        /// ObjectSerializer&lt;SerializableObject&gt;.Save(serializableObject, @"C:\XMLObjects.xml", SerializedFormat.Binary);
        /// </code>
        /// </example>
        /// <param name="serializableObject">Serializable object to be saved to file.</param>
        /// <param name="path">Path of the file to save the object to.</param>
        /// <param name="serializedFormat">XML/JSON serialized format used to save the object.</param>
        public static void Save(T serializableObject, string path, SerializedFormat serializedFormat)
        {
            switch (serializedFormat)
            {
                case SerializedFormat.Binary:
                    SaveToBinaryFormat(serializableObject, path, null);
                    break;

                case SerializedFormat.Document:
                    SaveToDocumentFormat(serializableObject, null, path, null);
                    break;

                case SerializedFormat.JSON:
                default:
                    SaveToJSONFormat(serializableObject, null, path, null);
                    break;
            }
        }

        /// <summary>
        /// Saves an object to an XML file in Document format, supplying extra data types to enable serialization of custom types within the object.
        /// </summary>
        /// <example>
        /// <code>        
        /// SerializableObject serializableObject = new SerializableObject();
        /// 
        /// ObjectSerializer&lt;SerializableObject&gt;.Save(serializableObject, @"C:\XMLObjects.xml", new Type[] { typeof(MyCustomType) });
        /// </code>
        /// </example>
        /// <param name="serializableObject">Serializable object to be saved to file.</param>
        /// <param name="path">Path of the file to save the object to.</param>
        /// <param name="extraTypes">Extra data types to enable serialization of custom types within the object.</param>
        public static void Save(T serializableObject, string path, System.Type[] extraTypes)
        {
            SaveToDocumentFormat(serializableObject, extraTypes, path, null);
        }

        /// <summary>
        /// Saves an object to a JSON file in Document format, located in a specified isolated storage area.
        /// </summary>
        /// <example>
        /// <code>        
        /// SerializableObject serializableObject = new SerializableObject();
        /// 
        /// ObjectSerializer&lt;SerializableObject&gt;.Save(serializableObject, "JSONObjects.json", IsolatedStorageFile.GetUserStoreForAssembly());
        /// </code>
        /// </example>
        /// <param name="serializableObject">Serializable object to be saved to file.</param>
        /// <param name="fileName">Name of the file in the isolated storage area to save the object to.</param>
        /// <param name="isolatedStorageDirectory">Isolated storage area directory containing the JSON file to save the object to.</param>
        public static void Save(T serializableObject, string fileName, IsolatedStorageFile isolatedStorageDirectory)
        {
            SaveToJSONFormat(serializableObject, null, fileName, isolatedStorageDirectory);
        }

        /// <summary>
        /// Saves an object to an XML/JSON file located in a specified isolated storage area, using a specified serialized format.
        /// </summary>
        /// <example>
        /// <code>        
        /// SerializableObject serializableObject = new SerializableObject();
        /// 
        /// ObjectSerializer&lt;SerializableObject&gt;.Save(serializableObject, "XMLObjects.xml", IsolatedStorageFile.GetUserStoreForAssembly(), SerializedFormat.Binary);
        /// </code>
        /// </example>
        /// <param name="serializableObject">Serializable object to be saved to file.</param>
        /// <param name="fileName">Name of the file in the isolated storage area to save the object to.</param>
        /// <param name="isolatedStorageDirectory">Isolated storage area directory containing the XML/JSON file to save the object to.</param>
        /// <param name="serializedFormat">XML/JSON serialized format used to save the object.</param>        
        public static void Save(T serializableObject, string fileName, IsolatedStorageFile isolatedStorageDirectory, SerializedFormat serializedFormat)
        {
            switch (serializedFormat)
            {
                case SerializedFormat.Binary:
                    SaveToBinaryFormat(serializableObject, fileName, isolatedStorageDirectory);
                    break;

                case SerializedFormat.Document:
                    SaveToDocumentFormat(serializableObject, null, fileName, isolatedStorageDirectory);
                    break;

                case SerializedFormat.JSON:
                default:
                    SaveToJSONFormat(serializableObject, null, fileName, isolatedStorageDirectory);
                    break;
            }
        }

        /// <summary>
        /// Saves an object to an XML file in Document format, located in a specified isolated storage area, and supplying extra data types to enable serialization of custom types within the object.
        /// </summary>
        /// <example>
        /// <code>
        /// SerializableObject serializableObject = new SerializableObject();
        /// 
        /// ObjectSerializer&lt;SerializableObject&gt;.Save(serializableObject, "XMLObjects.xml", IsolatedStorageFile.GetUserStoreForAssembly(), new Type[] { typeof(MyCustomType) });
        /// </code>
        /// </example>		
        /// <param name="serializableObject">Serializable object to be saved to file.</param>
        /// <param name="fileName">Name of the file in the isolated storage area to save the object to.</param>
        /// <param name="isolatedStorageDirectory">Isolated storage area directory containing the XML file to save the object to.</param>
        /// <param name="extraTypes">Extra data types to enable serialization of custom types within the object.</param>
        public static void Save(T serializableObject, string fileName, IsolatedStorageFile isolatedStorageDirectory, System.Type[] extraTypes)
        {
            SaveToDocumentFormat(serializableObject, null, fileName, isolatedStorageDirectory);
        }

        #endregion

        #region Private

        private static FileStream CreateFileStream(IsolatedStorageFile isolatedStorageFolder, string path)
        {
            FileStream fileStream = null;

            if (isolatedStorageFolder == null)
                fileStream = new FileStream(path, FileMode.OpenOrCreate);
            else
                fileStream = new IsolatedStorageFileStream(path, FileMode.OpenOrCreate, isolatedStorageFolder);

            return fileStream;
        }

        private static T LoadFromBinaryFormat(string path, IsolatedStorageFile isolatedStorageFolder)
        {
            T serializableObject = null;

            using (FileStream fileStream = CreateFileStream(isolatedStorageFolder, path))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                serializableObject = binaryFormatter.Deserialize(fileStream) as T;
            }

            return serializableObject;
        }

        private static T LoadFromDocumentFormat(System.Type[] extraTypes, string path, IsolatedStorageFile isolatedStorageFolder)
        {
            T serializableObject = null;

            using (TextReader textReader = CreateTextReader(isolatedStorageFolder, path))
            {
                XmlSerializer xmlSerializer = CreateXmlSerializer(extraTypes);
                serializableObject = xmlSerializer.Deserialize(textReader) as T;

            }

            return serializableObject;
        }

        private static T LoadFromJSONFormat(System.Type[] extraTypes, string path, IsolatedStorageFile isolatedStorageFolder)
        {
            T serializableObject = null;

            using (TextReader textReader = CreateTextReader(isolatedStorageFolder, path))
            {
                DataContractJsonSerializer JSONSerializer = CreateJSONSerializer(extraTypes);

                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(textReader.ReadToEnd()));
                serializableObject = JSONSerializer.ReadObject(ms) as T;
            }

            return serializableObject;
        }

        private static TextReader CreateTextReader(IsolatedStorageFile isolatedStorageFolder, string path)
        {
            TextReader textReader = null;

            if (isolatedStorageFolder == null)
                textReader = new StreamReader(path);
            else
                textReader = new StreamReader(new IsolatedStorageFileStream(path, FileMode.Open, isolatedStorageFolder));

            return textReader;
        }

        private static TextWriter CreateTextWriter(IsolatedStorageFile isolatedStorageFolder, string path)
        {
            TextWriter textWriter = null;

            if (isolatedStorageFolder == null)
                textWriter = new StreamWriter(path);
            else
                textWriter = new StreamWriter(new IsolatedStorageFileStream(path, FileMode.OpenOrCreate, isolatedStorageFolder));

            return textWriter;
        }

        private static XmlSerializer CreateXmlSerializer(System.Type[] extraTypes)
        {
            Type ObjectType = typeof(T);

            XmlSerializer xmlSerializer = null;

            if (extraTypes != null)
                xmlSerializer = new XmlSerializer(ObjectType, extraTypes);
            else
                xmlSerializer = new XmlSerializer(ObjectType);

            return xmlSerializer;
        }

        private static DataContractJsonSerializer CreateJSONSerializer(System.Type[] extraTypes)
        {
            Type ObjectType = typeof(T);

            DataContractJsonSerializer JSONSerializer = null;

            if (extraTypes != null)
                JSONSerializer = new DataContractJsonSerializer(ObjectType, extraTypes);
            else
                JSONSerializer = new DataContractJsonSerializer(ObjectType);

            return JSONSerializer;
        }

        private static void SaveToJSONFormat(T serializableObject, System.Type[] extraTypes, string path, IsolatedStorageFile isolatedStorageFolder)
        {
            using (TextWriter textWriter = CreateTextWriter(isolatedStorageFolder, path))
            {
                DataContractJsonSerializer JSONSerializer = CreateJSONSerializer(extraTypes);
                MemoryStream ms = new MemoryStream();
                JSONSerializer.WriteObject(ms, serializableObject);
                string jsonString = Encoding.UTF8.GetString(ms.ToArray());
                ms.Close();
                textWriter.Write(JsonTools.Format(jsonString, "\t"));
            }
        }

        private static void SaveToDocumentFormat(T serializableObject, System.Type[] extraTypes, string path, IsolatedStorageFile isolatedStorageFolder)
        {
            using (TextWriter textWriter = CreateTextWriter(isolatedStorageFolder, path))
            {
                XmlSerializer xmlSerializer = CreateXmlSerializer(extraTypes);
                xmlSerializer.Serialize(textWriter, serializableObject);
            }
        }

        private static void SaveToBinaryFormat(T serializableObject, string path, IsolatedStorageFile isolatedStorageFolder)
        {
            using (FileStream fileStream = CreateFileStream(isolatedStorageFolder, path))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(fileStream, serializableObject);
            }
        }

        #endregion
    }



    public class JsonTools
    {
        public static string Format(string json, string fillStringUnit)
        {
            if (json == null || json.Trim().Length == 0)
            {
                return null;
            }

            int fixedLenth = 0;
            List<string> tokenList = new List<string>();
            {
                string jsonTemp = json;
                //Ô¤¶ÁÈ¡ 
                while (jsonTemp.Length > 0)
                {
                    string token = getToken(jsonTemp);
                    jsonTemp = jsonTemp.Substring(token.Length);
                    token = token.Trim();
                    tokenList.Add(token);
                }
            }

            for (int i = 0; i < tokenList.Count; i++)
            {
                string token = tokenList[i];
                int length = token.ToCharArray().Length;
                if (length > fixedLenth && i < tokenList.Count - 1 && tokenList[i + 1].Equals(":"))
                {
                    fixedLenth = length;
                }
            }

            StringBuilder buf = new StringBuilder();
            int count = 0;
            for (int i = 0; i < tokenList.Count; i++)
            {

                string token = tokenList[i];

                if (token.Equals(","))
                {
                    buf.Append(token);
                    doFill(buf, count, fillStringUnit);
                    continue;
                }
                if (token.Equals(":"))
                {
                    buf.Append(" ").Append(token).Append(" ");
                    continue;
                }
                if (token.Equals("{"))
                {
                    string nextToken = tokenList[i + 1];
                    if (nextToken.Equals("}"))
                    {
                        i++;
                        buf.Append("{ }");
                    }
                    else
                    {
                        count++;
                        buf.Append(token);
                        doFill(buf, count, fillStringUnit);
                    }
                    continue;
                }
                if (token.Equals("}"))
                {
                    count--;
                    doFill(buf, count, fillStringUnit);
                    buf.Append(token);
                    continue;
                }
                if (token.Equals("["))
                {
                    string nextToken = tokenList[i + 1];
                    if (nextToken.Equals("]"))
                    {
                        i++;
                        buf.Append("[ ]");
                    }
                    else
                    {
                        count++;
                        buf.Append(token);
                        doFill(buf, count, fillStringUnit);
                    }
                    continue;
                }
                if (token.Equals("]"))
                {
                    count--;
                    doFill(buf, count, fillStringUnit);
                    buf.Append(token);
                    continue;
                }

                buf.Append(token);
                //×ó¶ÔÆë 
                //if (i < tokenList.Count - 1 && tokenList[i + 1].Equals(":"))
                //{
                //    int fillLength = fixedLenth - token.ToCharArray().Length;
                //    if (fillLength > 0)
                //    {
                //        for (int j = 0; j < fillLength; j++)
                //        {
                //            buf.Append(" ");
                //        }
                //    }
                //}
            }
            return buf.ToString();
        }

        private static string getToken(string json)
        {
            StringBuilder buf = new StringBuilder();
            bool isInYinHao = false;
            while (json.Length > 0)
            {
                string token = json.Substring(0, 1);
                json = json.Substring(1);

                if (!isInYinHao &&
                        (token.Equals(":") || token.Equals("{") || token.Equals("}")
                                || token.Equals("[") || token.Equals("]")
                                || token.Equals(",")))
                {
                    if (buf.ToString().Trim().Length == 0)
                    {
                        buf.Append(token);
                    }

                    break;
                }

                if (token.Equals("\\"))
                {
                    buf.Append(token);
                    buf.Append(json.Substring(0, 1));
                    json = json.Substring(1);
                    continue;
                }
                if (token.Equals("\""))
                {
                    buf.Append(token);
                    if (isInYinHao)
                    {
                        break;
                    }
                    else
                    {
                        isInYinHao = true;
                        continue;
                    }
                }
                buf.Append(token);
            }
            return buf.ToString();
        }

        private static void doFill(StringBuilder buf, int count, string fillStringUnit)
        {
            buf.Append("\n");
            for (int i = 0; i < count; i++)
            {
                buf.Append(fillStringUnit);
            }
        }

    } 
}