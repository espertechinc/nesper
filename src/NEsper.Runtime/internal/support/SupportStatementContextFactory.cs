///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.container;

namespace com.espertech.esper.runtime.@internal.support
{
    public class SupportStatementContextFactory
    {
        public static StatementContext MakeContext(
            IContainer container,
            int statementId)
        {
            StatementInformationalsRuntime informationals = new StatementInformationalsRuntime();
            return new StatementContext(
                container,
                null,
                "deployment1",
                statementId,
                "s0",
                null,
                informationals,
                null,
                new StatementContextRuntimeServices(container),
                null,
                null,
                null,
                null,
                new ScheduleBucket(statementId),
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);
        }
    }
} // end of namespace