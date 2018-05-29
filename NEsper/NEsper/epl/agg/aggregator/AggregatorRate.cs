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
    ///     Aggregation computing an event arrival rate for data windowed-events.
    /// </summary>
    public class AggregatorRate : AggregationMethod
    {
        private bool _isSet;

        public AggregatorRate(long oneSecondTime)
        {
            OneSecondTime = oneSecondTime;
        }

        public long OneSecondTime { get; }

        public double Accumulator { get; set; }

        public long Latest { get; set; }

        public long Oldest { get; set; }

        public virtual void Enter(object value)
        {
            if (value is Array array)
                EnterValueArr(array);
            else
                EnterValueSingle(value);
        }

        public virtual void Leave(object value)
        {
            if (value is Array array)
                LeaveValueArr(array);
            else
                LeaveValueSingle(value);
        }

        public object Value
        {
            get
            {
                if (!_isSet) return null;
                return Accumulator * OneSecondTime / (Latest - Oldest);
            }
        }

        public void Clear()
        {
            Accumulator = 0;
            Latest = 0;
            Oldest = 0;
        }

        public bool IsSet()
        {
            return _isSet;
        }

        public void SetSet(bool set)
        {
            _isSet = set;
        }

        protected void EnterValueSingle(object value)
        {
            Accumulator++;
            Latest = (long) value;
        }

        protected void EnterValueArr(Array parameters)
        {
            var val = parameters.GetValue(1);
            Accumulator += val.AsDouble();
            Latest = parameters.GetValue(0).AsLong();
        }

        protected void LeaveValueArr(Array parameters)
        {
            var val = parameters.GetValue(1);
            Accumulator -= val.AsDouble();
            Oldest = parameters.GetValue(0).AsLong();
            if (!_isSet) _isSet = true;
        }

        protected void LeaveValueSingle(object value)
        {
            Accumulator--;
            Oldest = (long) value;
            if (!_isSet) _isSet = true;
        }
    }
} // end of namespace