///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.factory;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.core.context.mgr
{
    public abstract class ContextControllerStatementBase
    {
        protected ContextControllerStatementBase(StatementSpecCompiled statementSpec,
                                                 StatementContext statementContext,
                                                 ContextMergeView mergeView,
                                                 StatementAgentInstanceFactory factory)
        {
            StatementSpec = statementSpec;
            StatementContext = statementContext;
            MergeView = mergeView;
            Factory = factory;
        }

        public StatementSpecCompiled StatementSpec { get; private set; }

        public StatementContext StatementContext { get; private set; }

        public ContextMergeView MergeView { get; private set; }

        public StatementAgentInstanceFactory Factory { get; private set; }
    }
}