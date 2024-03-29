///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.join.queryplan;

namespace com.espertech.esper.common.@internal.epl.join.support
{
    public interface QueryPlanIndexHook
    {
        void Subquery(QueryPlanIndexDescSubquery subquery);

        void InfraOnExpr(QueryPlanIndexDescOnExpr onexpr);

        void FireAndForget(QueryPlanIndexDescFAF queryPlanIndexDescFAF);

        void Join(QueryPlanForge join);

        void Historical(QueryPlanIndexDescHistorical historical);
    }
} // end of namespace