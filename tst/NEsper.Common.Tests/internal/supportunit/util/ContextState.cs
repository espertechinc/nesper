///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.supportunit.util
{
    public class ContextState
    {
        public ContextState(
            int level,
            int parentPath,
            int subpath,
            int agentInstanceId,
            object payload,
            bool started)
        {
            Level = level;
            ParentPath = parentPath;
            Subpath = subpath;
            AgentInstanceId = agentInstanceId;
            Payload = payload;
            IsStarted = started;
        }

        public int Level { get; }

        public int ParentPath { get; }

        public int Subpath { get; }

        public int AgentInstanceId { get; }

        public object Payload { get; }

        public bool IsStarted { get; }
    }
} // end of namespace
