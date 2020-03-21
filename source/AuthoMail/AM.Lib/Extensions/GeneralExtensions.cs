using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace AM.Lib
{
    public static class GeneralExtensions
    {
        public static bool IsNullOrEmpty(this string value)
        {
            return String.IsNullOrEmpty(value);
        }

        public static bool IsNullOrWhiteSpace(this string value)
        {
            return String.IsNullOrWhiteSpace(value);
        }

        public static string TrimIfNotNull(this string value)
        {
            return !value.IsNullOrWhiteSpace() ? value.Trim() : value;
        }

        public static string ToTitleCase(this string value)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
        }

        public static string ToCsvString(this string value)
        {
            return "\"{0}\"".Fill(value ?? "");
        }

        public static string Fill(this string format, params object[] args)
        {
            return String.Format(format, args);
        }

        public static void CreateDirectoryIfNotExist(this string dirPath)
        {
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
        }

        public static void CleanDirectory(this string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);
        }

        public static T ParseEnum<T>(this string value)
        {
            return value.ParseEnum<T>(false);
        }

        public static T ParseEnum<T>(this string value, bool ignoreCase)
        {
            return (T)Enum.Parse(typeof(T), value, ignoreCase);
        }


        public static T Deserialize<T>(this string xml) where T : new()
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

        public static string Serialize<T>(this T obj) where T : new()
        {
            return obj.Serialize(null);
        }

        public static string Serialize<T>(this T obj, XmlSerializerNamespaces namespaces) where T : new()
        {
            var serializer = new XmlSerializer(typeof(T));
            string xml = null;
            using (var stringWriter = new StringWriter())
            {
                serializer.Serialize(stringWriter, obj, namespaces);
                xml = stringWriter.ToString();
            }
            return xml;
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

        public static DateTime? AnyFormatToDateTime(this string date)
        {
            string[] formats = {"M/d/yyyy h:mm:ss tt", "M/d/yyyy h:mm tt",
"MM/dd/yyyy hh:mm:ss", "M/d/yyyy h:mm:ss","M/d/yyyy hh:mm tt", "M/d/yyyy hh tt","M/d/yyyy h:mm", "M/d/yyyy h:mm","MM/dd/yyyy hh:mm", "M/dd/yyyy hh:mm","dd-MMM-yyyy hh:mm:ss","MMMM     dd, yyyy","MMM-dd-yyyy hh:mm:ss","MMMM dd, yyyy","MMMM dd, yyyy hh:mm:ss"};

            DateTime dateValue;
            if (DateTime.TryParseExact(date, formats, new CultureInfo("en-US"), DateTimeStyles.None, out dateValue))
                return dateValue;
            else
                return null;
        }

        public static string RemoveCSVStringFormat(this string str)
        {
            return str.Replace("\"", "");
        }

        public static DateTime UtcToClientTimeZone(this DateTime dt, string clientTimeZone)
        {
            dt = DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTime(dt, TimeZoneInfo.FindSystemTimeZoneById("UTC"), TimeZoneInfo.FindSystemTimeZoneById(clientTimeZone));
        }

        public static DateTime ClientTimeZoneToUtc(this DateTime dt, string clientTimeZone)
        {
            dt = DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTime(dt, TimeZoneInfo.FindSystemTimeZoneById(clientTimeZone), TimeZoneInfo.FindSystemTimeZoneById("UTC"));
        }

        public static DateTime UtcToEst(this DateTime dt)
        {
            return dt.UtcToClientTimeZone("Eastern Standard Time");
        }

        public static DateTime EstToUtc(this DateTime dt)
        {
            return dt.ClientTimeZoneToUtc("Eastern Standard Time");
        }

        public static DateTime PstToUtc(this DateTime dt)
        {
            return dt.ClientTimeZoneToUtc("Pacific Standard Time");
        }

        public static DateTime ToStartDateTime(this DateTime dt)
        {
            return (dt.Date + new TimeSpan(00, 00, 00));
        }
        public static DateTime ToEndDateTime(this DateTime dt)
        {
            return (dt.Date + new TimeSpan(23, 59, 59));
        }

        public static string GetDescription(this PropertyInfo p)
        {
            var descAttr = p.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute;
            return descAttr == null ? null : descAttr.Description;
        }

        public static string RemoveFormatting(this string value)
        {
            return value.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "").Replace(",", "");
        }

        public static MethodInfo GetConversionMethod(this Type t)
        {
            MethodInfo Convert;

            switch (t.FullName)
            {
                case "System.TimeSpan":
                    Convert = typeof(TimeSpan).GetMethod("Parse", new Type[] { typeof(string) });
                    break;
                default:
                    Convert = typeof(Convert).GetMethod("To" + t.Name, new Type[] { typeof(object) });
                    break;
            }

            return Convert;
        }

        public static object GetValueOfPropertyFromPropertyName<T>(this T cls, string propertyName)
        {
            return (object)cls.GetType().GetProperty(propertyName).GetValue(cls, null);
        }

        public static string SeparateZip4And5(this string value, int number)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            string[] zipCode = value.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            if (zipCode.Length > 0)
            {
                foreach (string zip in zipCode)
                {
                    if (zip.Length == number)
                    {
                        return zip;
                    }
                    else if (zip.Length == number)
                    {
                        return zip;
                    }
                }
            }
            else
            {
                if (zipCode[0].Length == number)
                {
                    return zipCode[0];
                }
                else if (zipCode[0].Length == number)
                {
                    return zipCode[0];
                }
            }

            return "";
        }

        public static List<string> SplitCsv(this string csv)
        {
            var values = new List<string>();

            int last = -1;
            bool inQuotes = false;

            int n = 0;
            while (n < csv.Length)
            {
                switch (csv[n])
                {
                    case '"':
                        inQuotes = !inQuotes;
                        break;
                    case ',':
                        if (!inQuotes)
                        {
                            values.Add(csv.Substring(last + 1, (n - last)).Trim(' ', ','));
                            last = n;
                        }
                        break;
                }
                n++;
            }

            if (last != csv.Length - 1)
                values.Add(csv.Substring(last + 1).Trim());

            return values;
        }
    }
}
