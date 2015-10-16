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
using com.espertech.esper.epl.datetime.eval;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.datetime.calop
{
    public class CalendarOpRound : CalendarOp
    {
        private readonly CalendarFieldEnum _fieldName;
        private readonly int _code;
    
        public CalendarOpRound(CalendarFieldEnum fieldName, DatetimeMethodEnum method) {
            _fieldName = fieldName;
            if (method == DatetimeMethodEnum.ROUNDCEILING) {
                _code = ApacheCommonsDateUtils.MODIFY_CEILING;
            }
            else if (method == DatetimeMethodEnum.ROUNDFLOOR) {
                _code = ApacheCommonsDateUtils.MODIFY_TRUNCATE;
            }
            else if (method == DatetimeMethodEnum.ROUNDHALF) {
                _code = ApacheCommonsDateUtils.MODIFY_ROUND;
            }
            else {
                throw new ArgumentException("Unrecognized method '" + method + "'");
            }
        }
    
        public void Evaluate(DateTimeEx dateTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            dateTime.Set(
                ApacheCommonsDateUtils.Modify(
                    dateTime.DateTime, _fieldName.ToDateTimeFieldEnum(), _code, dateTime.TimeZone));
        }
    }
}
