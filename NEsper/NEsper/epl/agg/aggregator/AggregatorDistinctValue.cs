///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.collection;

namespace com.espertech.esper.epl.agg.aggregator
{
    /// <summary>
    /// AggregationMethod for use on top of another aggregator that handles unique value 
    /// aggregation (versus all-value aggregation) for the underlying aggregator.
    /// </summary>
    public class AggregatorDistinctValue : AggregationMethod
    {
        private readonly AggregationMethod _inner;
        private readonly RefCountedSet<Object> _valueSet;
    
        /// <summary>Ctor. </summary>
        /// <param name="inner">is the aggregator function computing aggregation values</param>
        public AggregatorDistinctValue(AggregationMethod inner)
        {
            _inner = inner;
            _valueSet = new RefCountedSet<Object>();
        }
    
        public virtual void Clear()
        {
            _valueSet.Clear();
            _inner.Clear();
        }

        public virtual void Enter(Object value)
        {
            // if value not already encountered, enter into aggregate
            if (_valueSet.Add(value))
            {
                _inner.Enter(value);
            }
        }

        public virtual void Leave(Object value)
        {
            // if last reference to the value is removed, remove from aggregate
            if (_valueSet.Remove(value))
            {
                _inner.Leave(value);
            }
        }

        public virtual object Value
        {
            get { return _inner.Value; }
        }

        public virtual Type ValueType
        {
            get { return _inner.ValueType; }
        }
    }
}
