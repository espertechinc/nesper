///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.epl.agg.aggregator
{
    /// <summary>Sum for decimal values. </summary>
    public class AggregatorSumDecimal : AggregationMethod
    {
        private decimal _sum;
        private long _numDataPoints;
    
        /// <summary>Ctor. </summary>
        public AggregatorSumDecimal()
        {
            _sum = 0.0m;
        }
    
        public virtual void Clear()
        {
            _sum = 0.0m;
            _numDataPoints = 0;
        }

        public virtual void Enter(Object @object)
        {
            if (@object == null)
            {
                return;
            }
            _numDataPoints++;
            _sum += @object.AsDecimal();
        }
    
        public virtual void Leave(Object @object)
        {
            if (@object == null)
            {
                return;
            }
            _numDataPoints--;
            _sum -= @object.AsDecimal();
        }

        public virtual object Value
        {
            get
            {
                if (_numDataPoints == 0)
                {
                    return null;
                }
                return _sum;
            }
        }

        public virtual Type ValueType
        {
            get { return typeof (decimal?); }
        }
    }
}
