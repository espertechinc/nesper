///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.expression.subquery
{
    public class SubselectEvalStrategyRowFilteredUnselectedTable : SubselectEvalStrategyRowFilteredUnselected
    {
        private readonly TableMetadata _tableMetadata;
    
        public SubselectEvalStrategyRowFilteredUnselectedTable(TableMetadata tableMetadata)
        {
            _tableMetadata = tableMetadata;
        }

        public override Object Evaluate(
            EventBean[] eventsPerStream,
            bool newData,
            ICollection<EventBean> matchingEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            ExprSubselectRowNode parent)
        {
            EventBean[] eventsZeroBased = EventBeanUtility.AllocatePerStreamShift(eventsPerStream);
            EventBean subSelectResult = ExprSubselectRowNodeUtility.EvaluateFilterExpectSingleMatch(
                eventsZeroBased, newData, matchingEvents, exprEvaluatorContext, parent);
            if (subSelectResult == null)
            {
                return null;
            }
            return _tableMetadata.EventToPublic.ConvertToUnd(
                subSelectResult, new EvaluateParams(eventsPerStream, newData, exprEvaluatorContext));
        }
    }
} // end of namespace
