///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.namedwindow.consume
{
    public class NamedWindowConsumerManagementServiceImpl : NamedWindowConsumerManagementService
    {
        public static readonly NamedWindowConsumerManagementServiceImpl INSTANCE =
            new NamedWindowConsumerManagementServiceImpl();

        private NamedWindowConsumerManagementServiceImpl()
        {
        }

        public int Count => 0;

        public void AddConsumer(
            string namedWindowDeploymentId, string namedWindowName, int namedWindowConsumerId,
            StatementContext statementContext, bool subquery)
        {
        }

        public void DestroyConsumer(
            string namedWindowDeploymentId, string namedWindowName, StatementContext statementContext)
        {
        }
    }
} // end of namespace