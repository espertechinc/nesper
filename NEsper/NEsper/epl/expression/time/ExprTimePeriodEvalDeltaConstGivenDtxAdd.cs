///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.util;

namespace com.espertech.esper.epl.expression.time
{
    public class ExprTimePeriodEvalDeltaConstGivenDtxAdd
        : ExprTimePeriodEvalDeltaConst
        , ExprTimePeriodEvalDeltaConstFactory
    {
        private readonly DateTimeEx _dateTime;
        private readonly ExprTimePeriodImpl.TimePeriodAdder[] _adders;
        private readonly int[] _added;
        private readonly TimeAbacus _timeAbacus;
        private readonly int _indexMicroseconds;
        private readonly ILockable _iLock;

        public ExprTimePeriodEvalDeltaConstGivenDtxAdd(
            ILockManager lockManager,
            ExprTimePeriodImpl.TimePeriodAdder[] adders,
            int[] added,
            TimeZoneInfo timeZone,
            TimeAbacus timeAbacus)
        {
            _iLock = lockManager.CreateLock(MethodBase.GetCurrentMethod().DeclaringType);
            _adders = adders;
            _added = added;
            _dateTime = new DateTimeEx(DateTimeOffset.Now, timeZone);
            _timeAbacus = timeAbacus;
            _indexMicroseconds = ExprTimePeriodUtil.FindIndexMicroseconds(adders);
        }

        public ExprTimePeriodEvalDeltaConst Make(
            string validateMsgName,
            string validateMsgValue,
            AgentInstanceContext agentInstanceContext)
        {
            return this;
        }

        public bool EqualsTimePeriod(ExprTimePeriodEvalDeltaConst otherComputation)
        {
            if (otherComputation is ExprTimePeriodEvalDeltaConstGivenDtxAdd)
            {
                var other = (ExprTimePeriodEvalDeltaConstGivenDtxAdd) otherComputation;
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

        public long DeltaAdd(long fromTime)
        {
            using (_iLock.Acquire())
            {
                long target = AddSubtract(fromTime, 1);
                return target - fromTime;
            }
        }

        public long DeltaSubtract(long fromTime)
        {
            using (_iLock.Acquire())
            {
                long target = AddSubtract(fromTime, -1);
                return fromTime - target;
            }
        }

        public ExprTimePeriodEvalDeltaResult DeltaAddWReference(long fromTime, long reference)
        {
            // find the next-nearest reference higher then the current time, compute delta, return reference one lower
            if (reference > fromTime)
            {
                while (reference > fromTime)
                {
                    reference = reference - DeltaSubtract(reference);
                }
            }

            long next = reference;
            long last;
            do
            {
                last = next;
                next = next + DeltaAdd(last);
            } while (next <= fromTime);
            return new ExprTimePeriodEvalDeltaResult(next - fromTime, last);
        }

        private long AddSubtract(long fromTime, int factor)
        {
            long remainder = _timeAbacus.CalendarSet(fromTime, _dateTime);
            for (int i = 0; i < _adders.Length; i++)
            {
                _adders[i].Add(_dateTime, factor*_added[i]);
            }
            long result = _timeAbacus.CalendarGet(_dateTime, remainder);
            if (_indexMicroseconds != -1)
            {
                result += factor*_added[_indexMicroseconds];
            }
            return result;
        }
    }
} // end of namespace
