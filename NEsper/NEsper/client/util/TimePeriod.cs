///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;

namespace com.espertech.esper.client.util
{
    [Serializable]
    public class TimePeriod
    {
        public TimePeriod(int? years, int? months, int? weeks, int? days, int? hours, int? minutes, int? seconds, int? milliseconds, int? microseconds)
        {
            Years = years;
            Months = months;
            Weeks = weeks;
            Days = days;
            Hours = hours;
            Minutes = minutes;
            Seconds = seconds;
            Milliseconds = milliseconds;
            Microseconds = microseconds;
        }

        public TimePeriod()
        {
        }

        public int? Years { get; set; }

        public int? Months { get; set; }

        public int? Weeks { get; set; }

        public int? Days { get; set; }

        public int? Hours { get; set; }

        public int? Minutes { get; set; }

        public int? Seconds { get; set; }

        public int? Milliseconds { get; set; }

        public int? Microseconds { get; set; }

        public TimePeriod SetYears(int? years)
        {
            Years = years;
            return this;
        }

        public TimePeriod SetMonths(int? months)
        {
            Months = months;
            return this;
        }

        public TimePeriod SetWeeks(int? weeks)
        {
            Weeks = weeks;
            return this;
        }

        public TimePeriod SetDays(int? days)
        {
            Days = days;
            return this;
        }

        public TimePeriod SetHours(int? hours)
        {
            Hours = hours;

            return this;
        }

        public TimePeriod SetMinutes(int? minutes)
        {
            Minutes = minutes;
            return this;
        }

        public TimePeriod SetSeconds(int? seconds)
        {
            Seconds = seconds;
            return this;
        }

        public TimePeriod SetMillis(int? milliseconds)
        {
            Milliseconds = milliseconds;
            return this;
        }

        protected bool Equals(TimePeriod other)
        {
            return Years == other.Years &&
                   Months == other.Months &&
                   Weeks == other.Weeks &&
                   Days == other.Days &&
                   Hours == other.Hours &&
                   Minutes == other.Minutes &&
                   Seconds == other.Seconds &&
                   Milliseconds == other.Milliseconds &&
                   Microseconds == other.Microseconds;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((TimePeriod)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Years.GetHashCode();
                hashCode = (hashCode * 397) ^ Months.GetHashCode();
                hashCode = (hashCode * 397) ^ Weeks.GetHashCode();
                hashCode = (hashCode * 397) ^ Days.GetHashCode();
                hashCode = (hashCode * 397) ^ Hours.GetHashCode();
                hashCode = (hashCode * 397) ^ Minutes.GetHashCode();
                hashCode = (hashCode * 397) ^ Seconds.GetHashCode();
                hashCode = (hashCode * 397) ^ Milliseconds.GetHashCode();
                hashCode = (hashCode * 397) ^ Microseconds.GetHashCode();
                return hashCode;
            }
        }

        public string ToStringISO8601()
        {
            var buf = new StringBuilder();
            if (Years != null)
            {
                Append(buf, Years, "Y");
            }
            if (Months != null)
            {
                Append(buf, Months, "M");
            }
            if (Weeks != null)
            {
                Append(buf, Weeks, "W");
            }
            if (Days != null)
            {
                Append(buf, Days, "D");
            }
            if (Hours != null || Minutes != null || Seconds != null)
            {
                buf.Append("T");
                if (Hours != null)
                {
                    Append(buf, Hours, "H");
                }
                if (Minutes != null)
                {
                    Append(buf, Minutes, "M");
                }
                if (Seconds != null)
                {
                    Append(buf, Seconds, "S");
                }
            }
            return buf.ToString();
        }

        public int? LargestAbsoluteValue()
        {
            int? absMax = null;
            if (Years != null && (absMax == null || Math.Abs(Years.Value) > absMax))
            {
                absMax = Math.Abs(Years.Value);
            }
            if (Months != null && (absMax == null || Math.Abs(Months.Value) > absMax))
            {
                absMax = Math.Abs(Months.Value);
            }
            if (Weeks != null && (absMax == null || Math.Abs(Weeks.Value) > absMax))
            {
                absMax = Math.Abs(Weeks.Value);
            }
            if (Days != null && (absMax == null || Math.Abs(Days.Value) > absMax))
            {
                absMax = Math.Abs(Days.Value);
            }
            if (Hours != null && (absMax == null || Math.Abs(Hours.Value) > absMax))
            {
                absMax = Math.Abs(Hours.Value);
            }
            if (Minutes != null && (absMax == null || Math.Abs(Minutes.Value) > absMax))
            {
                absMax = Math.Abs(Minutes.Value);
            }
            if (Seconds != null && (absMax == null || Math.Abs(Seconds.Value) > absMax))
            {
                absMax = Math.Abs(Seconds.Value);
            }
            if (Milliseconds != null && (absMax == null || Math.Abs(Milliseconds.Value) > absMax))
            {
                absMax = Math.Abs(Milliseconds.Value);
            }
            if (Microseconds != null && (absMax == null || Math.Abs(Microseconds.Value) > absMax))
            {
                absMax = Math.Abs(Microseconds.Value);
            }
            return absMax;
        }

        private void Append(StringBuilder buf, int? units, string unit)
        {
            buf.Append(units.ToString());
            buf.Append(unit);
        }
    }
} // end of namespace
