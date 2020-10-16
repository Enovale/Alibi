using System;

namespace Alibi.Plugins.Webhook.Helpers
{
    public static class TimeSpanExtensions
    {
        public static string LargestIntervalWithUnits(this TimeSpan interval)
        {
            if (interval < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("interval");
            }

            if (interval == TimeSpan.Zero)
            {
                return "now";
            }

            int timeValue;
            string timeUnits;

            if (interval.TotalHours > 22.0)
            {
                timeValue = (int)Math.Ceiling(interval.TotalDays);
                timeUnits = " day";
            }
            else if (interval.TotalMinutes > 50.0)
            {
                timeValue = (int)Math.Ceiling(interval.TotalHours);
                timeUnits = " hour";
            }
            else if (interval.TotalSeconds > 40.0)
            {
                timeValue = (int)Math.Ceiling(interval.TotalMinutes);
                timeUnits = " minute";
            }
            else if (interval.TotalMilliseconds > 500.0)
            {
                timeValue = (int)Math.Ceiling(interval.TotalSeconds);
                timeUnits = " second";
            }
            else
            {
                timeValue = (int)Math.Ceiling(interval.TotalMilliseconds);
                timeUnits = " millisecond";
            }

            return string.Format("{0:#,##0}{1}{2}",
                timeValue,
                timeUnits,
                (timeValue == 1 ? string.Empty : "s"));
        }
    }
}