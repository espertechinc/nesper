///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.datetime.calop
{
    public class CalendarOpSet : CalendarOp 
    {
        private readonly CalendarFieldEnum _fieldName;
        private readonly ExprEvaluator _valueExpr;
    
        public CalendarOpSet(CalendarFieldEnum fieldName, ExprEvaluator valueExpr) 
        {
            _fieldName = fieldName;
            _valueExpr = valueExpr;
        }
    
        public void Evaluate(ref DateTime dateTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) 
        {
            int? ovalue = CalendarOpUtil.GetInt(_valueExpr, eventsPerStream, isNewData, context);
            if (ovalue == null)
            {
                return;
            }

            int value = ovalue.Value;

            switch (_fieldName)
            {
                case CalendarFieldEnum.MILLISEC:
                    dateTime = new DateTime(
                        dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, value);
                    break;
                case CalendarFieldEnum.SECOND:
                    dateTime = new DateTime(
                        dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, value, dateTime.Millisecond);
                    break;
                case CalendarFieldEnum.MINUTE:
                    dateTime = new DateTime(
                        dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, value, dateTime.Second, dateTime.Millisecond);
                    break;
                case CalendarFieldEnum.HOUR:
                    dateTime = new DateTime(
                        dateTime.Year, dateTime.Month, dateTime.Day, value, dateTime.Minute, dateTime.Second, dateTime.Millisecond);
                    break;
                case CalendarFieldEnum.DAY:
                    dateTime = new DateTime(
                        dateTime.Year, dateTime.Month, value, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond);
                    break;
                case CalendarFieldEnum.MONTH:
                    dateTime = new DateTime(
                        dateTime.Year, value, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond);
                    break;
                case CalendarFieldEnum.YEAR:
                    dateTime = new DateTime(
                        value, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond);
                    break;
                case CalendarFieldEnum.WEEK:
                    dateTime = dateTime.MoveToWeek(value);
                    break;
            }
        }
    }
}
