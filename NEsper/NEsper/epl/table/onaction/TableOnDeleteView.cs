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
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.table.onaction
{
    /// <summary>
    /// View for the on-delete statement that handles removing events from a named window.
    /// </summary>
    public class TableOnDeleteView : TableOnViewBase
    {
        private readonly TableOnDeleteViewFactory parent;
    
        public TableOnDeleteView(SubordWMatchExprLookupStrategy lookupStrategy, TableStateInstance rootView, ExprEvaluatorContext exprEvaluatorContext, TableMetadata metadata, TableOnDeleteViewFactory parent)
            : base(lookupStrategy, rootView, exprEvaluatorContext, metadata, true)
        {
            this.parent = parent;
        }
    
        public override void HandleMatching(EventBean[] triggerEvents, EventBean[] matchingEvents)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QInfraOnAction(OnTriggerType.ON_DELETE, triggerEvents, matchingEvents);}
    
            if ((matchingEvents != null) && (matchingEvents.Length > 0))
            {
                foreach (EventBean @event in matchingEvents) {
                    TableStateInstance.DeleteEvent(@event);
                }
    
                // The on-delete listeners receive the events deleted, but only if there is interest
                if (parent.StatementResultService.IsMakeNatural || parent.StatementResultService.IsMakeSynthetic) {
                    EventBean[] posted = TableOnViewUtil.ToPublic(matchingEvents, parent.TableMetadata, triggerEvents, true, base.ExprEvaluatorContext);
                    UpdateChildren(posted, null);
                }
            }
    
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AInfraOnAction();}
        }

        public override EventType EventType
        {
            get { return parent.TableMetadata.PublicEventType; }
        }
    }
}
