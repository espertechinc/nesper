///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.plugin;

namespace com.espertech.esper.supportregression.client
{
    public class SupportAggMFStateArrayCollScalarFactory : PlugInAggregationMultiFunctionStateFactory
    {
        private readonly ExprEvaluator _evaluator;
    
        public SupportAggMFStateArrayCollScalarFactory(ExprEvaluator evaluator)
        {
            _evaluator = evaluator;
        }
    
        public AggregationState MakeAggregationState(PlugInAggregationMultiFunctionStateContext stateContext)
        {
            return new SupportAggMFStateArrayCollScalar(this);
        }

        public ExprEvaluator Evaluator
        {
            get { return _evaluator; }
        }
    }
}
