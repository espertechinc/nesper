///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.agg.rollup
{
    public class GroupByRollupPerLevelExpression
    {
        private readonly ExprEvaluator[] _optionalHavingNodes;
        private readonly OrderByElement[][] _optionalOrderByElements;
        private readonly SelectExprProcessor[] _selectExprProcessor;

        public GroupByRollupPerLevelExpression(
            SelectExprProcessor[] selectExprProcessor,
            ExprEvaluator[] optionalHavingNodes,
            OrderByElement[][] optionalOrderByElements)
        {
            _selectExprProcessor = selectExprProcessor;
            _optionalHavingNodes = optionalHavingNodes;
            _optionalOrderByElements = optionalOrderByElements;
        }

        public SelectExprProcessor[] SelectExprProcessor
        {
            get { return _selectExprProcessor; }
        }

        public ExprEvaluator[] OptionalHavingNodes
        {
            get { return _optionalHavingNodes; }
        }

        public OrderByElement[][] OptionalOrderByElements
        {
            get { return _optionalOrderByElements; }
        }
    }
}