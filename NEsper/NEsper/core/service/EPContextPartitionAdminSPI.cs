///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.context;
using com.espertech.esper.core.context.mgr;

namespace com.espertech.esper.core.service
{
    public interface EPContextPartitionAdminSPI : EPContextPartitionAdmin
    {
        EPContextPartitionExtract ExtractDestroyPaths(String contextName, ContextPartitionSelector selector);
        EPContextPartitionExtract ExtractStopPaths(String contextName, ContextPartitionSelector selector);
    
        EPContextPartitionExtract ExtractPaths(String contextName, ContextPartitionSelector selector);
        EPContextPartitionImportResult ImportStartPaths(String contextName, EPContextPartitionImportable importable, AgentInstanceSelector agentInstanceSelector);
    }
}
