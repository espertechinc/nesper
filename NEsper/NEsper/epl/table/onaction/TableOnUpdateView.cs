///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.table.upd;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.table.onaction
{
    public class TableOnUpdateView : TableOnViewBase
    {
        private readonly TableOnUpdateViewFactory parent;
    
        public TableOnUpdateView(SubordWMatchExprLookupStrategy lookupStrategy, TableStateInstance rootView, ExprEvaluatorContext exprEvaluatorContext, TableMetadata metadata, TableOnUpdateViewFactory parent)
            : base(lookupStrategy, rootView, exprEvaluatorContext, metadata, true)
        {
            this.parent = parent;
        }
    
        public override void HandleMatching(EventBean[] triggerEvents, EventBean[] matchingEvents)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QInfraOnAction(OnTriggerType.ON_UPDATE, triggerEvents, matchingEvents);}
    
            EventBean[] eventsPerStream = new EventBean[3];
    
            bool postUpdates = parent.StatementResultService.IsMakeNatural || parent.StatementResultService.IsMakeSynthetic;
            EventBean[] postedOld = null;
            if (postUpdates) {
                postedOld = TableOnViewUtil.ToPublic(matchingEvents, parent.TableMetadata, triggerEvents, false, base.ExprEvaluatorContext);
            }
    
            TableUpdateStrategy tableUpdateStrategy = parent.TableUpdateStrategy;
    
            foreach (EventBean triggerEvent in triggerEvents) {
                eventsPerStream[1] = triggerEvent;
                tableUpdateStrategy.UpdateTable(matchingEvents, TableStateInstance, eventsPerStream, exprEvaluatorContext);
            }
    
            // The on-delete listeners receive the events deleted, but only if there is interest
            if (postUpdates) {
                EventBean[] postedNew = TableOnViewUtil.ToPublic(matchingEvents, parent.TableMetadata, triggerEvents, true, base.ExprEvaluatorContext);
                UpdateChildren(postedNew, postedOld);
            }
    
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AInfraOnAction();}
        }
    }
}
