///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.agg.groupby
{
    public interface AggSvcGroupByReclaimAgedEvalFuncFactory
    {
        AggSvcGroupByReclaimAgedEvalFunc Make(ExprEvaluatorContext exprEvaluatorContext);
    }

    public class ProxyAggSvcGroupByReclaimAgedEvalFuncFactory : AggSvcGroupByReclaimAgedEvalFuncFactory
    {
        public Func<ExprEvaluatorContext, AggSvcGroupByReclaimAgedEvalFunc> procMake;

        public AggSvcGroupByReclaimAgedEvalFunc Make(ExprEvaluatorContext exprEvaluatorContext)
            => procMake(exprEvaluatorContext);
    }
} // end of namespace