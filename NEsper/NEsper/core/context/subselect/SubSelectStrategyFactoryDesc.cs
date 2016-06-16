///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.prev;
using com.espertech.esper.epl.expression.prior;

namespace com.espertech.esper.core.context.subselect
{
    /// <summary>
    /// Record holding lookup resource references for use by <seealso cref="SubSelectActivationCollection" />.
    /// </summary>
    public class SubSelectStrategyFactoryDesc
    {
        public SubSelectStrategyFactoryDesc(
            SubSelectActivationHolder subSelectActivationHolder,
            SubSelectStrategyFactory factory,
            AggregationServiceFactoryDesc aggregationServiceFactoryDesc,
            IList<ExprPriorNode> priorNodesList,
            IList<ExprPreviousNode> prevNodesList,
            int subqueryNumber)
        {
            SubSelectActivationHolder = subSelectActivationHolder;
            Factory = factory;
            AggregationServiceFactoryDesc = aggregationServiceFactoryDesc;
            PriorNodesList = priorNodesList;
            PrevNodesList = prevNodesList;
            SubqueryNumber = subqueryNumber;
        }

        public SubSelectActivationHolder SubSelectActivationHolder { get; private set; }

        public SubSelectStrategyFactory Factory { get; private set; }

        public AggregationServiceFactoryDesc AggregationServiceFactoryDesc { get; private set; }

        public IList<ExprPriorNode> PriorNodesList { get; private set; }

        public IList<ExprPreviousNode> PrevNodesList { get; private set; }

        public int SubqueryNumber { get; private set; }
    }
}