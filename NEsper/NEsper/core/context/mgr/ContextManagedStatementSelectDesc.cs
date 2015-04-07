///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.core.context.factory;
using com.espertech.esper.core.context.subselect;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextManagedStatementSelectDesc : ContextControllerStatementBase
    {
        public ContextManagedStatementSelectDesc(StatementSpecCompiled statementSpec, StatementContext statementContext, ContextMergeView mergeView, StatementAgentInstanceFactory factory, IList<AggregationServiceAggExpressionDesc> aggregationExpressions, SubSelectStrategyCollection subSelectPrototypeCollection)
                    : base(statementSpec, statementContext, mergeView, factory)
        {
            AggregationExpressions = aggregationExpressions;
            SubSelectPrototypeCollection = subSelectPrototypeCollection;
        }

        public IList<AggregationServiceAggExpressionDesc> AggregationExpressions { get; private set; }

        public SubSelectStrategyCollection SubSelectPrototypeCollection { get; private set; }
    }
}
