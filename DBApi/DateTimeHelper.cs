using System;
using System.Collections.Generic;
using System.Linq;

namespace DBApi
{
    public static class DateTimeHelper
    {
        public static DateTime ConvertStringToDatetime(this string dateTime, string format = "dd/MM/yyyy HH:mm")
        {
            try
            {
                return Convert.ToDateTime(dateTime);
            } catch
            {
                try
                {
                    return DateTime.ParseExact(dateTime, format, System.Globalization.CultureInfo.InvariantCulture);
                } catch
                {
                    return DateTime.MinValue;
                }
            }
        }

        public static DateTime Max(DateTime dt1, DateTime dt2)
        {
            return (dt1 > dt2) ? dt1 : dt2;
        }
        public static DateTime Min(DateTime dt1, DateTime dt2)
        {
            return (dt1 > dt2) ? dt2 : dt1;
        }

        public static DateTime AddCalendarDays(this DateTime current, int days)
        {
            current = current.AddDays(days);
            
            return new DateTime(current.Year, current.Month, current.Day, current.Hour, current.Minute, current.Second);
        }

        public static DateTime AddBusinessDays(this DateTime current, int days, IEnumerable<DateTime>? holidays = null)
        {
            var sign = Math.Sign(days);
            var unsignedDays = Math.Abs(days);
            var dateTimes = holidays as DateTime[] ?? holidays.ToArray();
            for (var i = 0; i < unsignedDays; i++)
            {
                do
                {
                    current = current.AddDays(sign);
                } while (
                    current.DayOfWeek == DayOfWeek.Saturday ||
                    current.DayOfWeek == DayOfWeek.Sunday ||
                    (dateTimes.Contains(current.Date)));
            }
            return new DateTime(current.Year, current.Month, current.Day, 17,0,0);
        }
        public static DateTime SubtractBusinessDays(this DateTime current, int days, IEnumerable<DateTime>? holidays = null)
        {
            return AddBusinessDays(current, -days, holidays);
        }

        public static DateTime GetOrthodoxEaster(int year)
        {
            int a = year % 19;
            int b = year % 7;
            int c = year % 4;

            int d = (19 * a + 16) % 30;
            int e = (2 * c + 4 * b + 6 * d) % 7;
            int f = (19 * a + 16) % 30;
            int key = f + e + 3;

            int month = (key > 30) ? 5 : 4;
            int day = (key > 30) ? key - 30 : key;

            return new DateTime(year, month, day);
        }

        public static IEnumerable<DateTime> GetHolidays(IEnumerable<int> years)
        {
            var list = new List<DateTime>();
            foreach (var year in years)
            {
                list.Add(new DateTime(year, 1, 1)); //Πρωτοχρονιά
                list.Add(new DateTime(year, 1, 6)); //Θεοφάνεια
                list.Add(new DateTime(year, 3, 25)); // Ευαγγελισμός
                list.Add(new DateTime(year, 5, 1)); // Απεργία
                list.Add(new DateTime(year, 8, 15)); // Κοίμηση
                list.Add(new DateTime(year, 10, 28)); // ΟΧΙΕ
                list.Add(new DateTime(year, 12, 25)); // Χριστούγεννα
                list.Add(new DateTime(year, 12, 26)); // 2η Χριστουγέννων

                var easterDate = GetOrthodoxEaster(year);
                list.Add(easterDate.AddDays(1)); //Δευτέρα του πάσχα
                list.Add(easterDate.AddDays(-2)); //Μεγάλη παρασκευή
                list.Add(easterDate.AddDays(50)); //Πεντηκοστή
                list.Add(easterDate.AddDays(-48)); //Καθαρά δευτέρα
                    
            }
            return list;
        }

        public static IEnumerable<int> GetYears(DateTime dt, int days)
        {
            List<int> years = new List<int>();
            DateTime end = dt.AddDays(days);
            for (int i = dt.Year; i <= end.Year; i++)
            {
                years.Add(i);
            }
            return years;
        }

        public static int MinutesToDays(int minutes)
        {
            return (minutes / 1440);
        }
    }
}
