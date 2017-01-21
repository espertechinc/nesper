///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextControllerTreeEntry
    {
        public ContextControllerTreeEntry(ContextController parent,
                                          IDictionary<int, ContextController> childContexts,
                                          Object initPartitionKey,
                                          IDictionary<String, Object> initContextProperties)
        {
            Parent = parent;
            ChildContexts = childContexts;
            InitPartitionKey = initPartitionKey;
            InitContextProperties = initContextProperties;
        }

        public ContextController Parent { get; private set; }

        public IDictionary<int, ContextController> ChildContexts { get; set; }

        public object InitPartitionKey { get; private set; }

        public IDictionary<int, ContextControllerTreeAgentInstanceList> AgentInstances { get; set; }

        public IDictionary<string, object> InitContextProperties { get; private set; }
    }
}