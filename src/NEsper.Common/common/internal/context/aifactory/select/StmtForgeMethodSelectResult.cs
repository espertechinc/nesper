///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage3;

namespace com.espertech.esper.common.@internal.context.aifactory.select
{
    public class StmtForgeMethodSelectResult
    {
        public StmtForgeMethodSelectResult(
            StmtForgeMethodResult forgeResult,
            EventType eventType,
            int numStreams)
        {
            ForgeResult = forgeResult;
            EventType = eventType;
            NumStreams = numStreams;
        }

        public StmtForgeMethodResult ForgeResult { get; }

        public EventType EventType { get; }

        public int NumStreams { get; }
    }
} // end of namespace