using System.Collections.Generic;
using System.Linq;

namespace VA.Framework.Common.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> value) => (value == null || !value.Any());

        public static bool AnySafe<T>(this IEnumerable<T> value) => (value != null && value.Any());

    }
}
