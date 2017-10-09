///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using com.espertech.esper.compat.collections;
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.schedule
{
    /// <summary>
    /// Holds a schedule specification which consists of a set of integer values or a null 
    /// value for each schedule unit to indicate a wildcard. There is always an element in 
    /// the specification for each unit minutes, hours, day of month, month, and day of week. 
    /// There is optionally an element in the specification for the unit seconds.
    /// </summary>
    [Serializable]
    public sealed class ScheduleSpec : MetaDefItem
    {
        // Per unit hold the set of valid integer values, or null if wildcarded.
        // The seconds unit is optional.
        private readonly IDictionary<ScheduleUnit, ICollection<int>> _unitValues;
        private String _optionalTimeZone;
        private readonly CronParameter _optionalDayOfMonthOperator;
        private readonly CronParameter _optionalDayOfWeekOperator;

        /// <summary>
        /// Constructor - validates that all mandatory schedule.
        /// </summary>
        /// <param name="unitValues">are the values for each minute, hour, day, month etc.</param>
        /// <param name="optionalTimeZone">The optional time zone.</param>
        /// <param name="optionalDayOfMonthOperator">The optional day of month operator.</param>
        /// <param name="optionalDayOfWeekOperator">The optional day of week operator.</param>
        /// <throws>ArgumentException - if validation of value set per unit fails</throws>
        public ScheduleSpec(IDictionary<ScheduleUnit, ICollection<int>> unitValues, String optionalTimeZone, CronParameter optionalDayOfMonthOperator, CronParameter optionalDayOfWeekOperator)
        {
            Validate(unitValues);

            // Reduce to wildcards any unit's values set, if possible
            Compress(unitValues);

            _unitValues = unitValues;
            _optionalTimeZone = optionalTimeZone;
            _optionalDayOfMonthOperator = optionalDayOfMonthOperator;
            _optionalDayOfWeekOperator = optionalDayOfWeekOperator;
        }

        /// <summary>
        /// Constructor - for unit testing, initialize to all wildcards but leave seconds empty.
        /// </summary>
        public ScheduleSpec()
        {
            _unitValues = new Dictionary<ScheduleUnit, ICollection<int>>();
            _unitValues.Put(ScheduleUnit.MINUTES, null);
            _unitValues.Put(ScheduleUnit.HOURS, null);
            _unitValues.Put(ScheduleUnit.DAYS_OF_MONTH, null);
            _unitValues.Put(ScheduleUnit.MONTHS, null);
            _unitValues.Put(ScheduleUnit.DAYS_OF_WEEK, null);
            _optionalTimeZone = null;
        }

        public CronParameter OptionalDayOfMonthOperator
        {
            get { return _optionalDayOfMonthOperator; }
        }

        public CronParameter OptionalDayOfWeekOperator
        {
            get { return _optionalDayOfWeekOperator; }
        }

        /// <summary>
        /// Return map of ordered set of valid schedule values for minute, hour, day, month etc. units
        /// </summary>
        /// <value>map of 5 or 6 entries each with a set of integers</value>
        public IDictionary<ScheduleUnit, ICollection<int>> UnitValues
        {
            get { return _unitValues; }
        }

        public string OptionalTimeZone
        {
            get { return _optionalTimeZone; }
            set { _optionalTimeZone = value; }
        }

        /// <summary>For unit testing, add a single value, changing wildcards to value sets. </summary>
        /// <param name="element">to add</param>
        /// <param name="value">to add</param>
        public void AddValue(ScheduleUnit element, int value)
        {
            var set = _unitValues.Get(element);
            if (set == null)
            {
                set = new SortedSet<int>();
                _unitValues.Put(element, set);
            }
            set.Add(value);
        }

        public override String ToString()
        {
            var buffer = new StringBuilder();
            foreach (ScheduleUnit element in ScheduleUnit.Values)
            {
                if (!_unitValues.ContainsKey(element))
                {
                    continue;
                }

                ICollection<int> valueSet = _unitValues.Get(element);
                buffer.Append(element + "={");
                if (valueSet == null)
                {
                    buffer.Append("null");
                }
                else
                {
                    String delimiter = "";
                    foreach (int i in valueSet)
                    {
                        buffer.Append(delimiter + i);
                        delimiter = ",";
                    }
                }
                buffer.Append("} ");
            }
            return buffer.ToString();
        }

        public override bool Equals(Object otherObject)
        {
            if (otherObject == this)
            {
                return true;
            }

            if (otherObject == null)
            {
                return false;
            }

            if (GetType() != otherObject.GetType())
            {
                return false;
            }

            var other = (ScheduleSpec)otherObject;
            if (_unitValues.Count != other._unitValues.Count)
            {
                return false;
            }

            foreach (var entry in _unitValues)
            {
                ICollection<int> mySet = entry.Value;
                ICollection<int> otherSet = other._unitValues.Get(entry.Key);

                if ((otherSet == null) && (mySet != null))
                {
                    return false;
                }
                if ((otherSet != null) && (mySet == null))
                {
                    return false;
                }
                if ((otherSet == null) && (mySet == null))
                {
                    continue;
                }
                if (mySet.Count != otherSet.Count)
                {
                    return false;
                }

                // Commpare value by value
                if (mySet.Any(i => !(otherSet.Contains(i))))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hashCode = 0;
            foreach (var entry in _unitValues)
            {
                if (entry.Value != null)
                {
                    hashCode *= 31;
                    hashCode ^= entry.Value.First();
                }
            }
            return hashCode;
        }

        /// <summary>
        /// Function to reduce value sets for unit that cover the whole range down to a wildcard. 
        /// I.e. reduce 0,1,2,3,4,5,6 for week value to 'null' indicating the wildcard.
        /// </summary>
        /// <param name="unitValues">is the set of valid values per unit</param>
        internal static void Compress(IDictionary<ScheduleUnit, ICollection<int>> unitValues)
        {
            var termList = new List<ScheduleUnit>();

            foreach (var entry in unitValues)
            {
                int elementValueSetSize = entry.Key.Max() - entry.Key.Min() + 1;
                if (entry.Value != null)
                {
                    if (entry.Value.Count == elementValueSetSize)
                    {
                        termList.Add(entry.Key);
                    }
                }
            }

            foreach (var term in termList)
            {
                unitValues[term] = null;
            }
        }

        /// <summary>Validate units and their value sets. </summary>
        /// <param name="unitValues">is the set of valid values per unit</param>
        internal static void Validate(IDictionary<ScheduleUnit, ICollection<int>> unitValues)
        {
            if ((!unitValues.ContainsKey(ScheduleUnit.MONTHS)) ||
                (!unitValues.ContainsKey(ScheduleUnit.DAYS_OF_WEEK)) ||
                (!unitValues.ContainsKey(ScheduleUnit.HOURS)) ||
                (!unitValues.ContainsKey(ScheduleUnit.MINUTES)) ||
                (!unitValues.ContainsKey(ScheduleUnit.DAYS_OF_MONTH)))
            {
                throw new ArgumentException("Incomplete information for schedule specification, only the following keys are supplied=" + unitValues.Keys.Render());
            }

            foreach (ScheduleUnit unit in ScheduleUnit.Values)
            {
                if ((unit == ScheduleUnit.SECONDS) && (!unitValues.ContainsKey(unit)))  // Seconds are optional
                {
                    continue;
                }

                if (unitValues.Get(unit) == null)       // Wildcard - no validation for unit
                {
                    continue;
                }

                ICollection<int> values = unitValues.Get(unit);
                foreach (int? value in values)
                {
                    if ((value < unit.Min()) || (value > unit.Max()))
                    {
                        throw new ArgumentException("Invalid value found for schedule unit, value of " +
                                value + " is not valid for unit " + unit);
                    }
                }
            }
        }
    }
}
