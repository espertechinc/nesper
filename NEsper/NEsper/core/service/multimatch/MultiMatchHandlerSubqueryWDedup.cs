///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.filter;

namespace com.espertech.esper.core.service.multimatch
{
    public class MultiMatchHandlerSubqueryWDedup : MultiMatchHandler
    {
        private readonly bool _subselectPreeval;

        public MultiMatchHandlerSubqueryWDedup(bool subselectPreeval)
        {
            _subselectPreeval = subselectPreeval;
        }

        public void Handle(ICollection<FilterHandleCallback> callbacks, EventBean theEvent)
        {

            var dedup = MultiMatchHandlerNoSubqueryWDedup.Dedups.GetOrCreate();
            dedup.Clear();
            dedup.AddAll(callbacks);

            if (_subselectPreeval)
            {
                // sub-selects always go first
                foreach (var callback in dedup)
                {
                    if (callback.IsSubSelect)
                    {
                        callback.MatchFound(theEvent, dedup);
                    }
                }

                foreach (var callback in dedup)
                {
                    if (!callback.IsSubSelect)
                    {
                        callback.MatchFound(theEvent, dedup);
                    }
                }
            }
            else
            {
                // sub-selects always go last
                foreach (var callback in dedup)
                {
                    if (!callback.IsSubSelect)
                    {
                        callback.MatchFound(theEvent, dedup);
                    }
                }

                foreach (var callback in dedup)
                {
                    if (callback.IsSubSelect)
                    {
                        callback.MatchFound(theEvent, dedup);
                    }
                }
            }

            dedup.Clear();
        }
    }
} // end of namespace
