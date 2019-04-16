///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.settings;

namespace com.espertech.esper.common.@internal.epl.agg.access.core
{
    public class AggregationAgentForgeFactory
    {
        public static AggregationAgentForge Make(
            int streamNum,
            ExprNode optionalFilter,
            ImportService importService,
            bool isFireAndForget,
            string statementName)
        {
            ExprForge evaluator = optionalFilter == null ? null : optionalFilter.Forge;
            if (streamNum == 0) {
                if (optionalFilter == null) {
                    return AggregationAgentDefault.INSTANCE;
                }
                else {
                    return new AggregationAgentDefaultWFilterForge(evaluator);
                }
            }
            else {
                if (optionalFilter == null) {
                    return new AggregationAgentRewriteStreamForge(streamNum);
                }
                else {
                    return new AggregationAgentRewriteStreamWFilterForge(streamNum, evaluator);
                }
            }
        }
    }
} // end of namespace