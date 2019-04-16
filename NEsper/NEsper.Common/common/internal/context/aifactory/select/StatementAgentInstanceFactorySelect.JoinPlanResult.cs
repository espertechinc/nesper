///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.@join.@base;
using com.espertech.esper.common.@internal.epl.output.core;

namespace com.espertech.esper.common.@internal.context.aifactory.@select
{
    public partial class StatementAgentInstanceFactorySelect
    {
        internal class JoinPlanResult
        {
            internal JoinPlanResult(
                OutputProcessView viewable,
                JoinPreloadMethod preloadMethod,
                JoinSetComposerDesc joinSetComposerDesc)
            {
                Viewable = viewable;
                PreloadMethod = preloadMethod;
                JoinSetComposerDesc = joinSetComposerDesc;
            }

            public OutputProcessView Viewable { get; }

            public JoinPreloadMethod PreloadMethod { get; }

            public JoinSetComposerDesc JoinSetComposerDesc { get; }
        }
    }
}