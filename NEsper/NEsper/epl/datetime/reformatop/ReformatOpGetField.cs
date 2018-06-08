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
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.epl.@join.plan;

namespace com.espertech.esper.epl.datetime.reformatop
{
    public class ReformatOpGetField : ReformatOp
    {
        private readonly CalendarFieldEnum _fieldNum;
        private readonly TimeZoneInfo _timeZone;
        private readonly TimeAbacus _timeAbacus;
    
        public ReformatOpGetField(CalendarFieldEnum fieldNum, TimeZoneInfo timeZone, TimeAbacus timeAbacus) {
            _fieldNum = fieldNum;
            _timeZone = timeZone;
            _timeAbacus = timeAbacus;
        }
    
        public Object Evaluate(long ts, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext) {
            DateTimeEx cal = DateTimeEx.GetInstance(_timeZone);
            _timeAbacus.CalendarSet(ts, cal);
            return Action(cal);
        }
    
        public Object Evaluate(DateTime d, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext) {
            DateTimeEx cal = DateTimeEx.GetInstance(_timeZone, d);
            return Action(cal);
        }

        public Object Evaluate(DateTimeOffset d, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext)
        {
            DateTimeEx cal = DateTimeEx.GetInstance(_timeZone, d);
            return Action(cal);
        }

        public Object Evaluate(DateTimeEx dtx, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext)
        {
            return Action(dtx);
        }

        private int Action(DateTimeEx dtx)
        {
            switch (_fieldNum)
            {
                case CalendarFieldEnum.WEEK:
                    return dtx.DateTime.GetWeekOfYear();
                case CalendarFieldEnum.YEAR:
                    return dtx.Year;
                case CalendarFieldEnum.MONTH:
                    return dtx.Month;
                case CalendarFieldEnum.DAY:
                    return dtx.Day;
                case CalendarFieldEnum.HOUR:
                    return dtx.Hour;
                case CalendarFieldEnum.MINUTE:
                    return dtx.Minute;
                case CalendarFieldEnum.SECOND:
                    return dtx.Second;
                case CalendarFieldEnum.MILLISEC:
                    return dtx.Millisecond;
                default:
                    throw new ArgumentException("invalid value for field num");
            }
        }

        public Type ReturnType
        {
            get { return typeof (int?); }
        }

        public FilterExprAnalyzerAffector GetFilterDesc(
            EventType[] typesPerStream, 
            DatetimeMethodEnum currentMethod, 
            IList<ExprNode> currentParameters, 
            ExprDotNodeFilterAnalyzerInput inputDesc)
        {
            return null;
        }
    }
} // end of namespace
