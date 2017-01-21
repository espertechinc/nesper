///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.datetime.calop;
using com.espertech.esper.epl.datetime.eval;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.dot;

namespace com.espertech.esper.epl.datetime.reformatop
{
    public class ReformatOpGetField : ReformatOp
    {
        private readonly CalendarFieldEnum _fieldNum;
        private readonly TimeZoneInfo _timeZone;
    
        public ReformatOpGetField(CalendarFieldEnum fieldNum, TimeZoneInfo timeZone)
        {
            _fieldNum = fieldNum;
            _timeZone = timeZone;
        }

        public Object Evaluate(long ts, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext) 
        {
            return Action(ts.TimeFromMillis(_timeZone));
        }

        public object Evaluate(DateTimeOffset d, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext) 
        {
            return Action(d);
        }
    
        private int Action(DateTimeOffset dateTime)
        {
            switch (_fieldNum)
            {
                case CalendarFieldEnum.MILLISEC:
                    return dateTime.Millisecond;
                case CalendarFieldEnum.SECOND:
                    return dateTime.Second;
                case CalendarFieldEnum.MINUTE:
                    return dateTime.Minute;
                case CalendarFieldEnum.HOUR:
                    return dateTime.Hour;
                case CalendarFieldEnum.DAY:
                    return dateTime.Day;
                case CalendarFieldEnum.MONTH:
                    return dateTime.Month;
                case CalendarFieldEnum.YEAR:
                    return dateTime.Year;
                case CalendarFieldEnum.WEEK:
                    return dateTime.GetWeekOfYear();
            }

            throw new ArgumentException("invalid field enum");
        }

        public Type ReturnType
        {
            get { return typeof (int?); }
        }

        public ExprDotNodeFilterAnalyzerDesc GetFilterDesc(EventType[] typesPerStream,
                                                           DatetimeMethodEnum currentMethod,
                                                           ICollection<ExprNode> currentParameters,
                                                           ExprDotNodeFilterAnalyzerInput inputDesc)
        {
            return null;
        }
    }
}
