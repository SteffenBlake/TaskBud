using System;

namespace TaskBud.Business.Extensions
{
    public static class DateTimeOffsetExtensions
    {
        public static string ToHumanReadable(this DateTimeOffset self)
        {
            var now = DateTimeOffset.Now;

            // 12:00 PM
            if (self.Date == now.Date)
            {
                return self.ToString("t");
            }

            // Yesterday
            if (self.Date.AddDays(-1) == now.Date)
            {
                return "Tomorrow";
            }

            // Tomorrow
            if (self.Date.AddDays(1) == now.Date)
            {
                return "Yesterday";
            }

            // March 2, 2022
            if (self.Date.Year != now.Year)
            {
                return self.ToString("MMMM dd, yyyy");
            }

            // April 8
            return self.ToString("M");
        }


    }
}
