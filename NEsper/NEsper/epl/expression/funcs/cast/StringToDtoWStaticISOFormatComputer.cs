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
    public class StringToDtoWStaticISOFormatComputer : StringToDateTimeBaseWStaticISOFormatComputer<DateTimeOffset>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringToDtoWStaticISOFormatComputer"/> class.
        /// </summary>
        /// <param name="timeZoneInfo">The time zone information.</param>
        public StringToDtoWStaticISOFormatComputer(TimeZoneInfo timeZoneInfo) : base(timeZoneInfo)
        {
        }

        /// <summary>
        /// Computes the value from the format and input.
        /// </summary>
        /// <param name="dateFormat">The date format.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        protected override DateTimeOffset ComputeFromFormat(string dateFormat, string input)
        {
            return ParseISO(input).DateTime;
        }
    }
}
