///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.agg.groupby
{
    public class AggSvcGroupByReclaimAgedEvalFuncFactoryConst : AggSvcGroupByReclaimAgedEvalFuncFactory,
        AggSvcGroupByReclaimAgedEvalFunc
    {
        private readonly double valueDouble;

        public AggSvcGroupByReclaimAgedEvalFuncFactoryConst(double valueDouble)
        {
            this.valueDouble = valueDouble;
        }

        public double? LongValue => valueDouble;

        public AggSvcGroupByReclaimAgedEvalFunc Make(ExprEvaluatorContext exprEvaluatorContext)
        {
            return this;
        }
    }
} // end of namespace