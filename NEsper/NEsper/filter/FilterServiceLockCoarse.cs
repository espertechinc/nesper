///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.filter
{
    public sealed class FilterServiceLockCoarse : FilterServiceBase
    {
        private readonly IReaderWriterLock _iLock =
            ReaderWriterLockManager.CreateDefaultLock();

        public FilterServiceLockCoarse()
            : base(new FilterServiceGranularLockFactoryNone())
        {
        }

        public override ILockable WriteLock
        {
            get { return _iLock.WriteLock; }
        }

        public override FilterSet Take(ICollection<String> statementId)
        {
            using(_iLock.WriteLock.Acquire())
            {
                return base.TakeInternal(statementId);
            }
        }

        public override void Apply(FilterSet filterSet)
        {
            using(_iLock.WriteLock.Acquire())
            {
                base.ApplyInternal(filterSet);
            }
        }

        public override long Evaluate(EventBean theEvent, ICollection<FilterHandle> matches)
        {
            using (_iLock.ReadLock.Acquire())
            {
                return base.EvaluateInternal(theEvent, matches);
            }
        }

        public override long Evaluate(EventBean theEvent, ICollection<FilterHandle> matches, String statementId)
        {
            using (_iLock.ReadLock.Acquire())
            {
                return base.EvaluateInternal(theEvent, matches, statementId);
            }
        }

        public override void Add(FilterValueSet filterValueSet, FilterHandle callback)
        {
            using (_iLock.WriteLock.Acquire())
            {
                base.AddInternal(filterValueSet, callback);
            }
        }

        public override void Remove(FilterHandle callback)
        {
            using (_iLock.WriteLock.Acquire())
            {
                base.RemoveInternal(callback);
            }
        }

        public override void RemoveType(EventType type)
        {
            using (_iLock.WriteLock.Acquire())
            {
                base.RemoveTypeInternal(type);
            }
        }
    }
}
