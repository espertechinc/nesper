///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat.datetime;

namespace com.espertech.esper.compat
{
    /// <summary>
    ///     DateTime with offset and timezone tracking.  When math operations are performed against this
    ///     structure, they can take into account the timezone the date was associated with.
    /// </summary>
    public class DateTimeEx
        : IComparable<DateTimeEx>,
            IComparable
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DateTimeEx" /> class.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="timeZone">The time zone.</param>
        public DateTimeEx(
            DateTimeOffset dateTime,
            TimeZoneInfo timeZone)
        {
            var dateTimeUTC = dateTime.UtcDateTime;
            var dateTimeOffset = timeZone.GetUtcOffset(dateTimeUTC);
            DateTime = new DateTimeOffset(dateTimeUTC).ToOffset(dateTimeOffset);
            TimeZone = timeZone;
        }

        public DateTimeEx(
            int year,
            int month,
            int day,
            int hour,
            int minute,
            int second,
            int millis,
            TimeZoneInfo timeZoneInfo)
        {
            var dateTimeReference = new DateTime(year, month, day, hour, minute, second, millis);
            var dateTimeUtcOffset = timeZoneInfo.GetUtcOffset(dateTimeReference);
            DateTime = new DateTimeOffset(year, month, day, hour, minute, second, millis, dateTimeUtcOffset);
            TimeZone = timeZoneInfo;
        }

        public DateTimeEx(DateTimeEx source)
        {
            DateTime = source.DateTime;
            TimeZone = source.TimeZone;
        }

        public int Year => DateTime.Year;

        public int Month => DateTime.Month;

        public int Day => DateTime.Day;

        public int Hour => DateTime.Hour;

        public int Minute => DateTime.Minute;

        public int Second => DateTime.Second;

        public int Millisecond => DateTime.Millisecond;

        public DayOfWeek DayOfWeekEnum => DateTime.DayOfWeek;

        public int DayOfWeek => (int) DateTime.DayOfWeek;

        public long UtcMillis => DateTime.UtcMillis();

        public int DayOfYear => DateTime.DateTime.DayOfYear;

        public int WeekOfYear => DateTime.GetWeekOfYear();

        /// <summary>
        ///     Gets the underlying date time object.
        /// </summary>
        /// <value>
        ///     The date time.
        /// </value>
        public DateTimeOffset DateTime { get; private set; }

        /// <summary>
        ///     Gets the underlying date time object, translated to UTC.
        /// </summary>
        /// <value>
        ///     The UTC date time.
        /// </value>
        public DateTimeOffset UtcDateTime => DateTime.TranslateTo(TimeZoneInfo.Utc);

        /// <summary>
        ///     Gets the time zone associated with the value.
        /// </summary>
        /// <value>
        ///     The time zone.
        /// </value>
        public TimeZoneInfo TimeZone { get; }

        /// <summary>
        ///     Compares the current instance with another object of the same type and
        ///     returns an integer that indicates whether the current instance precedes,
        ///     follows, or occurs in the same position in the sort order as the other
        ///     object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        ///     A value that indicates the relative order of the objects being compared. The return value has these meanings: Value
        ///     Meaning Less than zero This instance precedes <paramref name="obj" /> in the sort order. Zero This instance occurs
        ///     in the same position in the sort order as <paramref name="obj" />. Greater than zero This instance follows
        ///     <paramref name="obj" /> in the sort order.
        /// </returns>
        public int CompareTo(object obj)
        {
            var otherDateTime = obj as DateTimeEx;
            if (otherDateTime == null) {
                throw new ArgumentException("invalid value", nameof(obj));
            }

            return CompareTo(otherDateTime);
        }

        /// <summary>
        ///     Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///     A value that indicates the relative order of the objects being compared.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public int CompareTo(DateTimeEx other)
        {
            return DateTime.CompareTo(other.DateTime);
        }

        public DateTimeEx Clone()
        {
            return new DateTimeEx(this);
        }

        public DateTimeEx SetUtcMillis(long millis)
        {
            DateTime = millis.TimeFromMillis(TimeZone);
            return this;
        }

        public DateTimeEx Set(DateTimeOffset dateTime)
        {
            var dateTimeOffset = TimeZone.GetUtcOffset(dateTime);
            DateTime = new DateTimeOffset(dateTime.DateTime, dateTimeOffset);
            return this;
        }

        public DateTimeEx Set(DateTime dateTime)
        {
            return SetUtcMillis(dateTime.UtcMillis());
        }

