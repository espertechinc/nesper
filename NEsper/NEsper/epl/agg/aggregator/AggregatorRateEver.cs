///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.schedule;

namespace com.espertech.esper.epl.agg.aggregator
{
    /// <summary>
    /// Aggregation computing an event arrival rate for with and without data window.
    /// </summary>
    public class AggregatorRateEver : AggregationMethod
    {
        private readonly long _interval;
        private readonly LinkedList<Int64> _points;
        private bool _hasLeave = false;
        private readonly TimeProvider _timeProvider;
    
        /// <summary>Ctor. </summary>
        /// <param name="interval">rate interval</param>
        /// <param name="timeProvider">time</param>
        public AggregatorRateEver(long interval, TimeProvider timeProvider) {
            _interval = interval;
            _timeProvider = timeProvider;
            _points = new LinkedList<Int64>();
        }
    
        public virtual void Clear()
        {
            _points.Clear();
        }
    
        public virtual void Enter(Object @object)
        {
            long timestamp = _timeProvider.Time;
            _points.AddLast(timestamp);
            RemoveFromHead(timestamp);
        }
    
        public virtual void Leave(Object @object)
        {
            // This is an "ever" aggregator and is designed for use in non-window env
        }

        public object Value
        {
            get
            {
                if (_points.IsNotEmpty())
                {
                    long newest = _points.Last.Value;
                    RemoveFromHead(newest);
                }
                if (!_hasLeave)
                {
                    return null;
                }
                if (_points.IsEmpty())
                {
                    return 0d;
                }
                return (_points.Count*1000d)/_interval;
            }
        }

        private void RemoveFromHead(long timestamp) {
            if (_points.Count > 1)
            {
                while (true)
                {
                    long first = _points.First.Value;
                    long delta = timestamp - first;
                    if (delta >= _interval)
                    {
                        _points.RemoveFirst();
                        _hasLeave = true;
                    }
                    else if (delta == _interval)
                    {
                        _hasLeave = true;
                        break;
                    }
                    else
                    {
                        break;
                    }
                    if (_points.IsEmpty())
                    {
                        break;
                    }
                }
            }
        }
    }
}
