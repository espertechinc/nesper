///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.datetime.eval;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.datetime.calop
{
    public class CalendarForgeFactory : DatetimeMethodProviderForgeFactory
    {
        public CalendarForge GetOp(
            DatetimeMethodDesc desc,
            string methodNameUsed,
            IList<ExprNode> parameters,
            ExprForge[] forges)
        {
            DateTimeMethodEnum method = desc.DatetimeMethod;
            switch (method) {
                case DateTimeMethodEnum.WITHTIME:
                    return new CalendarWithTimeForge(forges[0], forges[1], forges[2], forges[3]);

                case DateTimeMethodEnum.WITHDATE:
                    return new CalendarWithDateForge(forges[0], forges[1], forges[2]);

                case DateTimeMethodEnum.PLUS:
                case DateTimeMethodEnum.MINUS:
                    return new CalendarPlusMinusForge(forges[0], method == DateTimeMethodEnum.MINUS ? -1 : 1);

                case DateTimeMethodEnum.WITHMAX:
                case DateTimeMethodEnum.WITHMIN:
                case DateTimeMethodEnum.ROUNDCEILING:
                case DateTimeMethodEnum.ROUNDFLOOR:
                case DateTimeMethodEnum.ROUNDHALF:
                case DateTimeMethodEnum.SET: {
                    var fieldNum = CalendarOpUtil.GetEnum(methodNameUsed, parameters[0]);
                    return method switch {
                        DateTimeMethodEnum.WITHMIN => new CalendarWithMinForge(fieldNum),
                        DateTimeMethodEnum.ROUNDCEILING => new CalendarForgeRound(fieldNum, method),
                        DateTimeMethodEnum.ROUNDFLOOR => new CalendarForgeRound(fieldNum, method),
                        DateTimeMethodEnum.ROUNDHALF => new CalendarForgeRound(fieldNum, method),
                        DateTimeMethodEnum.SET => new CalendarSetForge(fieldNum, forges[1]),
                        _ => new CalendarWithMaxForge(fieldNum)
                    };
                }

                default:
                    throw new IllegalStateException("Unrecognized calendar-op code '" + method + "'");
            }
        }
    }
} // end of namespace