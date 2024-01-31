///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.runtime.client;

namespace com.espertech.esper.runtime.@internal.deploymentlifesvc
{
    public interface ListenerRecoveryService
    {
        IEnumerator<KeyValuePair<int, UpdateListener[]>> Listeners { get; }

        void Put(
            int statementId,
            string statementName,
            UpdateListener[] updateListeners);

        void Remove(int statementId);
    }
} // end of namespace