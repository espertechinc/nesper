///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.collection;
using com.espertech.esper.compat;

namespace com.espertech.esper.epl.agg.aggregator
{
    /// <summary>Median aggregation. </summary>
    public class AggregatorMedian : AggregationMethod
    {
        private readonly SortedDoubleVector _vector;
    
        /// <summary>Ctor. </summary>
        public AggregatorMedian()
        {
            _vector = new SortedDoubleVector();
        }

        public virtual void Enter(Object @object)
        {
            if (@object == null)
            {
                return;
            }

            _vector.Add(@object.AsDouble());
        }
    
        public virtual void Leave(Object @object)
        {
            if (@object == null)
            {
                return;
            }
            _vector.Remove(@object.AsDouble());
        }

        public virtual void Clear()
        {
            _vector.Clear();
        }

        public virtual object Value
        {
            get
            {
                if (_vector.Count == 0)
                {
                    return null;
                }
                if (_vector.Count == 1)
                {
                    return _vector[0];
                }

                int middle = _vector.Count >> 1;
                if (_vector.Count % 2 == 0)
                {
                    return (_vector[middle - 1] + _vector[middle])/2;
                }
                else
                {
                    return _vector[middle];
                }
            }
        }
    }
}
