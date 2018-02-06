///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.index.quadtree;
using com.espertech.esper.epl.@join.table;

namespace com.espertech.esper.epl.lookup
{
    public interface EventAdvancedIndexFactory
    {
        AdvancedIndexConfigContextPartition ConfigureContextPartition(
            EventType eventType, AdvancedIndexDesc indexDesc,
            ExprEvaluator[] parameters, ExprEvaluatorContext exprEvaluatorContext, EventTableOrganization organization,
            EventAdvancedIndexConfigStatement advancedIndexConfigStatement);

        EventTable Make(
            EventAdvancedIndexConfigStatement configStatement,
            AdvancedIndexConfigContextPartition configContextPartition, 
            EventTableOrganization organization);

        bool ProvidesIndexForOperation(string operationName, IDictionary<int, ExprNode> expressions);

        SubordTableLookupStrategyFactoryQuadTree GetSubordinateLookupStrategy(
            string operationName, IDictionary<int, ExprNode> expressions, 
            bool isNWOnTrigger, int numOuterstreams);
    }
} // end of namespace