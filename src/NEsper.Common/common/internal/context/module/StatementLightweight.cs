///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.context.module
{
    public class StatementLightweight
    {
        public StatementLightweight(
            StatementProvider statementProvider,
            StatementInformationalsRuntime statementInformationals,
            StatementResultService statementResultService,
            StatementContext statementContext)
        {
            StatementProvider = statementProvider;
            StatementInformationals = statementInformationals;
            StatementResultService = statementResultService;
            StatementContext = statementContext;
        }

        public StatementInformationalsRuntime StatementInformationals { get; }

        public StatementResultService StatementResultService { get; }

        public StatementProvider StatementProvider { get; }

        public StatementContext StatementContext { get; }
    }
} // end of namespace