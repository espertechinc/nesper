///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.filter;

namespace com.espertech.esper.core.service.multimatch
{
    public class MultiMatchHandlerSubqueryPostevalNoDedup : MultiMatchHandler
    {
        public static readonly MultiMatchHandlerSubqueryPostevalNoDedup INSTANCE =
            new MultiMatchHandlerSubqueryPostevalNoDedup();

        private MultiMatchHandlerSubqueryPostevalNoDedup()
        {
        }

        public void Handle(ICollection<FilterHandleCallback> callbacks, EventBean theEvent)
        {
            foreach (FilterHandleCallback callback in callbacks)
            {
                if (!callback.IsSubSelect)
                {
                    callback.MatchFound(theEvent, callbacks);
                }
            }
            foreach (FilterHandleCallback callback in callbacks)
            {
                if (callback.IsSubSelect)
                {
                    callback.MatchFound(theEvent, callbacks);
                }
            }
        }
    }
} // end of namespace
