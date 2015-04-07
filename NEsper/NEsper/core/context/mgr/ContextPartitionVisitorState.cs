///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client.context;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextPartitionVisitorState : ContextPartitionVisitor
    {
        public ContextPartitionVisitorState()
        {
            States = new OrderedDictionary<ContextStatePathKey, ContextStatePathValue>();
            ContextPartitionInfo = new Dictionary<int, ContextPartitionDescriptor>();
        }

        public void Visit(int nestingLevel, int pathId, ContextStatePathValueBinding binding, Object payload, ContextController contextController, ContextControllerInstanceHandle instanceHandle)
        {
            ContextStatePathKey key = new ContextStatePathKey(nestingLevel, pathId, instanceHandle.SubPathId);
            int agentInstanceId = instanceHandle.ContextPartitionOrPathId;
            States.Put(key, new ContextStatePathValue(agentInstanceId, binding.ToByteArray(payload), instanceHandle.Instances.State));
    
            ContextPartitionState state = instanceHandle.Instances.State;
            ContextPartitionIdentifier identifier = contextController.Factory.KeyPayloadToIdentifier(payload);
            ContextPartitionDescriptor descriptor = new ContextPartitionDescriptor(agentInstanceId, identifier, state);
            ContextPartitionInfo.Put(agentInstanceId, descriptor);
        }

        public OrderedDictionary<ContextStatePathKey, ContextStatePathValue> States { get; private set; }

        public IDictionary<int, ContextPartitionDescriptor> ContextPartitionInfo { get; private set; }
    }
}
