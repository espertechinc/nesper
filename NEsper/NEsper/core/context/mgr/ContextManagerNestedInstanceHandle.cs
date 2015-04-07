///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.core.context.mgr
{
    public class ContextManagerNestedInstanceHandle : ContextControllerInstanceHandle
    {
        private readonly int _subPathId;
        private readonly ContextController _controller;
        private readonly int _contextPartitionOrPathId;
        private readonly bool _branch;
        private readonly ContextControllerTreeAgentInstanceList _branchAgentInstances;
    
        public ContextManagerNestedInstanceHandle(int subPathId, ContextController controller, int contextPartitionOrPathId, bool branch, ContextControllerTreeAgentInstanceList branchAgentInstances)
        {
            _subPathId = subPathId;
            _controller = controller;
            _contextPartitionOrPathId = contextPartitionOrPathId;
            _branch = branch;
            _branchAgentInstances = branchAgentInstances;
        }

        public int SubPathId
        {
            get { return _subPathId; }
        }

        public ContextController Controller
        {
            get { return _controller; }
        }

        public int ContextPartitionOrPathId
        {
            get { return _contextPartitionOrPathId; }
        }

        public bool IsBranch
        {
            get { return _branch; }
        }

        public ContextControllerTreeAgentInstanceList Instances
        {
            get { return _branchAgentInstances; }
        }
    }
}
