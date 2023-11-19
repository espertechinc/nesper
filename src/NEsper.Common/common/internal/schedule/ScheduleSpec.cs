///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.schedule
{
    /// <summary>
    ///     Holds a schedule specification which consists of a set of integer values or a null
    ///     value for each schedule unit to indicate a wildcard. There is always an element in
    ///     the specification for each unit minutes, hours, day of month, month, and day of week.
    ///     There is optionally an element in the specification for the unit seconds.
    /// </summary>
    public sealed class ScheduleSpec
    {
        // Per unit hold the set of valid integer values, or null if wildcarded.
        // The seconds unit is optional.
        private string _optionalTimeZone;

        /// <summary>
        ///     Constructor - validates that all mandatory schedule.
        /// </summary>
        /// <param name="unitValues">are the values for each minute, hour, day, month etc.</param>
        /// <param name="optionalTimeZone">The optional time zone.</param>
        /// <param name="optionalDayOfMonthOperator">The optional day of month operator.</param>
        /// <param name="optionalDayOfWeekOperator">The optional day of week operator.</param>
        /// <throws>ArgumentException - if validation of value set per unit fails</throws>
        public ScheduleSpec(
            IDictionary<ScheduleUnit, ICollection<int>> unitValues,
            string optionalTimeZone,
            CronParameter optionalDayOfMonthOperator,
            CronParameter optionalDayOfWeekOperator)
        {
            Validate(unitValues);

            // Reduce to wildcards any unit's values set, if possible
            Compress(unitValues);

            UnitValues = unitValues;
            _optionalTimeZone = optionalTimeZone;
            OptionalDayOfMonthOperator = optionalDayOfMonthOperator;
            OptionalDayOfWeekOperator = optionalDayOfWeekOperator;
        }

        /// <summary>
        ///     Constructor - for unit testing, initialize to all wildcards but leave seconds empty.
        /// </summary>
        public ScheduleSpec()
        {
            UnitValues = new Dictionary<ScheduleUnit, ICollection<int>>();
            UnitValues.Put(ScheduleUnit.MINUTES, null);
            UnitValues.Put(ScheduleUnit.HOURS, null);
            UnitValues.Put(ScheduleUnit.DAYS_OF_MONTH, null);
            UnitValues.Put(ScheduleUnit.MONTHS, null);
            UnitValues.Put(ScheduleUnit.DAYS_OF_WEEK, null);
            _optionalTimeZone = null;
        }

        public CronParameter OptionalDayOfMonthOperator { get; set; }

        public CronParameter OptionalDayOfWeekOperator { get; set; }

        /// <summary>
        ///     Return map of ordered set of valid schedule values for minute, hour, day, month etc. units
        /// </summary>
        /// <value>map of 5 or 6 entries each with a set of integers</value>
        public IDictionary<ScheduleUnit, ICollection<int>> UnitValues { get; }

        public string OptionalTimeZone {
            get => _optionalTimeZone;
            set => _optionalTimeZone = value;
        }

        /// <summary>For unit testing, add a single value, changing wildcards to value sets. </summary>
        /// <param name="element">to add</param>
        /// <param name="value">to add</param>
        public void AddValue(
            ScheduleUnit element,
            int value)
        {
            var set = UnitValues.Get(element);
            if (set == null) {
                set = new SortedSet<int>();
                UnitValues.Put(element, set);
            }

            set.Add(value);
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();
            foreach (var element in EnumHelper.GetValues<ScheduleUnit>()) {
                if (!UnitValues.ContainsKey(element)) {
                    continue;
                }

                var valueSet = UnitValues.Get(element);
                buffer.Append(element + "={");
                if (valueSet == null) {
                    buffer.Append("null");
                }
                else {
                    var delimiter = "";
                    foreach (var i in valueSet) {
                        buffer.Append(delimiter + i);
                        delimiter = ",";
                    }
                }

                buffer.Append("} ");
            }

            return buffer.ToString();
        }

        public override bool Equals(object otherObject)
        {
            if (otherObject == this) {
                return true;
            }

            if (otherObject == null) {
                return false;
            }

            if (GetType() != otherObject.GetType()) {
                return false;
            }

            var other = (ScheduleSpec)otherObject;
            if (UnitValues.Count != other.UnitValues.Count) {
                return false;
            }

            foreach (var entry in UnitValues) {
                var mySet = entry.Value;
                var otherSet = other.UnitValues.Get(entry.Key);

                if (otherSet == null && mySet != null) {
                    return false;
                }

                if (otherSet != null && mySet == null) {
                    return false;
                }

                if (otherSet == null && mySet == null) {
                    continue;
                }

                if (mySet.Count != otherSet.Count) {
                    return false;
                }

                // Compare value by value
                if (mySet.Any(i => !otherSet.Contains(i))) {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            var hashCode = 0;
            foreach (var entry in UnitValues) {
                if (entry.Value != null) {
                    hashCode *= 31;
                    hashCode ^= entry.Value.First();
                }
            }

            return hashCode;
        }

        /// <summary>
        ///     Function to reduce value sets for unit that cover the whole range down to a wildcard.
        ///     I.e. reduce 0,1,2,3,4,5,6 for week value to 'null' indicating the wildcard.
        /// </summary>
        /// <param name="unitValues">is the set of valid values per unit</param>
        internal static void Compress(IDictionary<ScheduleUnit, ICollection<int>> unitValues)
        {
            var termList = new List<ScheduleUnit>();

            foreach (var entry in unitValues) {
                var elementValueSetSize = entry.Key.Max() - entry.Key.Min() + 1;
                if (entry.Value != null) {
                    if (entry.Value.Count == elementValueSetSize) {
                        termList.Add(entry.Key);
                    }
                }
            }

            foreach (var term in termList) {
                unitValues[term] = null;
            }
        }

        /// <summary>Validate units and their value sets. </summary>
        /// <param name="unitValues">is the set of valid values per unit</param>
        internal static void Validate(IDictionary<ScheduleUnit, ICollection<int>> unitValues)
        {
            if (!unitValues.ContainsKey(ScheduleUnit.MONTHS) ||
                !unitValues.ContainsKey(ScheduleUnit.DAYS_OF_WEEK) ||
                !unitValues.ContainsKey(ScheduleUnit.HOURS) ||
                !unitValues.ContainsKey(ScheduleUnit.MINUTES) ||
                !unitValues.ContainsKey(ScheduleUnit.DAYS_OF_MONTH)) {
                throw new ArgumentException(
                    "Incomplete information for schedule specification, only the following keys are supplied=" +
                    unitValues.Keys.RenderAny());
            }

            foreach (var unit in EnumHelper.GetValues<ScheduleUnit>()) {
                if ((unit == ScheduleUnit.SECONDS ||
                     unit == ScheduleUnit.MILLISECONDS ||
                     unit == ScheduleUnit.MICROSECONDS) &&
                    !unitValues.ContainsKey(unit)) {
                    // Seconds, milliseconds and microseconds are optional
                    continue;
                }

                if (unitValues.Get(unit) == null) // Wildcard - no validation for unit
                {
                    continue;
                }

                var values = unitValues.Get(unit);
                foreach (var value in values) {
                    if (value < unit.Min() || value > unit.Max()) {
                        throw new ArgumentException(
                            "Invalid value found for schedule unit, value of " +
                            value +
                            " is not valid for unit " +
                            unit);
                    }
                }
            }
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ScheduleSpec), GetType(), classScope);
            var spec = Ref("spec");
            method.Block.DeclareVarNewInstance<ScheduleSpec>(spec.Ref);
            if (_optionalTimeZone != null) {
                method.Block.SetProperty(spec, "OptionalTimeZone", Constant(_optionalTimeZone));
            }

            foreach (var unit in UnitValues.Keys) {
                var values = UnitValues.Get(unit);
                var valuesExpr = ConstantNull();
                if (values != null) {
                    valuesExpr = NewInstance<SortedSet<int>>(
                        StaticMethod(
                            typeof(CompatExtensions),
                            "AsList",
                            Constant(IntArrayUtil.ToArray(values))));
                }

                method.Block.Expression(
                    ExprDotMethodChain(spec).Get("UnitValues").Add("Put", Constant(unit), valuesExpr));
            }

            if (OptionalDayOfWeekOperator != null) {
                method.Block.SetProperty(spec, "OptionalDayOfWeekOperator", OptionalDayOfWeekOperator.Make());
            }

            if (OptionalDayOfMonthOperator != null) {
                method.Block.SetProperty(spec, "OptionalDayOfMonthOperator", OptionalDayOfMonthOperator.Make());
            }

            method.Block.MethodReturn(spec);
            return LocalMethod(method);
        }
    }
}