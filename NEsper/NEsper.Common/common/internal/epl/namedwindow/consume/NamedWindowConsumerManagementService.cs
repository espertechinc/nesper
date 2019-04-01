///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.epl.namedwindow.consume
{
    public interface NamedWindowConsumerManagementService
    {
        void AddConsumer(
            string namedWindowDeploymentId, 
            string namedWindowName,
            int namedWindowConsumerId,
            StatementContext statementContext, bool subquery);

        void DestroyConsumer(
            string namedWindowDeploymentId,
            string namedWindowName,
            StatementContext statementContext);
    }
} // end of namespace