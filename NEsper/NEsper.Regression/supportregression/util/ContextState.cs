///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.supportregression.util
{
    public class ContextState
    {
        public ContextState(int level, int parentPath, int subpath, int agentInstanceId, Object payload, bool started)
        {
            Level = level;
            ParentPath = parentPath;
            Subpath = subpath;
            AgentInstanceId = agentInstanceId;
            Payload = payload;
            IsStarted = started;
        }

        public int Level { get; private set; }

        public int ParentPath { get; private set; }

        public int Subpath { get; private set; }

        public int AgentInstanceId { get; private set; }

        public object Payload { get; private set; }

        public bool IsStarted { get; private set; }
    }
}
