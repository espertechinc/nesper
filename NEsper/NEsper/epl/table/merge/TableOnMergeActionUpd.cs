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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.table.onaction;
using com.espertech.esper.epl.table.upd;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.epl.table.merge
{
    public class TableOnMergeActionUpd 
        : TableOnMergeAction 
        , TableUpdateStrategyReceiver
    {
        private TableUpdateStrategy _tableUpdateStrategy;
    
        public TableOnMergeActionUpd(ExprEvaluator optionalFilter, TableUpdateStrategy tableUpdateStrategy)
            : base(optionalFilter)
        {
            _tableUpdateStrategy = tableUpdateStrategy;
        }
    
        public void Update(TableUpdateStrategy updateStrategy)
        {
            _tableUpdateStrategy = updateStrategy;
        }
    
        public override void Apply(EventBean matchingEvent, EventBean[] eventsPerStream, TableStateInstance tableStateInstance, TableOnMergeViewChangeHandler changeHandlerAdded, TableOnMergeViewChangeHandler changeHandlerRemoved, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (changeHandlerRemoved != null) {
                changeHandlerRemoved.Add(matchingEvent, eventsPerStream, false, exprEvaluatorContext);
            }
            _tableUpdateStrategy.UpdateTable(Collections.SingletonList(matchingEvent), tableStateInstance, eventsPerStream, exprEvaluatorContext);
            if (changeHandlerAdded != null) {
                changeHandlerAdded.Add(matchingEvent, eventsPerStream, false, exprEvaluatorContext);
            }
        }

        public override string Name
        {
            get { return "update"; }
        }
    }
}
