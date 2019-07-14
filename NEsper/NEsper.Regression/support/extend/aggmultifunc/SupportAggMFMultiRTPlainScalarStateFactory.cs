///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.regressionlib.support.extend.aggmultifunc
{
    public class SupportAggMFMultiRTPlainScalarStateFactory : AggregationMultiFunctionStateFactory
    {
        public ExprEvaluator Param { get; private set; }

        public AggregationMultiFunctionState NewState(AggregationMultiFunctionStateFactoryContext ctx)
        {
            return new SupportAggMFMultiRTPlainScalarState(this);
        }

        public void SetParam(ExprEvaluator param)
        {
            Param = param;
        }
    }
} // end of namespace