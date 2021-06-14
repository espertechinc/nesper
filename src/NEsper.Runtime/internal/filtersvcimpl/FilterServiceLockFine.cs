///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    public class FilterServiceLockFine : FilterServiceBase
    {
        private readonly IReaderWriterLock _lock;

        public FilterServiceLockFine(
            IReaderWriterLockManager rwLockManager,
            int stageId) :
            base(new FilterServiceGranularLockFactoryReentrant(rwLockManager), stageId)
        {
            _lock = rwLockManager.CreateLock(GetType());
        }

        public override void AcquireWriteLock()
        {
            _lock.WriteLock.Acquire();
        }

        public override void ReleaseWriteLock()
        {
            _lock.WriteLock.Release();
        }

        public override IDictionary<EventTypeIdPair, IDictionary<int, IList<FilterItem[]>>> Get(ISet<int> statementId)
        {
            using (_lock.ReadLock.Acquire()) {
                return GetInternal(statementId);
            }
        }

        public override long Evaluate(
            EventBean theEvent,
            ICollection<FilterHandle> matches,
            ExprEvaluatorContext ctx)
        {
            using (_lock.ReadLock.Acquire()) {
                return EvaluateInternal(theEvent, matches, ctx);
            }
        }

        public override long Evaluate(
            EventBean theEvent,
            ICollection<FilterHandle> matches,
            int statementId,
            ExprEvaluatorContext ctx)
        {
            using (_lock.ReadLock.Acquire()) {
                return EvaluateInternal(theEvent, matches, statementId, ctx);
            }
        }

        public override void Add(
            EventType eventType,
            FilterValueSetParam[][] valueSet,
            FilterHandle callback)
        {
            AddInternal(eventType, valueSet, callback);
        }

        public override void Remove(
            FilterHandle callback,
            EventType eventType,
            FilterValueSetParam[][] valueSet)
        {
            RemoveInternal(callback, eventType, valueSet);
        }

        public override void RemoveType(EventType type)
        {
            RemoveTypeInternal(type);
        }
    }
} // end of namespace