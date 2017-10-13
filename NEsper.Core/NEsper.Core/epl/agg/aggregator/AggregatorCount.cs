///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.epl.agg.aggregator
{
    /// <summary>
    /// Counts all datapoints including null values.
    /// </summary>
    public class AggregatorCount : AggregationMethod
    {
        private long _numDataPoints;
    
        public void Clear()
        {
            _numDataPoints = 0;
        }
    
        public void Enter(Object @object)
        {
            _numDataPoints++;
        }
    
        public void Leave(Object @object)
        {
            if (_numDataPoints > 0) {
                _numDataPoints--;
            }
        }

        public object Value
        {
            get { return _numDataPoints; }
        }
    }
}
