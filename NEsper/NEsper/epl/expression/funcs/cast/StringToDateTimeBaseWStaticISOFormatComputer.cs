///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.pattern.observer;

namespace com.espertech.esper.epl.expression.funcs.cast
{
    public abstract class StringToDateTimeBaseWStaticISOFormatComputer<T> : StringToDateTimeBaseComputer<T>
    {
        private readonly TimeZoneInfo _timeZoneInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringToDateTimeBaseWStaticISOFormatComputer{T}"/> class.
        /// </summary>
        /// <param name="timeZoneInfo">The time zone information.</param>
        protected StringToDateTimeBaseWStaticISOFormatComputer(TimeZoneInfo timeZoneInfo)
        {
            _timeZoneInfo = timeZoneInfo;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is constant for constant input.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is constant for constant input; otherwise, <c>false</c>.
        /// </value>
        public override bool IsConstantForConstInput
        {
            get { return true; }
        }

        /// <summary>
        /// Returns the date format that should be used for a given invocation.
        /// </summary>
        /// <param name="evaluateParams">The evaluate parameters.</param>
        /// <returns></returns>
        protected override string GetDateFormat(EvaluateParams evaluateParams)
        {
            return null;
        }

        /// <summary>
        /// Parses the date using an ISO8601 parser.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="translateToTargetTimeZone">if set to <c>true</c> [translate to target time zone].</param>
        /// <returns></returns>
        protected DateTimeEx ParseISO(string input, bool translateToTargetTimeZone = true)
        {
            var dateTimeEx = TimerScheduleISO8601Parser.ParseDate(input);
            if (translateToTargetTimeZone && (_timeZoneInfo != null))
            {
                dateTimeEx = DateTimeEx.GetInstance(_timeZoneInfo, dateTimeEx.DateTime.TranslateTo(_timeZoneInfo));
            }

            return dateTimeEx;
        }
    }
}
