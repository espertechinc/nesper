///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.aggregator;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>A row in aggregation state. </summary>
    public class AggregationMethodPairRow
    {
        private long _refcount;
        private readonly AggregationMethod[] _methods;
        private readonly AggregationState[] _states;
    
        /// <summary>Ctor. </summary>
        /// <param name="refcount">number of items in state</param>
        /// <param name="methods">aggregations</param>
        /// <param name="states">for first/last/window type access</param>
        public AggregationMethodPairRow(long refcount, AggregationMethod[] methods, AggregationState[] states)
        {
            _refcount = refcount;
            _methods = methods;
            _states = states;
        }

        /// <summary>Returns number of data points. </summary>
        /// <value>data points</value>
        public long Refcount
        {
            get { return _refcount; }
        }

        /// <summary>Returns aggregation state. </summary>
        /// <value>state</value>
        public AggregationMethod[] Methods
        {
            get { return _methods; }
        }

        /// <summary>Increase number of data points by one. </summary>
        public void IncreaseRefcount()
        {
            _refcount++;
        }
    
        /// <summary>Decrease number of data points by one. </summary>
        public void DecreaseRefcount()
        {
            _refcount--;
        }

        /// <summary>Returns the states for first/last/window aggregation functions. </summary>
        /// <value>states</value>
        public AggregationState[] States
        {
            get { return _states; }
        }
    }
}
