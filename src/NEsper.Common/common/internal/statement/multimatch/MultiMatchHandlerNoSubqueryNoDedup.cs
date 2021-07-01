///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
    public class MultiMatchHandlerNoSubqueryNoDedup : MultiMatchHandler
    {
        protected internal static readonly MultiMatchHandlerNoSubqueryNoDedup INSTANCE =
            new MultiMatchHandlerNoSubqueryNoDedup();

        private MultiMatchHandlerNoSubqueryNoDedup()
        {
        }

        public void Handle(
            ICollection<FilterHandleCallback> callbacks,
            EventBean theEvent)
        {
            foreach (var callback in callbacks) {
                callback.MatchFound(theEvent, callbacks);
            }
        }
    }
} // end of namespace