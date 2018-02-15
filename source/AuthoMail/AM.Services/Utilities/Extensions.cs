using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Entities.Utilities
{
    public static class Extensions
    {

        #region String
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return String.IsNullOrWhiteSpace(value);
        }

        public static bool IsNullOrWhiteSpaceOr(this string s, string value)
        {
            return String.IsNullOrWhiteSpace(s) || (s == value);
        }
        public static string ToTitleCase(this string value)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
        }

        public static string Fill(this string format, params object[] args)
        {
            return String.Format(format, args);
        }
        #endregion String
    }
}
