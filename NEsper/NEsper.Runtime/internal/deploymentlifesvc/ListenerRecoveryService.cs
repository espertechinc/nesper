///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.runtime.client;

namespace com.espertech.esper.runtime.@internal.deploymentlifesvc
{
    using UpdateEventHandler = EventHandler<UpdateEventArgs>;

    public interface ListenerRecoveryService
    {
        void Put(
            int statementId,
            string statementName,
            UpdateEventHandler[] eventHandlersEventHandlers);

        IEnumerator<KeyValuePair<int, UpdateEventHandler[]>> EventHandlers { get; }

        void Remove(int statementId);
    }
} // end of namespace