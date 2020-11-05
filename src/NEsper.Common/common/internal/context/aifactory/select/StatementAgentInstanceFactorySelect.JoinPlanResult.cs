///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.join.@base;
using com.espertech.esper.common.@internal.epl.output.core;

namespace com.espertech.esper.common.@internal.context.aifactory.select
{
    public partial class StatementAgentInstanceFactorySelect
    {
        public class JoinPlanResult
        {
            private readonly OutputProcessView _outputProcessView;
            private readonly JoinPreloadMethod _preloadMethod;
            private readonly JoinSetComposerDesc _joinSetComposerDesc;

            public JoinPlanResult(
                OutputProcessView viewable,
                JoinPreloadMethod preloadMethod,
                JoinSetComposerDesc joinSetComposerDesc)
            {
                this._outputProcessView = viewable;
                this._preloadMethod = preloadMethod;
                this._joinSetComposerDesc = joinSetComposerDesc;
            }

            public OutputProcessView Viewable => _outputProcessView;

            public JoinPreloadMethod PreloadMethod => _preloadMethod;

            public JoinSetComposerDesc JoinSetComposerDesc => _joinSetComposerDesc;
        }
    }
}