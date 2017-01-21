///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.aggregator;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>Pair of aggregation methods and states (first/last/window) data window representations. </summary>
    public class AggregationRowPair
    {
        /// <summary>Ctor. </summary>
        /// <param name="methods">aggregation methods/state</param>
        /// <param name="states">access is data window representations</param>
        public AggregationRowPair(AggregationMethod[] methods, AggregationState[] states)
        {
            Methods = methods;
            States = states;
        }

        /// <summary>Returns aggregation methods. </summary>
        /// <value>aggregation methods</value>
        public AggregationMethod[] Methods { get; private set; }

        /// <summary>Returns states to data window state. </summary>
        /// <value>states</value>
        public AggregationState[] States { get; private set; }
    }
}