///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.prev;
using com.espertech.esper.epl.expression.prior;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.expression.table;

namespace com.espertech.esper.core.context.stmt
{
    public interface AIRegistryExpr
    {
        AIRegistrySubselect GetSubselectService(ExprSubselectNode exprSubselectNode);
        AIRegistryAggregation GetSubselectAggregationService(ExprSubselectNode exprSubselectNode);
        AIRegistryPrior GetPriorServices(ExprPriorNode key);
        AIRegistryPrevious GetPreviousServices(ExprPreviousNode key);
        AIRegistryMatchRecognizePrevious GetMatchRecognizePrevious();
        AIRegistryTableAccess GetTableAccessServices(ExprTableAccessNode key);
    
        AIRegistrySubselect AllocateSubselect(ExprSubselectNode subselectNode);
        AIRegistryAggregation AllocateSubselectAggregation(ExprSubselectNode subselectNode);
        AIRegistryPrior AllocatePrior(ExprPriorNode key);
        AIRegistryPrevious AllocatePrevious(ExprPreviousNode previousNode);
        AIRegistryMatchRecognizePrevious AllocateMatchRecognizePrevious();
        AIRegistryTableAccess AllocateTableAccess(ExprTableAccessNode tableNode);

        int SubselectAgentInstanceCount { get; }
        int PreviousAgentInstanceCount { get; }
        int PriorAgentInstanceCount { get; }

        void DeassignService(int agentInstanceId);
    }
}
