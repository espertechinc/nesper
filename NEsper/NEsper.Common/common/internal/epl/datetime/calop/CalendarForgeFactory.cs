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
    public class CalendarForgeFactory : ForgeFactory
    {
        public CalendarForge GetOp(
            DatetimeMethodEnum method, string methodNameUsed, IList<ExprNode> parameters, ExprForge[] forges)
        {
            if (method == DatetimeMethodEnum.WITHTIME) {
                return new CalendarWithTimeForge(forges[0], forges[1], forges[2], forges[3]);
            }

            if (method == DatetimeMethodEnum.WITHDATE) {
                return new CalendarWithDateForge(forges[0], forges[1], forges[2]);
            }

            if (method == DatetimeMethodEnum.PLUS || method == DatetimeMethodEnum.MINUS) {
                return new CalendarPlusMinusForge(forges[0], method == DatetimeMethodEnum.MINUS ? -1 : 1);
            }

            if (method == DatetimeMethodEnum.WITHMAX ||
                method == DatetimeMethodEnum.WITHMIN ||
                method == DatetimeMethodEnum.ROUNDCEILING ||
                method == DatetimeMethodEnum.ROUNDFLOOR ||
                method == DatetimeMethodEnum.ROUNDHALF ||
                method == DatetimeMethodEnum.SET) {
                CalendarFieldEnum fieldNum = CalendarOpUtil.GetEnum(methodNameUsed, parameters[0]);
                if (method == DatetimeMethodEnum.WITHMIN) {
                    return new CalendarWithMinForge(fieldNum);
                }

                if (method == DatetimeMethodEnum.ROUNDCEILING || method == DatetimeMethodEnum.ROUNDFLOOR ||
                    method == DatetimeMethodEnum.ROUNDHALF) {
                    return new CalendarForgeRound(fieldNum, method);
                }

                if (method == DatetimeMethodEnum.SET) {
                    return new CalendarSetForge(fieldNum, forges[1]);
                }

                return new CalendarWithMaxForge(fieldNum);
            }

            throw new IllegalStateException("Unrecognized calendar-op code '" + method + "'");
        }
    }
} // end of namespace