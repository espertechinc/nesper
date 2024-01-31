///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.context.mgr
{
    public class ContextPartitionVisitorAgentInstanceId : ContextPartitionVisitor
    {
        private readonly int numLevels;

        public ContextPartitionVisitorAgentInstanceId(int numLevels)
        {
            this.numLevels = numLevels;
        }

        public ISet<int> Ids { get; } = new HashSet<int>();

        public void Add(
            int id,
            int nestingLevel)
        {
            if (nestingLevel == numLevels) {
                Ids.Add(id);
            }
        }
    }
} // end of namespace