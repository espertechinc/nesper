///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.statement.resource
{
    public class StatementResourceService
    {
        public StatementResourceService(bool partitioned)
        {
            if (partitioned) {
                ResourcesPartitioned = new Dictionary<int, StatementResourceHolder>();
            }
        }

        public StatementResourceHolder ResourcesUnpartitioned { get; private set; }

        public IDictionary<int, StatementResourceHolder> ResourcesPartitioned { get; }

        public StatementResourceHolder Unpartitioned {
            get => ResourcesUnpartitioned;
            set => ResourcesUnpartitioned = value;
        }

        public StatementResourceHolder GetPartitioned(int agentInstanceId)
        {
            return ResourcesPartitioned.Get(agentInstanceId);
        }

        public void SetPartitioned(int agentInstanceId, StatementResourceHolder statementResourceHolder)
        {
            ResourcesPartitioned.Put(agentInstanceId, statementResourceHolder);
        }

        public StatementResourceHolder DeallocatePartitioned(int agentInstanceId)
        {
            return ResourcesPartitioned.Delete(agentInstanceId);
        }

        public StatementResourceHolder DeallocateUnpartitioned()
        {
            var unpartitioned = ResourcesUnpartitioned;
            ResourcesUnpartitioned = null;
            return unpartitioned;
        }
    }
} // end of namespace