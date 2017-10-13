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
    /// Aggregator for the very last value.
    /// </summary>
    public class AggregatorLastEver : AggregationMethod
    {
        private Object _lastValue;
    
        public virtual void Clear()
        {
            _lastValue = null;
        }
    
        public virtual void Enter(Object @object)
        {
            _lastValue = @object;
        }
    
        public virtual void Leave(Object @object)
        {
        }

        public object Value
        {
            get { return _lastValue; }
        }
    }
}
