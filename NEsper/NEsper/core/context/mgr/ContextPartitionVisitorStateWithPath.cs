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
    public class ContextPartitionVisitorStateWithPath : ContextPartitionVisitorWithPath
    {
        private readonly ContextControllerFactory[] _nestedFactories;
        private readonly IDictionary<ContextController, ContextControllerTreeEntry> _subcontexts;

        public ContextPartitionVisitorStateWithPath(ContextControllerFactory[] nestedFactories, IDictionary<ContextController, ContextControllerTreeEntry> subcontexts)
        {
            AgentInstanceInfo = new Dictionary<int, ContextPartitionDescriptor>();
            ControllerAgentInstances = new Dictionary<ContextController, IList<LeafDesc>>();
            Subpaths = new List<int>();
            States = new OrderedDictionary<ContextStatePathKey, ContextStatePathValue>();
            _nestedFactories = nestedFactories;
            _subcontexts = subcontexts;
        }
    
        public void Visit(int nestingLevel, int pathId, ContextStatePathValueBinding binding, Object payload, ContextController contextController, ContextControllerInstanceHandle instanceHandle)
        {
            var key = new ContextStatePathKey(nestingLevel, pathId, instanceHandle.SubPathId);
            int maxNestingLevel = _nestedFactories.Length;
    
            int contextPartitionOrSubPath = instanceHandle.ContextPartitionOrPathId;
    
            if (contextController.Factory.FactoryContext.NestingLevel == maxNestingLevel) {
                var agentInstances = ControllerAgentInstances.Get(contextController);
                if (agentInstances == null) {
                    agentInstances = new List<LeafDesc>();
                    ControllerAgentInstances.Put(contextController, agentInstances);
                }
                var value = new ContextStatePathValue(contextPartitionOrSubPath, binding.ToByteArray(payload), instanceHandle.Instances.State);
                agentInstances.Add(new LeafDesc(key, value));
    
                // generate a nice payload text from the paths of keys
                var entry = _subcontexts.Get(contextController);
                var keys = ContextManagerNested.GetTreeCompositeKey(_nestedFactories, payload, entry, _subcontexts);
                var descriptor = new ContextPartitionDescriptor(contextPartitionOrSubPath, new ContextPartitionIdentifierNested(keys), value.State);
                AgentInstanceInfo.Put(contextPartitionOrSubPath, descriptor);
                States.Put(key, value);
            }
            else {
                // handle non-leaf
                Subpaths.Add(contextPartitionOrSubPath);
                States.Put(key, new ContextStatePathValue(contextPartitionOrSubPath, binding.ToByteArray(payload), ContextPartitionState.STARTED));
            }
        }

        public OrderedDictionary<ContextStatePathKey, ContextStatePathValue> States { get; private set; }

        public void ResetSubPaths()
        {
            Subpaths.Clear();
        }

        public ICollection<int> Subpaths { get; private set; }

        public IDictionary<ContextController, IList<LeafDesc>> ControllerAgentInstances { get; private set; }

        public IDictionary<int, ContextPartitionDescriptor> AgentInstanceInfo { get; private set; }

        public class LeafDesc
        {
            public LeafDesc(ContextStatePathKey key, ContextStatePathValue value) {
                Key = key;
                Value = value;
            }

            public ContextStatePathKey Key;
            public ContextStatePathValue Value;
        }
    }
}
