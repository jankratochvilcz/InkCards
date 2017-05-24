using System;
using System.Collections.Generic;
using System.Linq;

namespace InkCards.Infrastructure.Extensions
{
    public static class ListExtensions
    {
        public static T GetPreviousItemToSelect<T>(this IList<T> list, int originalIndex) where T : class
        {
            if (originalIndex > 0) return list[originalIndex - 1];
            if (list.Any()) return list[originalIndex];

            return null;
        }

        // http://stackoverflow.com/questions/4134366/math-stats-with-linq
        public static double? Median<TColl, TValue>(
            this IEnumerable<TColl> source,
            Func<TColl, TValue> selector)
            => source.Select(selector).Median();

        public static double? Median<T>(
            this IEnumerable<T> source)
        {
            source = source.NotNull();

            if (!source.Any()) return null;

            int count = source.Count();

            if (count == 0)
                return null;

            source = source.OrderBy(x => x);

            int midpoint = count / 2;
            if (count % 2 == 0)
                return (Convert.ToDouble(source.ElementAt(midpoint - 1)) + Convert.ToDouble(source.ElementAt(midpoint))) / 2.0;
            else
                return Convert.ToDouble(source.ElementAt(midpoint));
        }

        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> source)
            => Nullable.GetUnderlyingType(typeof(T)) != null
                ? source.Where(x => x != null)
                : source;
    }
}
