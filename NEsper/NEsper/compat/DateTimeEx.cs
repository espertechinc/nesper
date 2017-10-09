///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.datetime.calop;
using com.espertech.esper.epl.named;

namespace com.espertech.esper.compat
{
    /// <summary>
    /// DateTime with offset and timezone tracking.  When math operations are performed against this
    /// structure, they can take into account the timezone the date was associated with.
    /// </summary>
    public class DateTimeEx 
        : IComparable<DateTimeEx>
        , IComparable
    {
        private DateTimeOffset _dateTime;
        private readonly TimeZoneInfo _timeZone;

        public int Year { get { return _dateTime.Year; } }
        public int Month { get { return _dateTime.Month; } }
        public int Day { get { return _dateTime.Day; } }
        public int Hour { get { return _dateTime.Hour; } }
        public int Minute { get { return _dateTime.Minute; } }
        public int Second { get { return _dateTime.Second; } }
        public int Millisecond { get { return _dateTime.Millisecond; } }

        public DayOfWeek DayOfWeek { get { return _dateTime.DayOfWeek; } }

        public long TimeInMillis { get { return _dateTime.TimeInMillis(); } }

        public int DayOfYear { get { return _dateTime.DateTime.DayOfYear; } }

        /// <summary>
        /// Gets the underlying date time object.
        /// </summary>
        /// <value>
        /// The date time.
        /// </value>
        public DateTimeOffset DateTime
        {
            get { return _dateTime; }
        }

        /// <summary>
        /// Gets the underlying date time object, translated to UTC.
        /// </summary>
        /// <value>
        /// The UTC date time.
        /// </value>
        public DateTimeOffset UtcDateTime
        {
            get { return _dateTime.TranslateTo(TimeZoneInfo.Utc); }
        }

        /// <summary>
        /// Gets the time zone associated with the value.
        /// </summary>
        /// <value>
        /// The time zone.
        /// </value>
        public TimeZoneInfo TimeZone
        {
            get { return _timeZone; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeEx"/> class.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="timeZone">The time zone.</param>
        public DateTimeEx(DateTimeOffset dateTime, TimeZoneInfo timeZone)
        {
            DateTime dateTimeUTC = dateTime.UtcDateTime;
            TimeSpan dateTimeOffset = timeZone.GetUtcOffset(dateTimeUTC);
            _dateTime = new DateTimeOffset(dateTimeUTC).ToOffset(dateTimeOffset);
            _timeZone = timeZone;
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
            _dateTime = new DateTimeOffset(year, month, day, hour, minute, second, millis, dateTimeUtcOffset);
            _timeZone = timeZoneInfo;
        }

        public DateTimeEx(DateTimeEx source)
        {
            _dateTime = source._dateTime;
            _timeZone = source._timeZone;
        }

        public DateTimeEx Clone()
        {
            return new DateTimeEx(this);
        }

        public DateTimeEx SetUtcMillis(long millis)
        {
            _dateTime = DateTimeOffsetHelper.TimeFromMillis(millis, _timeZone);
            return this;
        }

        public DateTimeEx Set(DateTimeOffset dateTime)
        {
            var dateTimeOffset = _timeZone.GetUtcOffset(dateTime);
            _dateTime = new DateTimeOffset(dateTime.DateTime, dateTimeOffset);
            return this;
        }

        public DateTimeEx Set(int year, int month, int day, int hour = 0, int minute = 0, int second = 0, int millisecond = 0)
        {
            _dateTime = DateTimeOffsetHelper.CreateDateTime(
                year, month, day, hour, minute, second, millisecond, _timeZone);
            return this;
        }

        public DateTimeEx SetYear(int year)
        {
            _dateTime = DateTimeOffsetHelper.CreateDateTime(
                year, _dateTime.Month, _dateTime.Day,
                _dateTime.Hour, _dateTime.Minute, _dateTime.Second,
                _dateTime.Millisecond, _timeZone);
            return this;
        }

        public DateTimeEx SetMonth(int month)
        {
            _dateTime = DateTimeOffsetHelper.CreateDateTime(
                _dateTime.Year, month, _dateTime.Day,
                _dateTime.Hour, _dateTime.Minute, _dateTime.Second,
                _dateTime.Millisecond, _timeZone);
            return this;
        }

        public DateTimeEx SetDay(int day)
        {
            _dateTime = DateTimeOffsetHelper.CreateDateTime(
                _dateTime.Year, _dateTime.Month, day,
                _dateTime.Hour, _dateTime.Minute, _dateTime.Second,
                _dateTime.Millisecond, _timeZone);
            return this;
        }

        public DateTimeEx SetHour(int hour)
        {
            _dateTime = DateTimeOffsetHelper.CreateDateTime(
                _dateTime.Year, _dateTime.Month, _dateTime.Day,
                hour, _dateTime.Minute, _dateTime.Second,
                _dateTime.Millisecond, _timeZone);
            return this;
        }

        public DateTimeEx SetMinute(int minute)
        {
            _dateTime = DateTimeOffsetHelper.CreateDateTime(
                _dateTime.Year, _dateTime.Month, _dateTime.Day,
                _dateTime.Hour, minute, _dateTime.Second,
                _dateTime.Millisecond, _timeZone);
            return this;
        }

        public DateTimeEx SetSecond(int second)
        {
            _dateTime = DateTimeOffsetHelper.CreateDateTime(
                _dateTime.Year, _dateTime.Month, _dateTime.Day,
                _dateTime.Hour, _dateTime.Minute, second,
                _dateTime.Millisecond, _timeZone);
            return this;
        }

        public DateTimeEx SetMillis(int millis)
        {
            _dateTime = DateTimeOffsetHelper.CreateDateTime(
                _dateTime.Year, _dateTime.Month,  _dateTime.Day,
                _dateTime.Hour,  _dateTime.Minute, _dateTime.Second,
                millis, _timeZone);
            return this;
        }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public int CompareTo(DateTimeEx other)
        {
            return _dateTime.CompareTo(other._dateTime);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and 
        /// returns an integer that indicates whether the current instance precedes, 
        /// follows, or occurs in the same position in the sort order as the other
        /// object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref name="obj" /> in the sort order. Zero This instance occurs in the same position in the sort order as <paramref name="obj" />. Greater than zero This instance follows <paramref name="obj" /> in the sort order.
        /// </returns>
        public int CompareTo(object obj)
        {
            var otherDateTime = obj as DateTimeEx;
            if (otherDateTime == null)
            {
                throw new ArgumentException("invalid value", nameof(obj));
            }

            return CompareTo(otherDateTime);
        }

        protected bool Equals(DateTimeEx other)
        {
            return _dateTime.Equals(other._dateTime) && Equals(_timeZone, other._timeZone);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((DateTimeEx) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_dateTime.GetHashCode() * 397) ^ (_timeZone != null ? _timeZone.GetHashCode() : 0);
            }
        }

        public string ToString(TimeZoneInfo timeZone)
        {
            if (timeZone != null)
            {
                var offset = timeZone.GetUtcOffset(_dateTime);
                return string.Format("{0} [TZ = {1}]", _dateTime.ToOffset(offset), _timeZone);
            }
            else
            {
                return string.Format("{0} [TZ = {1}]", _dateTime, _timeZone);
            }
        }

        public override string ToString()
        {
            return ToString(TimeZoneInfo.Local);
        }

        public static bool operator >(DateTimeEx d1, DateTimeEx d2) { return d1._dateTime > d2._dateTime; }
        public static bool operator >=(DateTimeEx d1, DateTimeEx d2) { return d1._dateTime >= d2._dateTime; }
        public static bool operator <(DateTimeEx d1, DateTimeEx d2) { return d1._dateTime < d2._dateTime; }
        public static bool operator <=(DateTimeEx d1, DateTimeEx d2) { return d1._dateTime <= d2._dateTime; }

        public static bool operator ==(DateTimeEx d1, DateTimeEx d2)
        {
            if (ReferenceEquals(d1, null))
                return ReferenceEquals(d2, null);
            if (ReferenceEquals(d2, null))
                return false;
            return d1._dateTime == d2._dateTime;
        }

        public static bool operator !=(DateTimeEx d1, DateTimeEx d2)
        {
            if (ReferenceEquals(d1, null))
                return !ReferenceEquals(d2, null);
            if (ReferenceEquals(d2, null))
                return true;
            return d1._dateTime != d2._dateTime;
        }

        public DateTimeEx AddYears(int amount, DateTimeMathStyle style = DateTimeMathStyle.CLR)
        {
            _dateTime = _dateTime.AddYears(amount);
            if (style == DateTimeMathStyle.Java)
                return Rebase();
            return this;
        }

        public DateTimeEx AddMonths(int amount, DateTimeMathStyle style = DateTimeMathStyle.CLR)
        {
            if (style == DateTimeMathStyle.CLR)
            {
                _dateTime = _dateTime.AddMonths(amount);
            }
            else
            {
                _dateTime = _dateTime.AddMonthsLikeJava(amount);
                return Rebase();
            }

            return this;
        }

        public DateTimeEx AddDays(int amount, DateTimeMathStyle style = DateTimeMathStyle.CLR)
        {
            _dateTime = _dateTime.AddDays(amount);
            if (style == DateTimeMathStyle.Java)
                return Rebase();
            return this;
        }

        public DateTimeEx AddHours(int amount, DateTimeMathStyle style = DateTimeMathStyle.CLR)
        {
            _dateTime = _dateTime.AddHours(amount);
            if (style == DateTimeMathStyle.Java)
                return Realign();
            return this;
        }

        public DateTimeEx AddMinutes(int amount, DateTimeMathStyle style = DateTimeMathStyle.CLR)
        {
            _dateTime = _dateTime.AddMinutes(amount);
            if (style == DateTimeMathStyle.Java)
                return Realign();
            return this;
        }

        public DateTimeEx AddSeconds(int amount, DateTimeMathStyle style = DateTimeMathStyle.CLR)
        {
            _dateTime = _dateTime.AddSeconds(amount);
            if (style == DateTimeMathStyle.Java)
                return Realign();
            return this;
        }

        public DateTimeEx AddMilliseconds(int amount, DateTimeMathStyle style = DateTimeMathStyle.CLR)
        {
            _dateTime = _dateTime.AddMilliseconds(amount);
            if (style == DateTimeMathStyle.Java)
                return Realign();
            return this;
        }

        /// <summary>
        /// Adds to fields in the datetime.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public DateTimeEx AddUsingField(int field, int value)
        {
            _dateTime = _dateTime.AddUsingField(field, value);
            return this;
        }

        public int GetFieldValue(int field)
        {
            return _dateTime.GetFieldValue(field);
        }

        public DateTimeEx SetFieldValue(int field, int value)
        {
            _dateTime = _dateTime.SetFieldValue(field, value, _timeZone);
            return this;
        }

        public DateTimeEx SetMaximumDay()
        {
            _dateTime = _dateTime.GetWithMaximumDay(_timeZone);
            return Rebase();
        }

        public DateTimeEx SetMaximumMonth()
        {
            _dateTime = _dateTime.GetWithMaximumMonth(_timeZone);
            return Rebase();
        }

        private DateTimeEx Realign()
        {
            var offset = _timeZone.GetUtcOffset(_dateTime);
            _dateTime = _dateTime.ToOffset(offset);
            return this;
        }

        private DateTimeEx Rebase()
        {
            _dateTime = new DateTimeOffset(
                _dateTime.Year,
                _dateTime.Month,
                _dateTime.Day,
                _dateTime.Hour,
                _dateTime.Minute,
                _dateTime.Second,
                _dateTime.Millisecond,
                _timeZone.GetUtcOffset(_dateTime));
            return this;
        }

        public DateTimeEx SetMinimumWeek()
        {
            _dateTime = _dateTime.GetWithMinimumWeek(_timeZone);
            return Rebase();
        }

        public DateTimeEx SetMaximumWeek()
        {
            _dateTime = _dateTime.GetWithMaximumWeek(_timeZone);
            return Rebase();
        }

        public int GetActualMaximum(int dayOfMonth)
        {
            return DateTimeFieldMath.GetActualMaximum(_dateTime, DateTimeFieldEnum.DAY_OF_MONTH);
        }

        public DateTimeEx MoveToWeek(int week)
        {
            _dateTime = _dateTime.MoveToWeek(week, _timeZone);
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

        public static DateTimeEx GetInstance(TimeZoneInfo timeZoneInfo, DateTimeOffset dtoffset)
        {
            return new DateTimeEx(dtoffset, timeZoneInfo);
        }

        public static DateTimeEx GetInstance(TimeZoneInfo timeZoneInfo, DateTime dateTime)
        {
            var baseDt = DateTimeOffsetHelper.ToDateTimeOffset(dateTime, timeZoneInfo);
            return new DateTimeEx(baseDt, timeZoneInfo);
        }

        public static DateTimeEx GetInstance(TimeZoneInfo timeZoneInfo, long timeInMillis)
        {
            var baseDt = DateTimeOffsetHelper.TimeFromMillis(timeInMillis, timeZoneInfo);
            return new DateTimeEx(baseDt, timeZoneInfo);
        }
    }
}