        public DateTimeEx Set(
            int year,
            int month,
            int day,
            int hour = 0,
            int minute = 0,
            int second = 0,
            int millisecond = 0)
        {
            DateTime = DateTimeOffsetHelper.CreateDateTime(
                year,
                month,
                day,
                hour,
                minute,
                second,
                millisecond,
                TimeZone);
            return this;
        }

        public DateTimeEx SetYear(int year)
        {
            DateTime = DateTimeOffsetHelper.CreateDateTime(
                year,
                DateTime.Month,
                DateTime.Day,
                DateTime.Hour,
                DateTime.Minute,
                DateTime.Second,
                DateTime.Millisecond,
                TimeZone);
            return this;
        }

        public DateTimeEx SetMonth(int month)
        {
            DateTime = DateTimeOffsetHelper.CreateDateTime(
                DateTime.Year,
                month,
                DateTime.Day,
                DateTime.Hour,
                DateTime.Minute,
                DateTime.Second,
                DateTime.Millisecond,
                TimeZone);
            return this;
        }

        public DateTimeEx SetDay(int day)
        {
            DateTime = DateTimeOffsetHelper.CreateDateTime(
                DateTime.Year,
                DateTime.Month,
                day,
                DateTime.Hour,
                DateTime.Minute,
                DateTime.Second,
                DateTime.Millisecond,
                TimeZone);
            return this;
        }

        public DateTimeEx SetHour(int hour)
        {
            DateTime = DateTimeOffsetHelper.CreateDateTime(
                DateTime.Year,
                DateTime.Month,
                DateTime.Day,
                hour,
                DateTime.Minute,
                DateTime.Second,
                DateTime.Millisecond,
                TimeZone);
            return this;
        }

        public DateTimeEx SetMinute(int minute)
        {
            DateTime = DateTimeOffsetHelper.CreateDateTime(
                DateTime.Year,
                DateTime.Month,
                DateTime.Day,
                DateTime.Hour,
                minute,
                DateTime.Second,
                DateTime.Millisecond,
                TimeZone);
            return this;
        }

        public DateTimeEx SetSecond(int second)
        {
            DateTime = DateTimeOffsetHelper.CreateDateTime(
                DateTime.Year,
                DateTime.Month,
                DateTime.Day,
                DateTime.Hour,
                DateTime.Minute,
                second,
                DateTime.Millisecond,
                TimeZone);
            return this;
        }

        public DateTimeEx SetMillis(int millis)
        {
            DateTime = DateTimeOffsetHelper.CreateDateTime(
                DateTime.Year,
                DateTime.Month,
                DateTime.Day,
                DateTime.Hour,
                DateTime.Minute,
                DateTime.Second,
                millis,
                TimeZone);
            return this;
        }

        protected bool Equals(DateTimeEx other)
        {
            return DateTime.Equals(other.DateTime) && Equals(TimeZone, other.TimeZone);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((DateTimeEx) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                return (DateTime.GetHashCode() * 397) ^ (TimeZone != null ? TimeZone.GetHashCode() : 0);
            }
        }

        public string ToString(TimeZoneInfo timeZone)
        {
            if (timeZone != null) {
                var offset = timeZone.GetUtcOffset(DateTime);
                return string.Format("{0} [TZ = {1}]", DateTime.ToOffset(offset), TimeZone);
            }

            return string.Format("{0} [TZ = {1}]", DateTime, TimeZone);
        }

        public override string ToString()
        {
            return ToString(TimeZoneInfo.Utc);
        }

        public static bool operator >(
            DateTimeEx d1,
            DateTimeEx d2)
        {
            return d1.DateTime > d2.DateTime;
        }

        public static bool operator >=(
            DateTimeEx d1,
            DateTimeEx d2)
        {
            return d1.DateTime >= d2.DateTime;
        }

        public static bool operator <(
            DateTimeEx d1,
            DateTimeEx d2)
        {
            return d1.DateTime < d2.DateTime;
        }

        public static bool operator <=(
            DateTimeEx d1,
            DateTimeEx d2)
        {
            return d1.DateTime <= d2.DateTime;
        }

        public static bool operator ==(
            DateTimeEx d1,
            DateTimeEx d2)
        {
            if (ReferenceEquals(d1, null)) {
                return ReferenceEquals(d2, null);
            }

            if (ReferenceEquals(d2, null)) {
                return false;
            }

            return d1.DateTime == d2.DateTime;
        }

        public static bool operator !=(
            DateTimeEx d1,
            DateTimeEx d2)
        {
            if (ReferenceEquals(d1, null)) {
                return !ReferenceEquals(d2, null);
            }

            if (ReferenceEquals(d2, null)) {
                return true;
            }

            return d1.DateTime != d2.DateTime;
        }

