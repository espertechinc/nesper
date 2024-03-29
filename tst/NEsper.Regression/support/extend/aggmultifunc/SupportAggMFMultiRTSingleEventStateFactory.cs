///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.regressionlib.support.extend.aggmultifunc
{
    public class SupportAggMFMultiRTSingleEventStateFactory : AggregationMultiFunctionStateFactory
    {
        public static IList<SupportAggMFMultiRTSingleEventState> StateContexts { get; } =
            new List<SupportAggMFMultiRTSingleEventState>();

        public ExprEvaluator Param { get; set; }

        public AggregationMultiFunctionState NewState(AggregationMultiFunctionStateFactoryContext ctx)
        {
            var state = new SupportAggMFMultiRTSingleEventState();
            StateContexts.Add(state);
            return state;
        }

        public static void Reset()
        {
            StateContexts.Clear();
        }

        public static void Clear()
        {
            StateContexts.Clear();
        }
    }
} // end of namespace