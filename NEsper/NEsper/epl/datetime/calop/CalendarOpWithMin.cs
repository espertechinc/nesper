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
    public class CalendarOpWithMin : CalendarOp {
        private readonly CalendarFieldEnum _fieldName;
    
        public CalendarOpWithMin(CalendarFieldEnum fieldName) {
            _fieldName = fieldName;
        }
    
        public void Evaluate(ref DateTime dateTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            switch(_fieldName)
            {
                case CalendarFieldEnum.MILLISEC:
                    dateTime = new DateTime(
                        dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, 0);
                    break;
                case CalendarFieldEnum.SECOND:
                    dateTime = new DateTime(
                        dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0, dateTime.Millisecond);
                    break;
                case CalendarFieldEnum.MINUTE:
                    dateTime = new DateTime(
                        dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, dateTime.Second, dateTime.Millisecond);
                    break;
                case CalendarFieldEnum.HOUR:
                    dateTime = new DateTime(
                        dateTime.Year, dateTime.Month, dateTime.Day, 0, dateTime.Minute, dateTime.Second, dateTime.Millisecond);
                    break;
                case CalendarFieldEnum.DAY:
                    dateTime = new DateTime(
                        dateTime.Year, dateTime.Month, 1, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond);
                    break;
                case CalendarFieldEnum.MONTH:
                    dateTime = new DateTime(
                        dateTime.Year, 1, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond);
                    break;
                case CalendarFieldEnum.YEAR:
                {
                    var minDate = DateTime.MinValue;
                    dateTime = new DateTime(
                        minDate.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond);
                    break;
                }
                case CalendarFieldEnum.WEEK:
                    dateTime = dateTime.GetWithMinimumWeek();
                    break;
            }
        }
    }
}
