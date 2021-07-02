///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.lookup;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.lookupplansubord;

namespace com.espertech.esper.common.@internal.epl.namedwindow.core
{
    public interface NamedWindow
    {
        string Name { get; }

        NamedWindowRootView RootView { get; }

        NamedWindowTailView TailView { get; }

        NamedWindowInstance NamedWindowInstanceNoContext { get; }

        EventTableIndexMetadata EventTableIndexMetadata { get; }

        StatementContext StatementContext { get; set; }

        NamedWindowInstance GetNamedWindowInstance(ExprEvaluatorContext exprEvaluatorContext);

        NamedWindowInstance GetNamedWindowInstance(int cpid);

        void RemoveAllInstanceIndexes(IndexMultiKey index);

        void RemoveIndexReferencesStmtMayRemoveIndex(
            IndexMultiKey imk,
            string referringDeploymentId,
            string referringStatementName);

        void ValidateAddIndex(
            string deploymentId,
            string statementName,
            string indexName,
            string indexModuleName,
            QueryPlanIndexItem explicitIndexDesc,
            IndexMultiKey indexMultiKey);
    }
} // end of namespace