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
using com.espertech.esper.compat.datetime;

namespace com.espertech.esper.common.@internal.epl.datetime.calop
{
    public class CalendarForgeFactory : ForgeFactory
    {
        public CalendarForge GetOp(
            DateTimeMethodEnum method,
            string methodNameUsed,
            IList<ExprNode> parameters,
            ExprForge[] forges)
        {
            if (method == DateTimeMethodEnum.WITHTIME) {
                return new CalendarWithTimeForge(forges[0], forges[1], forges[2], forges[3]);
            }

            if (method == DateTimeMethodEnum.WITHDATE) {
                return new CalendarWithDateForge(forges[0], forges[1], forges[2]);
            }

            if (method == DateTimeMethodEnum.PLUS || method == DateTimeMethodEnum.MINUS) {
                return new CalendarPlusMinusForge(forges[0], method == DateTimeMethodEnum.MINUS ? -1 : 1);
            }

            if (method == DateTimeMethodEnum.WITHMAX ||
                method == DateTimeMethodEnum.WITHMIN ||
                method == DateTimeMethodEnum.ROUNDCEILING ||
                method == DateTimeMethodEnum.ROUNDFLOOR ||
                method == DateTimeMethodEnum.ROUNDHALF ||
                method == DateTimeMethodEnum.SET) {
                DateTimeFieldEnum fieldNum = CalendarOpUtil.GetEnum(methodNameUsed, parameters[0]);
                if (method == DateTimeMethodEnum.WITHMIN) {
                    return new CalendarWithMinForge(fieldNum);
                }

                if (method == DateTimeMethodEnum.ROUNDCEILING ||
                    method == DateTimeMethodEnum.ROUNDFLOOR ||
                    method == DateTimeMethodEnum.ROUNDHALF) {
                    return new CalendarForgeRound(fieldNum, method);
                }

                if (method == DateTimeMethodEnum.SET) {
                    return new CalendarSetForge(fieldNum, forges[1]);
                }

                return new CalendarWithMaxForge(fieldNum);
            }

            throw new IllegalStateException("Unrecognized calendar-op code '" + method + "'");
        }
    }
} // end of namespace