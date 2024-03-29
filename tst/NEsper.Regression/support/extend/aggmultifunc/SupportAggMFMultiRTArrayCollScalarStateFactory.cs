///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.regressionlib.support.extend.aggmultifunc
{
    public class SupportAggMFMultiRTArrayCollScalarStateFactory : AggregationMultiFunctionStateFactory
    {
        public ExprEvaluator Evaluator { get; set; }

        public Type EvaluationType { get; set; }

        public AggregationMultiFunctionState NewState(AggregationMultiFunctionStateFactoryContext ctx)
        {
            return new SupportAggMFMultiRTArrayCollScalarState(this);
        }
    }
} // end of namespace