using System;
using System.Data;

namespace VA.Framework.Common.Extensions
{
    public static class DataRowExtensions
    {
        public static string GetValue(this DataRow row, string columnName, bool emptyIfNotFound = false)
        {
            try
            {
                var value = row[columnName];
                if (value != null)
                    return value.ToString().Trim();
                else
                    throw new ArgumentException($"Unable to find columnName '{columnName}' from row");
            }
            catch (Exception)
            {
                if (emptyIfNotFound)
                    return string.Empty;
                else
                    throw;
            }
        }

        public static DateTime? GetDateTimeValue(this DataRow row, string columnName)
        {

            var value = row[columnName];
            if (value != null)
                return DateTime.Parse(value.ToString().Trim());
            else
                return null;
        }

        public static int? GetIntValue(this DataRow row, string columnName)
        {

            var value = row[columnName];
            if (value != null)
                return int.Parse(value.ToString().Trim());
            else
                return null;
        }

        public static decimal? GetDecimalValue(this DataRow row, string columnName)
        {

            var value = row[columnName];
            if (value != null)
                return decimal.Parse(value.ToString().Trim());
            else
                return null;
        }




    }
}
