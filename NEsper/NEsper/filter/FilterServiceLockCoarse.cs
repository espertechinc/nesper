///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.filter
{
    public sealed class FilterServiceLockCoarse : FilterServiceBase
    {
        private readonly IReaderWriterLock _iLock;

        public FilterServiceLockCoarse(
            ILockManager lockManager, 
            IReaderWriterLockManager rwLockManager,
            bool allowIsolation)
            : base(lockManager, FilterServiceGranularLockFactoryNone.Instance, allowIsolation)
        {
            _iLock = rwLockManager.CreateLock(GetType());
        }

        public override ILockable WriteLock
        {
            get { return _iLock.WriteLock; }
        }

        public override FilterSet Take(ICollection<int> statementId)
        {
            using(_iLock.AcquireWriteLock())
            {
                return base.TakeInternal(statementId);
            }
        }

        public override void Apply(FilterSet filterSet)
        {
            using(_iLock.AcquireWriteLock())
            {
                base.ApplyInternal(filterSet);
            }
        }

        public override long Evaluate(EventBean theEvent, ICollection<FilterHandle> matches)
        {
            using (_iLock.AcquireReadLock())
            {
                return base.EvaluateInternal(theEvent, matches);
            }
        }

        public override long Evaluate(EventBean theEvent, ICollection<FilterHandle> matches, int statementId)
        {
            using (_iLock.AcquireReadLock())
            {
                return base.EvaluateInternal(theEvent, matches, statementId);
            }
        }

        public override FilterServiceEntry Add(FilterValueSet filterValueSet, FilterHandle callback)
        {
            using (_iLock.AcquireWriteLock())
            {
                return base.AddInternal(filterValueSet, callback);
            }
        }

        public override void Remove(FilterHandle callback, FilterServiceEntry filterServiceEntry)
        {
            using (_iLock.AcquireWriteLock())
            {
                base.RemoveInternal(callback, filterServiceEntry);
            }
        }

        public override void RemoveType(EventType type)
        {
            using (_iLock.AcquireWriteLock())
            {
                base.RemoveTypeInternal(type);
            }
        }
    }
}
