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
    /// Aggregation computing an event arrival rate for data windowed-events.
    /// </summary>
    public class AggregatorRate : AggregationMethod
    {
        private double _accumulator;
        private long _latest;
        private long _oldest;
        private bool _isSet = false;

        public virtual void Enter(Object value)
        {
            if (value.GetType().IsArray)
            {
                var parameters = (Object[])value;
                _accumulator += parameters[1].AsDouble();
                _latest = parameters[0].AsLong();
            }
            else
            {
                _accumulator += 1;
                _latest = value.AsLong();
            }
        }

        public virtual void Leave(Object value)
        {
            if (value.GetType().IsArray)
            {
                var parameters = (Object[])value;
                _accumulator -= parameters[1].AsDouble();
                _oldest = parameters[0].AsLong();
            }
            else
            {
                _accumulator -= 1;
                _oldest = value.AsLong();
            }
            if (!_isSet) _isSet = true;
        }

        public virtual object Value
        {
            get
            {
                if (!_isSet)
                    return null;
                return (_accumulator * 1000) / (_latest - _oldest);
            }
        }

        public virtual void Clear()
        {
            _accumulator = 0;
            _latest = 0;
            _oldest = 0;
        }
    }
}
