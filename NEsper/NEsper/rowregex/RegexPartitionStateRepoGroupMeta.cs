///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.rowregex
{
    public class RegexPartitionStateRepoGroupMeta
    {
        public RegexPartitionStateRepoGroupMeta(bool hasInterval, ExprNode[] partitionExpressionNodes, ExprEvaluator[] partitionExpressions, ExprEvaluatorContext exprEvaluatorContext)
        {
            EventsPerStream = new EventBean[1];
            HasInterval = hasInterval;
            PartitionExpressionNodes = partitionExpressionNodes;
            PartitionExpressions = partitionExpressions;
            ExprEvaluatorContext = exprEvaluatorContext;
        }

        public bool HasInterval { get; private set; }

        public ExprNode[] PartitionExpressionNodes { get; private set; }

        public ExprEvaluator[] PartitionExpressions { get; private set; }

        public ExprEvaluatorContext ExprEvaluatorContext { get; private set; }

        public EventBean[] EventsPerStream { get; private set; }
    }
}
