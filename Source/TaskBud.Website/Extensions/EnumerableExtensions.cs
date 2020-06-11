using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TaskBud.Website.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<SelectListItem> SelectList<T1, T2, T3>(this IEnumerable<T1> self,
            Expression<Func<T1, T2>> value, Expression<Func<T1, T2>> text, Expression<Func<T1, T3>> groupBy)
        {
            if (!value.IsRequired())
            {
                yield return new SelectListItem("None", null);
            }

            var valueSelector = value.Compile();
            var textSelector = text.Compile();
            var groupSelector = groupBy.Compile();

            foreach (var group in self.GroupBy(groupSelector))
            {
                var selectGroup = new SelectListGroup
                {
                    Name = group.Key?.ToString() ?? ""
                };

                foreach (var item in group)
                {
                    yield return new SelectListItem
                    {
                        Value = valueSelector(item)?.ToString() ?? "",
                        Text = textSelector(item)?.ToString() ?? "",
                        Group = selectGroup
                    };
                }
            }
        }

        public static IEnumerable<SelectListItem> SelectList<T1, T2, T3>(this IEnumerable<T1> self, Expression<Func<T1, T2>> value, Expression<Func<T1, T3>> text)
        {
            if (!value.IsRequired())
            {
                yield return new SelectListItem("None", "");
            }

            var valueSelector = value.Compile();
            var textSelector = text.Compile();

            foreach (var item in self)
            {
                yield return new SelectListItem
                {
                    Value = valueSelector(item)?.ToString() ?? "",
                    Text = textSelector(item)?.ToString() ?? "",
                };
            }
        }

        public static IEnumerable<SelectListItem> SelectList<T>(this IEnumerable<T> self, Expression<Func<T, object>> value)
        {
            return self.SelectList(value, value);
        }

    }
}
