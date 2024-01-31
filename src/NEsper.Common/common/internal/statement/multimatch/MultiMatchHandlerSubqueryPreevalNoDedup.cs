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

namespace com.espertech.esper.common.@internal.statement.multimatch
{
    public class MultiMatchHandlerSubqueryPreevalNoDedup : MultiMatchHandler
    {
        protected internal static readonly MultiMatchHandlerSubqueryPreevalNoDedup INSTANCE =
            new MultiMatchHandlerSubqueryPreevalNoDedup();

        private MultiMatchHandlerSubqueryPreevalNoDedup()
        {
        }

        public void Handle(
            ICollection<FilterHandleCallback> callbacks,
            EventBean theEvent)
        {
            foreach (var callback in callbacks) {
                if (callback.IsSubSelect) {
                    callback.MatchFound(theEvent, callbacks);
                }
            }

            foreach (var callback in callbacks) {
                if (!callback.IsSubSelect) {
                    callback.MatchFound(theEvent, callbacks);
                }
            }
        }
    }
} // end of namespace