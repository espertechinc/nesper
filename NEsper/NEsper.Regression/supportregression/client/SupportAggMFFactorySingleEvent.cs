///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.plugin;

namespace com.espertech.esper.supportregression.client
{
    public class SupportAggMFFactorySingleEvent : PlugInAggregationMultiFunctionStateFactory
    {
        private static readonly IList<PlugInAggregationMultiFunctionStateContext> stateContexts = 
            new List<PlugInAggregationMultiFunctionStateContext>();
    
        public static void Reset()
        {
            stateContexts.Clear();
        }
    
        public static void Clear()
        {
            stateContexts.Clear();
        }

        public static IList<PlugInAggregationMultiFunctionStateContext> StateContexts
        {
            get { return stateContexts; }
        }

        public AggregationState MakeAggregationState(PlugInAggregationMultiFunctionStateContext stateContext)
        {
            stateContexts.Add(stateContext);;
            return new SupportAggMFStateSingleEvent();
        }
    }
}
