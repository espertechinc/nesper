///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.context.controller.core
{
    public abstract class ContextControllerBase : ContextController
    {
        internal readonly ContextManagerRealization realization;

        public ContextControllerBase(ContextManagerRealization realization)
        {
            this.realization = realization;
        }

        public virtual ContextManagerRealization Realization => realization;

        public abstract ContextControllerFactory Factory { get; }

        public abstract void Activate(
            IntSeqKey path,
            object[] parentPartitionKeys,
            EventBean optionalTriggeringEvent,
            IDictionary<string, object> optionalTriggeringPattern);

        public abstract void Deactivate(
            IntSeqKey path,
            bool terminateChildContexts);

        public abstract void VisitSelectedPartitions(
            IntSeqKey path,
            ContextPartitionSelector selector,
            ContextPartitionVisitor visitor,
            ContextPartitionSelector[] selectorPerLevel);

        public abstract void Transfer(
            IntSeqKey path,
            bool transferChildContexts,
            AgentInstanceTransferServices xfer);

        public abstract void Destroy();
    }
} // end of namespace