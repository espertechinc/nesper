///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.statement.dispatch;
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esper.runtime.@internal.kernel.statement
{
    public class EPStatementFactoryArgs
    {
        public EPStatementFactoryArgs(
            StatementContext statementContext,
            UpdateDispatchView dispatchChildView,
            StatementResultServiceImpl statementResultService)
        {
            StatementContext = statementContext;
            DispatchChildView = dispatchChildView;
            StatementResultService = statementResultService;
        }

        public StatementContext StatementContext { get; }

        public UpdateDispatchView DispatchChildView { get; }

        public StatementResultServiceImpl StatementResultService { get; }
    }
} // end of namespace