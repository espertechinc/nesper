///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.context.activator
{
    public class ViewableActivationResult
    {
        public ViewableActivationResult(
            Viewable viewable,
            AgentInstanceMgmtCallback stopCallback,
            EvalRootMatchRemover optEvalRootMatchRemover,
            bool suppressSameEventMatches,
            bool discardPartialsOnMatch,
            EvalRootState optionalPatternRoot,
            ViewableActivationResultExtension viewableActivationResultExtension,
            Runnable optPostContextMergeRunnable)
        {
            Viewable = viewable;
            StopCallback = stopCallback;
            OptEvalRootMatchRemover = optEvalRootMatchRemover;
            IsSuppressSameEventMatches = suppressSameEventMatches;
            IsDiscardPartialsOnMatch = discardPartialsOnMatch;
            OptionalPatternRoot = optionalPatternRoot;
            ViewableActivationResultExtension = viewableActivationResultExtension;
            OptPostContextMergeRunnable = optPostContextMergeRunnable;
        }

        public Viewable Viewable { get; }

        public AgentInstanceMgmtCallback StopCallback { get; }

        public ViewableActivationResultExtension ViewableActivationResultExtension { get; }

        public EvalRootMatchRemover OptEvalRootMatchRemover { get; }

        public bool IsSuppressSameEventMatches { get; }

        public bool IsDiscardPartialsOnMatch { get; }

        public EvalRootState OptionalPatternRoot { get; }

        public Runnable OptPostContextMergeRunnable { get; }
    }
} // end of namespace