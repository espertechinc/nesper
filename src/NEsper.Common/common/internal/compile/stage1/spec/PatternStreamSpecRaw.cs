///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.pattern.core;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    ///     Pattern specification in unvalidated, unoptimized form.
    /// </summary>
    public class PatternStreamSpecRaw
        : StreamSpecBase,
            StreamSpecRaw
    {
        public PatternStreamSpecRaw(
            EvalForgeNode evalForgeNode,
            ViewSpec[] viewSpecs,
            string optionalStreamName,
            StreamSpecOptions streamSpecOptions,
            bool suppressSameEventMatches,
            bool discardPartialsOnMatch)
            : base(optionalStreamName, viewSpecs, streamSpecOptions)
        {
            EvalForgeNode = evalForgeNode;
            IsSuppressSameEventMatches = suppressSameEventMatches;
            IsDiscardPartialsOnMatch = discardPartialsOnMatch;
        }

        /// <summary>
        ///     Returns the pattern expression evaluation node for the top pattern operator.
        /// </summary>
        /// <returns>parent pattern expression node</returns>
        public EvalForgeNode EvalForgeNode { get; }

        public bool IsSuppressSameEventMatches { get; }

        public bool IsDiscardPartialsOnMatch { get; }
    }
} // end of namespace