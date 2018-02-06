///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.epl.agg.aggregator
{
    /// <summary>Sum for any number value. </summary>
    public class AggregatorSumNumInteger : AggregationMethod
    {
        private int _sum;
        private long _numDataPoints;

        public AggregatorSumNumInteger()
        {
        }

        public virtual void Clear()
        {
            _sum = 0;
            _numDataPoints = 0;
        }

        public virtual void Enter(Object @object)
        {
            if (@object == null)
            {
                return;
            }
            _numDataPoints++;
            _sum += @object.AsInt();
        }

        public virtual void Leave(Object @object)
        {
            if (@object == null)
            {
                return;
            }
            if (_numDataPoints <= 1)
            {
                Clear();
            }
            else
            {
                _numDataPoints--;
                _sum -= @object.AsInt();
            }
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
    }
}