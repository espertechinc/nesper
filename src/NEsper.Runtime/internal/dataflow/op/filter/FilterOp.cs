///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.runtime.@internal.dataflow.op.filter
{
    public class FilterOp : DataFlowOperator
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static int _gid = 0;
        private int _id = ++_gid;
        
        private readonly FilterFactory factory;
        private readonly AgentInstanceContext agentInstanceContext;
        private EventBeanSPI theEvent;
        private readonly EventBean[] eventsPerStream = new EventBean[1];

#pragma warning disable 649
        [DataFlowContext] private EPDataFlowEmitter graphContext;
#pragma warning restore 649

        public FilterOp(FilterFactory factory, AgentInstanceContext agentInstanceContext)
        {
            this.factory = factory;
            this.agentInstanceContext = agentInstanceContext;

            theEvent = EventTypeUtility.GetShellForType(factory.EventType);
            eventsPerStream[0] = theEvent;
        }

        public void OnInput(object row)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug("Received row for filtering: " + row.RenderAny());
            }

            if (!(row is EventBeanSPI))
            {
                theEvent.Underlying = row;
            }
            else
            {
                theEvent = (EventBeanSPI) row;
            }

            object pass = factory.Filter.Evaluate(eventsPerStream, true, agentInstanceContext);
            if (pass != null && true.Equals(pass))
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug("Submitting row " + row.RenderAny());
                }

                if (factory.IsSingleOutputPort)
                {
                    graphContext.Submit(row);
                }
                else
                {
                    graphContext.SubmitPort(0, row);
                }
            }
            else
            {
                if (!factory.IsSingleOutputPort)
                {
                    graphContext.SubmitPort(1, row);
                }
            }
        }
    }
} // end of namespace