///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.context;

namespace com.espertech.esper.core.context.util
{
    public interface ContextEnumeratorHandler
    {
        IEnumerator<EventBean> GetEnumerator(int statementId);
        IEnumerator<EventBean> GetSafeEnumerator(int statementId);
        IEnumerator<EventBean> GetEnumerator(int statementId, ContextPartitionSelector selector);
        IEnumerator<EventBean> GetSafeEnumerator(int statementId, ContextPartitionSelector selector);

    }
}
