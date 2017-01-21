///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// Factory for pattern context instances, creating context objects for each distinct 
    /// pattern based on the patterns root node and stream id.
    /// </summary>
    public interface PatternContextFactory
    {
        /// <summary>
        /// Create a pattern context.
        /// </summary>
        /// <param name="statementContext">is the statement information and services</param>
        /// <param name="streamId">is the stream id</param>
        /// <param name="rootNode">is the pattern root node</param>
        /// <param name="matchedEventMapMeta">The matched event map meta.</param>
        /// <param name="allowResilient">if set to <c>true</c> [allow resilient].</param>
        /// <returns>pattern context</returns>
        PatternContext CreateContext(StatementContext statementContext,
                                            int streamId,
                                            EvalRootFactoryNode rootNode,
                                            MatchedEventMapMeta matchedEventMapMeta,
                                            bool allowResilient);

        PatternAgentInstanceContext CreatePatternAgentContext(PatternContext patternContext,
                                                                     AgentInstanceContext agentInstanceContext,
                                                                     bool hasConsumingFilter);
    }
}
