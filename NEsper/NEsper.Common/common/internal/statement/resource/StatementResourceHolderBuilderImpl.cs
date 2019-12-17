///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.statement.resource
{
    public class StatementResourceHolderBuilderImpl : StatementResourceHolderBuilder
    {
        public static readonly StatementResourceHolderBuilderImpl INSTANCE = new StatementResourceHolderBuilderImpl();

        private StatementResourceHolderBuilderImpl()
        {
        }

        public StatementResourceHolder Build(
            AgentInstanceContext agentInstanceContext,
            StatementAgentInstanceFactoryResult resultOfStart)
        {
            return StatementResourceHolderUtil.PopulateHolder(agentInstanceContext, resultOfStart);
        }
    }
} // end of namespace