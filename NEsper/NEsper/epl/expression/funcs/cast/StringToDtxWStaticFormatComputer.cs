///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.epl.expression.funcs.cast
{
    public class StringToDtxWStaticFormatComputer : StringToDateTimeBaseWStaticFormatComputer<DateTimeEx>
    {
        private readonly TimeZoneInfo _timeZoneInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringToDtxWDynamicFormatComputer"/> class.
        /// </summary>
        /// <param name="dateFormat">The date format eval.</param>
        /// <param name="timeZoneInfo">The time zone information.</param>
        public StringToDtxWStaticFormatComputer(string dateFormat, TimeZoneInfo timeZoneInfo)
            : base(dateFormat)
        {
            _timeZoneInfo = timeZoneInfo;
        }

        /// <summary>
        /// Computes from format.
        /// </summary>
        /// <param name="dateFormat">The date format.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        protected override DateTimeEx ComputeFromFormat(string dateFormat, string input)
        {
            return DateTimeEx.GetInstance(_timeZoneInfo, ParseDateTime(input, dateFormat, _timeZoneInfo));
        }
    }
}
