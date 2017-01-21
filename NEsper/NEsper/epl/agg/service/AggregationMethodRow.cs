///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.agg.aggregator;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>A row in aggregation state. </summary>
    public class AggregationMethodRow
    {
        private long _refcount;
        private readonly AggregationMethod[] _methods;
    
        /// <summary>Ctor. </summary>
        /// <param name="refcount">number of items in state</param>
        /// <param name="methods">aggregations</param>
        public AggregationMethodRow(long refcount, AggregationMethod[] methods)
        {
            _refcount = refcount;
            _methods = methods;
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
    }
}
