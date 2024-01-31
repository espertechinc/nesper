///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading.threadlocal;

namespace com.espertech.esper.common.@internal.statement.multimatch
{
    public class MultiMatchHandlerNoSubqueryWDedup : MultiMatchHandler
    {
        private readonly IThreadLocal<LinkedHashSet<FilterHandleCallback>> dedupes =
            new FastThreadLocal<LinkedHashSet<FilterHandleCallback>>(
                () => new LinkedHashSet<FilterHandleCallback>());

        protected internal MultiMatchHandlerNoSubqueryWDedup()
        {
        }

        public void Handle(
            ICollection<FilterHandleCallback> callbacks,
            EventBean theEvent)
        {
            if (callbacks.Count >= 8) {
                var dedup = dedupes.GetOrCreate();
                dedup.Clear();
                dedup.AddAll(callbacks);
                foreach (var callback in dedup) {
                    callback.MatchFound(theEvent, callbacks);
                }

                dedup.Clear();
            }
            else {
                var count = 0;
                foreach (var callback in callbacks) {
                    var haveInvoked = CheckDup(callback, callbacks, count);
                    if (!haveInvoked) {
                        callback.MatchFound(theEvent, callbacks);
                    }

                    count++;
                }
            }
        }

        private bool CheckDup(
            FilterHandleCallback callback,
            ICollection<FilterHandleCallback> callbacks,
            int count)
        {
            if (count < 1) {
                return false;
            }

            var index = 0;
            foreach (var candidate in callbacks) {
                if (candidate == callback) {
                    return true;
                }

                index++;
                if (index == count) {
                    break;
                }
            }

            return false;
        }
    }
} // end of namespace