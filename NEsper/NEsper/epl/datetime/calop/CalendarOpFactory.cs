///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.epl.datetime.eval;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.datetime.calop
{
    public class CalendarOpFactory : OpFactory
    {
        public CalendarOp GetOp(DatetimeMethodEnum method, String methodNameUsed, IList<ExprNode> parameters, ExprEvaluator[] evaluators)
        {
            if (method == DatetimeMethodEnum.WITHTIME) {
                return new CalendarOpWithTime(evaluators[0], evaluators[1], evaluators[2], evaluators[3]);
            }
            if (method == DatetimeMethodEnum.WITHDATE) {
                return new CalendarOpWithDate(evaluators[0], evaluators[1], evaluators[2]);
            }
            if (method == DatetimeMethodEnum.PLUS || method == DatetimeMethodEnum.MINUS) {
                return new CalendarOpPlusMinus(evaluators[0], method == DatetimeMethodEnum.MINUS ? -1 : 1);
            }
            if (method == DatetimeMethodEnum.WITHMAX ||
                method == DatetimeMethodEnum.WITHMIN ||
                method == DatetimeMethodEnum.ROUNDCEILING ||
                method == DatetimeMethodEnum.ROUNDFLOOR ||
                method == DatetimeMethodEnum.ROUNDHALF ||
                method == DatetimeMethodEnum.SET) {
                CalendarFieldEnum fieldNum = CalendarOpUtil.GetEnum(methodNameUsed, parameters[0]);
                if (method == DatetimeMethodEnum.WITHMIN) {
                    return new CalendarOpWithMin(fieldNum);
                }
                if (method == DatetimeMethodEnum.ROUNDCEILING || method == DatetimeMethodEnum.ROUNDFLOOR || method == DatetimeMethodEnum.ROUNDHALF) {
                    return new CalendarOpRound(fieldNum, method);
                }
                if (method == DatetimeMethodEnum.SET) {
                    return new CalendarOpSet(fieldNum, evaluators[1]);
                }
                return new CalendarOpWithMax(fieldNum);
            }
            throw new IllegalStateException("Unrecognized calendar-op code '" + method + "'");
        }
    }
}
