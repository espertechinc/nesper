///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    /// Default pattern context factory.
    /// </summary>
    public class PatternContextFactoryDefault : PatternContextFactory
    {
        /// <summary>
        /// Create a pattern context.
        /// </summary>
        /// <param name="statementContext">is the statement information and services</param>
        /// <param name="streamId">is the stream id</param>
        /// <param name="rootNode">is the pattern root node</param>
        /// <param name="matchedEventMapMeta"></param>
        /// <param name="allowResilient"></param>
        /// <returns>pattern context</returns>
        public PatternContext CreateContext(StatementContext statementContext,
                                            int streamId,
                                            EvalRootFactoryNode rootNode,
                                            MatchedEventMapMeta matchedEventMapMeta,
                                            bool allowResilient)
        {
            return new PatternContext(statementContext, streamId, matchedEventMapMeta, false);
        }

        public PatternAgentInstanceContext CreatePatternAgentContext(PatternContext patternContext, AgentInstanceContext agentInstanceContext, bool hasConsumingFilter)
        {
            return new PatternAgentInstanceContext(patternContext, agentInstanceContext, hasConsumingFilter);
        }
    }
}
