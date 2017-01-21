///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.table.mgmt
{
    public class TableColumnMethodPair
    {
        public TableColumnMethodPair(ExprEvaluator evaluator, int targetIndex, ExprNode aggregationNode)
        {
            Evaluator = evaluator;
            TargetIndex = targetIndex;
            AggregationNode = aggregationNode;
        }

        public ExprEvaluator Evaluator { get; private set; }

        public int TargetIndex { get; private set; }

        public ExprNode AggregationNode { get; private set; }
    }
}
