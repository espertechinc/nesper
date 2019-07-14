///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.pattern.guard;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.regressionlib.support.extend.pattern
{
    public class MyCountToPatternGuardFactory : GuardFactory
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ExprEvaluator NumCountToExpr { get; set; }

        public MatchedEventConvertor Convertor { get; set; }

        public Guard MakeGuard(
            PatternAgentInstanceContext context,
            MatchedEventMap beginState,
            Quitable quitable,
            object guardState)
        {
            var events = Convertor == null ? null : Convertor.Convert(beginState);
            var parameter = PatternExpressionUtil.EvaluateChecked(
                "Count-to guard",
                NumCountToExpr,
                events,
                context.AgentInstanceContext);
            if (parameter == null) {
                throw new EPException("Count-to guard parameter evaluated to a null value");
            }

            var numCountTo = parameter.AsInt();
            return new MyCountToPatternGuard(numCountTo, quitable);
        }
    }
} // end of namespace