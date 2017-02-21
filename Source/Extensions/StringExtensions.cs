using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Core.Extensions
{
    public static class StringExtensions
    {
        public static bool ToBoolean(this string source, bool def = default(bool))
        {
            bool result;
            return bool.TryParse(source, out result) ? result : def;
        }

        public static int ToInt(this string source, int def = default(int))
        {
            int result;
            return int.TryParse(source, out result) ? result : def;
        }

        public static bool IsNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }

        public static bool IsNullOrWhiteSpace(this string source)
        {
            return string.IsNullOrWhiteSpace(source);
        }

        public static T DeserializeJson<T>(this string source)
        {
            if (source.IsNullOrEmpty())
                return default(T);

            JsonSerializerSettings jss = new JsonSerializerSettings();
            jss.MissingMemberHandling = MissingMemberHandling.Ignore;

            return JsonConvert.DeserializeObject<T>(source, jss);
        }

        public static string SerializeToJson<T>(this T source)
        {
            if (source == null)
                return null;

            JsonSerializerSettings jss = new JsonSerializerSettings();
            jss.MissingMemberHandling = MissingMemberHandling.Ignore;

            return JsonConvert.SerializeObject(source, jss);
        }

        public static T DeserializeXml<T>(this string source, Encoding encoding = null) where T : class
        {
            if (source.IsNullOrEmpty())
                return default(T);

            if (encoding == null)
                encoding = Encoding.UTF8;

            T result = default(T);

            XmlSerializer serializer = new XmlSerializer(typeof(T), new XmlAttributeOverrides());
            try
            {
                using (var ms = new MemoryStream(encoding.GetBytes(source)))
                {
                    using (var reader = new StreamReader(ms, encoding))
                    {
                        using (var xmlReader = XmlReader.Create(reader))
                        {
                            result = serializer.Deserialize(xmlReader) as T;
                        }
                    }
                }

            }
            catch (Exception)
            {
            }

            return result;
        }

        public static string SerializeToXml<T>(this T source, Encoding encoding = null)
        {
            if (source == null)
                return null;

            if (encoding == null)
                encoding = Encoding.UTF8;

            XmlSerializer serializer = new XmlSerializer(typeof(T));

            try
            {
                StringWriter writer = new StringWriterWithEncoding(encoding);

                using (var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings() { Encoding = encoding }))
                {
                    serializer.Serialize(xmlWriter, source);
                }

                return writer.GetStringBuilder().ToString();
            }
            catch (Exception)
            {
            }

            return String.Empty;
        }
    }

    public sealed class StringWriterWithEncoding : StringWriter
    {
        private readonly Encoding _encoding;

        public StringWriterWithEncoding(Encoding encoding)
        {
            this._encoding = encoding;
        }

        public override Encoding Encoding
        {
            get { return _encoding; }
        }
    }
}
