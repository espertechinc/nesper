///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.funcs.cast
{
    public class StringToDtoWDynamicFormatComputer : StringToDateTimeBaseWDynamicFormat<DateTimeOffset>
    {
        private readonly TimeZoneInfo _timeZoneInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringToDtoWDynamicFormatComputer" /> class.
        /// </summary>
        /// <param name="dateFormatEval">The date format eval.</param>
        /// <param name="timeZoneInfo">The time zone information.</param>
        public StringToDtoWDynamicFormatComputer(ExprEvaluator dateFormatEval, TimeZoneInfo timeZoneInfo)
            : base(dateFormatEval)
        {
            _timeZoneInfo = timeZoneInfo;
        }

        /// <summary>
        /// Computes the value from the date format and input.
        /// </summary>
        /// <param name="dateFormat">The date format.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        protected override DateTimeOffset ComputeFromFormat(string dateFormat, string input)
        {
            return ParseDateTime(input, dateFormat, _timeZoneInfo);
        }
    }
}
