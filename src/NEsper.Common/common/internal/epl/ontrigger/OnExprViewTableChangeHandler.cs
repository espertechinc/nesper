///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    public class OnExprViewTableChangeHandler
    {
        private readonly Table table;
        private OneEventCollection coll;

        public OnExprViewTableChangeHandler(Table table)
        {
            this.table = table;
        }

        public EventBean[] Events => coll?.ToArray();

        public void Add(
            EventBean theEvent,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            if (coll == null) {
                coll = new OneEventCollection();
            }

            if (theEvent is NaturalEventBean bean) {
                theEvent = bean.OptionalSynthetic;
            }

            coll.Add(table.EventToPublic.Convert(theEvent, eventsPerStream, isNewData, context));
        }
    }
} // end of namespace