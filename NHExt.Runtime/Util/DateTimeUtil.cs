using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.Util
{
    public static class DateTimeUtil
    {
        private static readonly int FirsWeektDay = 1;

        private static DateTime BaseDate()
        {
            DateTime result = DateTime.Parse("1900-01-01").AddDays(DateTimeUtil.FirsWeektDay - 1);
            return result;
        }
        public static DateTime FirstDayOfWeek(DateTime dtDate)
        {
            DateTime dt = DateTimeUtil.BaseDate();
            TimeSpan ts = dtDate - dt;
            return dt.AddDays(ts.Days / 7 * 7);
        }
        public static DateTime LastDayOfWeek(DateTime dtDate)
        {
            DateTime dt = DateTimeUtil.BaseDate();
            TimeSpan ts = dtDate - dt;
            return dt.AddDays(ts.Days / 7 * 7 + 6);
        }
        public static int WeekNumberOfYear(DateTime dtDate)
        {
            dtDate = dtDate.Date;
            DateTime dt = DateTimeUtil.FirstDayOfWeek(DateTime.Parse(dtDate.Year + "-01-01"));
            TimeSpan ts = dtDate - dt;
            return ts.Days / 7;
        }
        public static int QuarterOfYear(DateTime dtDate) {
            int month = dtDate.Month;
            if (month % 3 == 0)
            {
                return month / 3;
            }
            else {
                return month / 3 + 1;
            }
        }
        /// <summary>
        /// 判断是否是闰年
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public static bool LeapYear(int year)
        {
            if (year % 100 == 0)
            {
                if (year % 400 == 0) return true;
            }
            else {
                if (year % 4 == 0) return true;
            }
            return false;
        }
        public static int NumberOfWeek(DateTime dtDate) {
            DateTime dt = DateTimeUtil.FirstDayOfWeek(dtDate);
            TimeSpan ts = dtDate - dt;
            return ts.Days + 1;
        }
        public static int DaysDiffer(DateTime begin, DateTime end) {
            TimeSpan span = end - begin;
            return span.Days;
        }
    }
}
