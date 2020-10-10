using System;
using System.Web;

namespace VA.Framework.Common.Extensions
{
    public static class StringExtensions
    {


        /// <summary>
        /// Removes CRLF characters and encodes HTML.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToSafeString(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            return HttpUtility.HtmlEncode(value.Replace("\r", string.Empty).Replace("\n", string.Empty));
        }


        public static DateTime? TryParseDate(this string stDate)
        {
            DateTime dtResult;
            try
            {
                dtResult = DateTime.FromOADate(double.Parse(stDate));
                return dtResult;
            }
            catch (Exception)
            {
                if (DateTime.TryParse(stDate, out dtResult))
                    return dtResult;
            }

            return null;
        }





    }
}
