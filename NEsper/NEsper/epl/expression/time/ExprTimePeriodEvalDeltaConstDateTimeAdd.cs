///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.epl.expression.time
{
    public class ExprTimePeriodEvalDeltaConstDateTimeAdd : ExprTimePeriodEvalDeltaConst
    {
        private DateTimeEx _dateTime;
        private readonly ExprTimePeriodImpl.TimePeriodAdder[] _adders;
        private readonly int[] _added;

        public ExprTimePeriodEvalDeltaConstDateTimeAdd(ExprTimePeriodImpl.TimePeriodAdder[] adders, int[] added, TimeZoneInfo timeZone)
        {
            _adders = adders;
            _added = added;
            _dateTime = new DateTimeEx(DateTimeOffsetHelper.Now(timeZone), timeZone);
        }
    
        public bool EqualsTimePeriod(ExprTimePeriodEvalDeltaConst otherComputation)
        {
            if (otherComputation is ExprTimePeriodEvalDeltaConstDateTimeAdd)
            {
                var other = (ExprTimePeriodEvalDeltaConstDateTimeAdd) otherComputation;
                if (other._adders.Length != _adders.Length)
                {
                    return false;
                }
                for (int i = 0; i < _adders.Length; i++)
                {
                    if (_added[i] != other._added[i] || _adders[i].GetType() != other._adders[i].GetType())
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    
        public long DeltaMillisecondsAdd(long fromTime)
        {
            lock (this)
            {
                _dateTime.SetUtcMillis(fromTime);
                AddSubtract(_adders, _added, _dateTime, 1);
                return _dateTime.TimeInMillis - fromTime;
            }
        }
    
        public long DeltaMillisecondsSubtract(long fromTime)
        {
            lock (this)
            {
                _dateTime.SetUtcMillis(fromTime);
                AddSubtract(_adders, _added, _dateTime, -1);
                return fromTime - _dateTime.TimeInMillis;
            }
        }

        public ExprTimePeriodEvalDeltaResult DeltaMillisecondsAddWReference(long fromTime, long reference)
        {
            lock (this)
            {
                // find the next-nearest reference higher then the current time, compute delta, return reference one lower
                if (reference > fromTime)
                {
                    while (reference > fromTime)
                    {
                        reference = reference - DeltaMillisecondsSubtract(reference);
                    }
                }

                long next = reference;
                long last;
                do
                {
                    last = next;
                    next = next + DeltaMillisecondsAdd(last);
                } while (next <= fromTime);
                return new ExprTimePeriodEvalDeltaResult(next - fromTime, last);
            }
        }

        private static void AddSubtract(ExprTimePeriodImpl.TimePeriodAdder[] adders, int[] added, DateTimeEx dateTime, int factor)
        {
            for (int i = 0; i < adders.Length; i++)
            {
                adders[i].Add(dateTime, factor*added[i]);
            }
        }
    }
}