        public DateTimeEx AddYears(
            int amount,
            DateTimeMathStyle style = DateTimeMathStyle.CLR)
        {
            DateTime = DateTime.AddYears(amount);
            if (style == DateTimeMathStyle.Java) {
                return Rebase();
            }

            return this;
        }

        public DateTimeEx AddMonths(
            int amount,
            DateTimeMathStyle style = DateTimeMathStyle.CLR)
        {
            if (style == DateTimeMathStyle.CLR) {
                DateTime = DateTime.AddMonths(amount);
            }
            else {
                DateTime = DateTime.AddMonthsLikeJava(amount);
                return Rebase();
            }

            return this;
        }

        public DateTimeEx AddDays(
            int amount,
            DateTimeMathStyle style = DateTimeMathStyle.CLR)
        {
            DateTime = DateTime.AddDays(amount);
            if (style == DateTimeMathStyle.Java) {
                return Rebase();
            }

            return this;
        }

        public DateTimeEx AddHours(
            int amount,
            DateTimeMathStyle style = DateTimeMathStyle.CLR)
        {
            DateTime = DateTime.AddHours(amount);
            if (style == DateTimeMathStyle.Java) {
                return Realign();
            }

            return this;
        }

        public DateTimeEx AddMinutes(
            int amount,
            DateTimeMathStyle style = DateTimeMathStyle.CLR)
        {
            DateTime = DateTime.AddMinutes(amount);
            if (style == DateTimeMathStyle.Java) {
                return Realign();
            }

            return this;
        }

        public DateTimeEx AddSeconds(
            int amount,
            DateTimeMathStyle style = DateTimeMathStyle.CLR)
        {
            DateTime = DateTime.AddSeconds(amount);
            if (style == DateTimeMathStyle.Java) {
                return Realign();
            }

            return this;
        }

        public DateTimeEx AddMilliseconds(
            int amount,
            DateTimeMathStyle style = DateTimeMathStyle.CLR)
        {
            DateTime = DateTime.AddMilliseconds(amount);
            if (style == DateTimeMathStyle.Java) {
                return Realign();
            }

            return this;
        }

