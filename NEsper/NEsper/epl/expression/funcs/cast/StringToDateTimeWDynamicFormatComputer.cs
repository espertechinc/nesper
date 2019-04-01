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
    public class StringToDateTimeWDynamicFormatComputer : StringToDateTimeBaseWDynamicFormat<DateTime>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringToDateTimeWDynamicFormatComputer"/> class.
        /// </summary>
        /// <param name="dateFormatEval">The date format evaluator.</param>
        public StringToDateTimeWDynamicFormatComputer(ExprEvaluator dateFormatEval)
            : base(dateFormatEval)
        {
        }

        /// <summary>
        /// Computes the value from the date format and input.
        /// </summary>
        /// <param name="dateFormat">The date format.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        protected override DateTime ComputeFromFormat(string dateFormat, string input)
        {
            return ParseDateTime(input, dateFormat, TimeZoneInfo.Utc).DateTime;
        }
    }
}
