///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.rowregex;

namespace com.espertech.esper.core.context.stmt
{
    public class AIRegistryMatchRecognizePreviousSingle
        : AIRegistryMatchRecognizePrevious
        , RegexExprPreviousEvalStrategy
    {
        private RegexExprPreviousEvalStrategy _strategy;

        public void AssignService(int num, RegexExprPreviousEvalStrategy value)
        {
            _strategy = value;
        }

        public void DeassignService(int num)
        {
            _strategy = null;
        }

        public int AgentInstanceCount
        {
            get { return _strategy == null ? 0 : 1; }
        }

        public RegexPartitionStateRandomAccess GetAccess(ExprEvaluatorContext exprEvaluatorContext)
        {
            return _strategy.GetAccess(exprEvaluatorContext);
        }
    }
}