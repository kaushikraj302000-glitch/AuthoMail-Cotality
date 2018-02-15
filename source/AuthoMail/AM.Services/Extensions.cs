using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AM.Services
{
    public static class Extensions
    {
        public static bool IsNullOrEmpty(this string value)
        {
            return String.IsNullOrEmpty(value);
        }

        public static bool IsNullOrWhiteSpace(this string value)
        {
            return String.IsNullOrWhiteSpace(value);
        }
        public static string SqlEscape(this string sample)
        {
            if (string.IsNullOrWhiteSpace(sample))
            {
                return "";
            }
            else
            {
                return Regex.Replace(sample, @"'", @" ");
            }
        }

        public static string ToTitleCase(this string value)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
        }

        public static string Fill(this string format, params object[] args)
        {
            return String.Format(format, args);
        }

        public static T ParseEnum<T>(this string value)
        {
            return value.ParseEnum<T>(false);
        }

        public static T ParseEnum<T>(this string value, bool ignoreCase)
        {
            return (T)Enum.Parse(typeof(T), value, ignoreCase);
        }

        public static T Deserialize<T>(string xml) where T : new()
        {
            T entity;
            var xmlSerializer = new XmlSerializer(typeof(T));

            //var attributes = typeof(T).GetCustomAttributes(typeof(SerializableAttribute), true);

            //if (attributes.Length == 0)
            //    throw new ArgumentException("T does not have attribute InsertableAttribute");

            using (var sr = new StringReader(xml))
            {
                entity = (T)xmlSerializer.Deserialize(sr);
                sr.Close();
            }

            return entity;
        }

        public static string Serialize<T>(T entity) where T : new()
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            var sb = new StringBuilder();

            using (var sw = new StringWriter(sb))
            {
                xmlSerializer.Serialize(sw, entity);
                sw.Close();
            }

            return sb.ToString();
        }

        public static string GetValidFileName(this string value)
        {
            if (!value.IsNullOrWhiteSpace())
            {
                value = value.Replace(Environment.NewLine, "_");
            }
            foreach (var ch in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(ch.ToString(), "");
            }
            return value.Length > 160 ? value.Substring(0, 160) : value;
        }
        public static bool IsValidForFileName(this string value)
        {
            return !value.ToCharArray().Any(ch => Path.GetInvalidFileNameChars().Contains(ch));
            //return !Regex.IsMatch(value, Global.FileNameRegex);
        }
    }
}
