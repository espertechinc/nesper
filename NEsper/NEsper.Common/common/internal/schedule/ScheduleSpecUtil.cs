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

using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.schedule
{
    /// <summary>
    /// Utility for computing from a set of parameter objects a schedule specification carry a
    /// crontab-like schedule definition.
    /// </summary>
    public class ScheduleSpecUtil
    {
        /// <summary>
        /// Compute from parameters a crontab schedule.
        /// </summary>
        /// <param name="args">parameters</param>
        /// <exception cref="ScheduleParameterException">if the parameters are invalid</exception>
        /// <returns>crontab schedule</returns>
        public static ScheduleSpec ComputeValues(object[] args)
        {
            if (args.Length <= 4 || args.Length >= 8) {
                throw new ScheduleParameterException(GetExpressionCountException(args.Length));
            }

            var unitMap = new Dictionary<ScheduleUnit, ICollection<int>>();
            var minutes = args[0];
            var hours = args[1];
            var daysOfMonth = args[2];
            var months = args[3];
            var daysOfWeek = args[4];
            unitMap.Put(ScheduleUnit.MINUTES, ComputeValues(minutes, ScheduleUnit.MINUTES));
            unitMap.Put(ScheduleUnit.HOURS, ComputeValues(hours, ScheduleUnit.HOURS));
            var resultMonths = ComputeValues(months, ScheduleUnit.MONTHS);
            if (daysOfWeek is CronParameter && daysOfMonth is CronParameter) {
                throw new ScheduleParameterException(
                    "Invalid combination between days of week and days of month fields for timer:at");
            }

            if (resultMonths != null && resultMonths.Count == 1 && (resultMonths.First().IsInt())) {
                // If other arguments are cronParameters, use it for later computations
                CronParameter parameter = null;
                if (daysOfMonth is CronParameter) {
                    parameter = (CronParameter) daysOfMonth;
                }
                else if (daysOfWeek is CronParameter) {
                    parameter = (CronParameter) daysOfWeek;
                }

                if (parameter != null) {
                    parameter.Month = resultMonths.First();
                }
            }

            var resultDaysOfWeek = ComputeValues(daysOfWeek, ScheduleUnit.DAYS_OF_WEEK);
            var resultDaysOfMonth = ComputeValues(daysOfMonth, ScheduleUnit.DAYS_OF_MONTH);
            if (resultDaysOfWeek != null && resultDaysOfWeek.Count == 1 && (resultDaysOfWeek.First().IsInt())) {
                // The result is in the form "last xx of the month
                // Days of week is replaced by a wildcard and days of month is updated with
                // the computation of "last xx day of month".
                // In this case "days of month" parameter has to be a wildcard.
                if (resultDaysOfWeek.First() > 6) {
                    if (resultDaysOfMonth != null) {
                        throw new ScheduleParameterException(
                            "Invalid combination between days of week and days of month fields for timer:at");
                    }

                    resultDaysOfMonth = resultDaysOfWeek;
                    resultDaysOfWeek = null;
                }
            }

            if (resultDaysOfMonth != null && resultDaysOfMonth.Count == 1 && (resultDaysOfMonth.First().IsInt())) {
                if (resultDaysOfWeek != null) {
                    throw new ScheduleParameterException(
                        "Invalid combination between days of week and days of month fields for timer:at");
                }
            }

            unitMap.Put(ScheduleUnit.DAYS_OF_WEEK, resultDaysOfWeek);
            unitMap.Put(ScheduleUnit.DAYS_OF_MONTH, resultDaysOfMonth);
            unitMap.Put(ScheduleUnit.MONTHS, resultMonths);
            if (args.Length > 5) {
                unitMap.Put(ScheduleUnit.SECONDS, ComputeValues(args[5], ScheduleUnit.SECONDS));
            }

            string timezone = null;
            if (args.Length > 6) {
                if (!(args[6] is WildcardParameter)) {
                    if (!(args[6] is string)) {
                        throw new ScheduleParameterException(
                            "Invalid timezone parameter '" + args[6] + "' for timer:at, expected a string-type value");
                    }

                    timezone = (string) args[6];
                }
            }

            var optionalDayOfMonthOp = GetOptionalSpecialOp(daysOfMonth);
            var optionalDayOfWeekOp = GetOptionalSpecialOp(daysOfWeek);
            return new ScheduleSpec(unitMap, timezone, optionalDayOfMonthOp, optionalDayOfWeekOp);
        }

        public static string GetExpressionCountException(int length)
        {
            return "Invalid number of crontab parameters, expecting between 5 and 7 parameters, received " + length;
        }

        private static CronParameter GetOptionalSpecialOp(object unitParameter)
        {
            if (!(unitParameter is CronParameter)) {
                return null;
            }

            return (CronParameter) unitParameter;
        }

        private static ICollection<int> ComputeValues(
            object unitParameter,
            ScheduleUnit unit)
        {
            ICollection<int> result;
            if (unitParameter is int) {
                result = new SortedSet<int>();
                result.Add((int) unitParameter);
                return result;
            }

            // cron parameters not handled as number sets
            if (unitParameter is CronParameter) {
                return null;
            }

            var numberSet = (NumberSetParameter) unitParameter;
            if (numberSet.IsWildcard(unit.Min(), unit.Max())) {
                return null;
            }

            result = numberSet.GetValuesInRange(unit.Min(), unit.Max());
            var resultSorted = new SortedSet<int>();
            resultSorted.AddAll(result);

            return resultSorted;
        }
    }
} // end of namespace