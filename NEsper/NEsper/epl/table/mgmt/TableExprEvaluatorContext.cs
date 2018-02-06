///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.epl.table.mgmt
{
    public class TableExprEvaluatorContext
    {
        private readonly IThreadLocal<IDictionary<ILockable, IDisposable>> _threadLocal;

        public TableExprEvaluatorContext(IThreadLocalManager threadLocalManager)
        {
            _threadLocal = threadLocalManager.Create<IDictionary<ILockable, IDisposable>>(
                () => new Dictionary<ILockable, IDisposable>());
        }

        /// <summary>
        /// Adds the acquired lock.  If the lock does not already belong to the context, then it will be locked and
        /// the disposable lock will be returned.
        /// </summary>
        /// <param name="lock">The lock.</param>
        /// <returns></returns>
        public IDisposable AddAcquiredLock(ILockable @lock)
        {
            var table = _threadLocal.GetOrCreate();
            if (table.ContainsKey(@lock))
                return null;

            var latch = @lock.Acquire();
            return table[@lock] = latch;
        }
    
        public void ReleaseAcquiredLocks()
        {
            var table = _threadLocal.Value;
            if (table == null || table.IsEmpty()) {
                return;
            }

            foreach (var latch in table.Values)
            {
                latch.Dispose();
            }

            table.Clear();
        }

        public int LockHeldCount
        {
            get
            {
                var table = _threadLocal.Value;
                return table != null ? table.Count : 0;
            }
        }
    }
}
