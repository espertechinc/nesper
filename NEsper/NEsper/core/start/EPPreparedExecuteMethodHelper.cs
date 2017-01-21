///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.core.start
{
    /// <summary>
    ///     Starts and provides the stop method for EPL statements.
    /// </summary>
    public class EPPreparedExecuteMethodHelper
    {
        internal static void ValidateFAFQuery(StatementSpecCompiled statementSpec)
        {
            for (int i = 0; i < statementSpec.StreamSpecs.Length; i++)
            {
                var streamSpec = statementSpec.StreamSpecs[i];
                if (!(streamSpec is NamedWindowConsumerStreamSpec || streamSpec is TableQueryStreamSpec)) {
                    throw new ExprValidationException("On-demand queries require tables or named windows and do not allow event streams or patterns");
                }
                if (streamSpec.ViewSpecs.Length != 0) {
                    throw new ExprValidationException("Views are not a supported feature of on-demand queries");
                }
            }
            if (statementSpec.OutputLimitSpec != null)
            {
                throw new ExprValidationException(
                    "Output rate limiting is not a supported feature of on-demand queries");
            }
        }

        public static ICollection<int> GetAgentInstanceIds(
            FireAndForgetProcessor processor,
            ContextPartitionSelector selector,
            ContextManagementService contextManagementService,
            String contextName)
        {
            ICollection<int> agentInstanceIds;
            if (selector == null || selector is ContextPartitionSelectorAll)
            {
                agentInstanceIds = processor.GetProcessorInstancesAll();
            }
            else
            {
                ContextManager contextManager = contextManagementService.GetContextManager(contextName);
                if (contextManager == null)
                {
                    throw new EPException("Context by name '" + contextName + "' could not be found");
                }
                agentInstanceIds = contextManager.GetAgentInstanceIds(selector);
            }
            return agentInstanceIds;
        }
    }
}