        /// <summary>
        ///     Adds to fields in the datetime.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public DateTimeEx AddUsingField(
            DateTimeFieldEnum field,
            int value)
        {
            DateTime = DateTime.AddUsingField(field, value);
            return this;
        }

        /// <summary>
        /// Adds a timespan to the current time.
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        /// <returns></returns>
        public DateTimeEx AddTimeSpan(TimeSpan timeSpan)
        {
            DateTime += timeSpan;
            return this;
        }

        public int GetFieldValue(DateTimeFieldEnum field)
        {
            return DateTime.GetFieldValue(field);
        }

        public DateTimeEx SetFieldValue(
            DateTimeFieldEnum field,
            int value)
        {
            DateTime = DateTime.SetFieldValue(field, value, TimeZone);
            return this;
        }

        public int GetActualMinimum(DateTimeFieldEnum field)
        {
            switch (field) {
                case DateTimeFieldEnum.MILLISEC:
                case DateTimeFieldEnum.SECOND:
                case DateTimeFieldEnum.MINUTE:
                case DateTimeFieldEnum.HOUR:
                    return 0;

                case DateTimeFieldEnum.DAY:
                case DateTimeFieldEnum.DATE:
                case DateTimeFieldEnum.MONTH:
                    return 1;

                case DateTimeFieldEnum.YEAR:
                    return 0;

                default:
                    throw new NotSupportedException();
            }
        }

        public int GetActualMaximum(DateTimeFieldEnum field)
        {
            switch (field) {
                case DateTimeFieldEnum.MILLISEC:
                    return 999;

                case DateTimeFieldEnum.SECOND:
                    return 59;

                case DateTimeFieldEnum.MINUTE:
                    return 59;

                case DateTimeFieldEnum.HOUR:
                    return 23;

                case DateTimeFieldEnum.DAY:
                case DateTimeFieldEnum.DATE:
                    return System.DateTime.DaysInMonth(this.Year, this.Month);

                case DateTimeFieldEnum.MONTH:
                    return 12;

                case DateTimeFieldEnum.YEAR:
                    return int.MaxValue;

                default:
                    throw new NotSupportedException();
            }
        }

        public DateTimeEx SetMaximumDay()
        {
            DateTime = DateTime.GetWithMaximumDay(TimeZone);
            return Rebase();
        }

        public DateTimeEx SetMaximumMonth()
        {
            DateTime = DateTime.GetWithMaximumMonth(TimeZone);
            return Rebase();
        }

        public DateTimeEx Truncate(DateTimeFieldEnum field)
        {
            DateTime = DateTime.Truncate(field);
            return this;
        }

        public DateTimeEx Ceiling(DateTimeFieldEnum field)
        {
            DateTime = DateTime.Ceiling(field);
            return this;
        }

        public DateTimeEx Round(DateTimeFieldEnum field)
        {
            DateTime = DateTime.Round(field);
            return this;
        }
        
        private DateTimeEx Realign()
        {
            var offset = TimeZone.GetUtcOffset(DateTime);
            DateTime = DateTime.ToOffset(offset);
            return this;
        }

        private DateTimeEx Rebase()
        {
            DateTime = new DateTimeOffset(
                DateTime.Year,
                DateTime.Month,
                DateTime.Day,
                DateTime.Hour,
                DateTime.Minute,
                DateTime.Second,
                DateTime.Millisecond,
                TimeZone.GetUtcOffset(DateTime));
            return this;
        }

        public DateTimeEx SetMinimumWeek()
        {
            DateTime = DateTime.GetWithMinimumWeek(TimeZone);
            return Rebase();
        }

        public DateTimeEx SetMaximumWeek()
        {
            DateTime = DateTime.GetWithMaximumWeek(TimeZone);
            return Rebase();
        }

        public int GetActualMaximum()
        {
            return DateTime.GetActualMaximum(DateTimeFieldEnum.DAY_OF_MONTH);
        }

        public DateTimeEx MoveToWeek(int week)
        {
            DateTime = DateTime.MoveToWeek(week, TimeZone);
            return Rebase();
        }

        public static DateTimeEx NowUtc()
        {
            return GetInstance(TimeZoneInfo.Utc);
        }

        public static DateTimeEx NowLocal()
        {
            return GetInstance(TimeZoneInfo.Local);
        }

        public static DateTimeEx GetInstance(TimeZoneInfo timeZoneInfo)
        {
            return new DateTimeEx(
                DateTimeOffsetHelper.Now(timeZoneInfo),
                timeZoneInfo
            );
        }

        public static DateTimeEx GetInstance(
            TimeZoneInfo timeZoneInfo,
            DateTimeEx dtx)
        {
            return new DateTimeEx(dtx.DateTime, timeZoneInfo);
        }

        public static DateTimeEx GetInstance(
            TimeZoneInfo timeZoneInfo,
            DateTimeOffset dtoffset)
        {
            return new DateTimeEx(dtoffset, timeZoneInfo);
        }

        public static DateTimeEx GetInstance(
            TimeZoneInfo timeZoneInfo,
            DateTime dateTime)
        {
            var baseDt = dateTime.ToDateTimeOffset(timeZoneInfo);
            return new DateTimeEx(baseDt, timeZoneInfo);
        }

        public static DateTimeEx GetInstance(
            TimeZoneInfo timeZoneInfo,
            long timeInMillis)
        {
            var baseDt = timeInMillis.TimeFromMillis(timeZoneInfo);
            return new DateTimeEx(baseDt, timeZoneInfo);
        }

        //

        public static DateTimeEx LocalInstance(DateTimeEx dtx)
        {
            return GetInstance(TimeZoneInfo.Local, dtx);
        }

        public static DateTimeEx LocalInstance(DateTimeOffset dtoffset)
        {
            return GetInstance(TimeZoneInfo.Local, dtoffset);
        }

        public static DateTimeEx LocalInstance(DateTime dateTime)
        {
            return GetInstance(TimeZoneInfo.Local, dateTime);
        }

        public static DateTimeEx LocalInstance(long timeInMillis)
        {
            return GetInstance(TimeZoneInfo.Local, timeInMillis);
        }
        //

        public static DateTimeEx UtcInstance(DateTimeEx dtx)
        {
            return GetInstance(TimeZoneInfo.Utc, dtx);
        }

        public static DateTimeEx UtcInstance(DateTimeOffset dtoffset)
        {
            return GetInstance(TimeZoneInfo.Utc, dtoffset);
        }

        public static DateTimeEx UtcInstance(DateTime dateTime)
        {
            return GetInstance(TimeZoneInfo.Utc, dateTime);
        }

        public static DateTimeEx UtcInstance(long timeInMillis)
        {
            return GetInstance(TimeZoneInfo.Utc, timeInMillis);
        }
    }
}