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
    /// <summary>
    /// Standard deviation always generates double-typed numbers.
    /// </summary>
    public class AggregatorStddev : AggregationMethod
    {
        private double _mean;
        private double _qn;
        private long _numDataPoints;
    
        public virtual void Clear()
        {
            _mean = 0;
            _numDataPoints = 0;
            _qn = 0;
        }

        public virtual void Enter(Object @object)
        {
            if (@object == null)
            {
                return;
            }
    
            double p = (@object).AsDouble();
    
            // compute running variance per Knuth's method
            if (_numDataPoints == 0) {
                _mean = p;
                _qn = 0;
                _numDataPoints = 1;
            }
            else {
                _numDataPoints++;
                double oldmean = _mean;
                _mean += (p - _mean)/_numDataPoints;
                _qn += (p - oldmean)*(p - _mean);
            }
        }

        public virtual void Leave(Object @object)
        {
            if (@object == null)
            {
                return;
            }
    
            double p = (@object).AsDouble();
    
            // compute running variance per Knuth's method
            if (_numDataPoints <= 1) {
                Clear();
            }
            else {
                _numDataPoints--;
                double oldmean = _mean;
                _mean -= (p - _mean)/_numDataPoints;
                _qn -= (p - oldmean)*(p - _mean);
            }
        }

        public virtual object Value
        {
            get
            {
                if (_numDataPoints < 2)
                {
                    return null;
                }
                return Math.Sqrt(_qn/(_numDataPoints - 1));
            }
        }
    }
}
