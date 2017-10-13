///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.threading;
using com.espertech.esper.pattern;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.activator
{
    public class ViewableActivationResult
    {
        public ViewableActivationResult(
            Viewable viewable,
            StopCallback stopCallback,
            IReaderWriterLock optionalLock,
            EvalRootState optionalPatternRoot,
            EvalRootMatchRemover optEvalRootMatchRemover,
            bool suppressSameEventMatches,
            bool discardPartialsOnMatch,
            ViewableActivationResultExtension viewableActivationResultExtension)
        {
            Viewable = viewable;
            StopCallback = stopCallback;
            OptionalLock = optionalLock;
            OptionalPatternRoot = optionalPatternRoot;
            OptEvalRootMatchRemover = optEvalRootMatchRemover;
            IsSuppressSameEventMatches = suppressSameEventMatches;
            IsDiscardPartialsOnMatch = discardPartialsOnMatch;
            ViewableActivationResultExtension = viewableActivationResultExtension;
        }

        public StopCallback StopCallback { get; private set; }

        public Viewable Viewable { get; private set; }

        public IReaderWriterLock OptionalLock { get; private set; }

        public EvalRootState OptionalPatternRoot { get; private set; }

        public EvalRootMatchRemover OptEvalRootMatchRemover { get; private set; }

        public bool IsSuppressSameEventMatches { get; private set; }

        public bool IsDiscardPartialsOnMatch { get; private set; }

        public ViewableActivationResultExtension ViewableActivationResultExtension { get; set; }
    }
}