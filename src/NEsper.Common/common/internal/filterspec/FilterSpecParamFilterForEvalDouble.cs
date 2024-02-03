///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.filterspec
{
    public interface FilterSpecParamFilterForEvalDouble : FilterSpecParamFilterForEval
    {
        double GetFilterValueDouble(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextFilterEvalEnv filterEvalEnv);
    }

    public class ProxyFilterSpecParamFilterForEvalDouble : FilterSpecParamFilterForEvalDouble
    {
        public delegate double GetFilterValueDoubleFunc(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextFilterEvalEnv filterEvalEnv);

        public GetFilterValueDoubleFunc ProcGetFilterValueDouble { get; set; }

        public ProxyFilterSpecParamFilterForEvalDouble()
        {
        }

        public ProxyFilterSpecParamFilterForEvalDouble(GetFilterValueDoubleFunc procGetFilterValueDouble)
        {
            ProcGetFilterValueDouble = procGetFilterValueDouble;
        }

        public object GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextFilterEvalEnv filterEvalEnv)
        {
            return ProcGetFilterValueDouble.Invoke(matchedEvents, exprEvaluatorContext, filterEvalEnv);
        }

        public double GetFilterValueDouble(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextFilterEvalEnv filterEvalEnv)
        {
            return ProcGetFilterValueDouble.Invoke(matchedEvents, exprEvaluatorContext, filterEvalEnv);
        }
    }
} // end of namespace