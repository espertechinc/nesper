///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Globalization;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.funcs.cast
{
    public class StringToDateTimeOffsetWStaticFormatComputer : StringToDateLongWStaticFormat
    {
        public StringToDateTimeOffsetWStaticFormatComputer(string dateFormat)
            : base(dateFormat)
        {
        }

        internal static Object ParseSafe(string formatString, string format, Object input)
        {
            var inputAsString = input.ToString();

            try
            {
                DateTime dateTimeTemp;
                DateTime.TryParseExact(inputAsString, format, null, DateTimeStyles.None, out dateTimeTemp);
                //return format.Parse(input.ToString());
            }
            catch (Exception e)
            {
                throw HandleParseException(formatString, input.ToString(), e);
            }
        }

        public override Object Compute(Object input, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext)
        {
            return ParseSafe(base.DateFormat, formats.Get(), input);
        }
    }
}
