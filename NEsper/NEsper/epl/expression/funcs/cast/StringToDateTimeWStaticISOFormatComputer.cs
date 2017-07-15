///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.pattern.observer;
using com.espertech.esper.schedule;

namespace com.espertech.esper.epl.expression.funcs.cast
{
    public class StringToDateTimeWStaticISOFormatComputer : CasterParserComputer
    {
        public Object Compute(Object input, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext)
        {
            try
            {
                return TimerScheduleISO8601Parser.ParseDate(input.ToString()).DateTime.DateTime;
            }
            catch (ScheduleParameterException e)
            {
                throw HandleParseISOException(input.ToString(), e);
            }
        }

        public bool IsConstantForConstInput
        {
            get { return true; }
        }
    }
}
