///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.epl.expression.funcs.cast
{
    public class StringToDateTimeWStaticISOFormatComputer : StringToDateTimeBaseWStaticISOFormatComputer<DateTime>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringToDateTimeWStaticISOFormatComputer"/> class.
        /// </summary>
        public StringToDateTimeWStaticISOFormatComputer() : base(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringToDateTimeWStaticISOFormatComputer"/> class.
        /// </summary>
        /// <param name="timeZoneInfo">The time zone information.</param>
        public StringToDateTimeWStaticISOFormatComputer(TimeZoneInfo timeZoneInfo) : base(timeZoneInfo)
        {
        }

        /// <summary>
        /// Computes datetime from the dateFormat and input.
        /// </summary>
        /// <param name="dateFormat">The date format.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        protected override DateTime ComputeFromFormat(string dateFormat, string input)
        {
            return ParseISO(input).DateTime.DateTime;
        }
    }
}
