///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly FilterFactory _factory;
        private readonly AgentInstanceContext _agentInstanceContext;
        private EventBeanSPI _theEvent;
        private readonly EventBean[] _eventsPerStream = new EventBean[1];

#pragma warning disable 649
        [DataFlowContext] private EPDataFlowEmitter graphContext;
#pragma warning restore 649

        public FilterOp(FilterFactory factory, AgentInstanceContext agentInstanceContext)
        {
            this._factory = factory;
            this._agentInstanceContext = agentInstanceContext;

            _theEvent = EventTypeUtility.GetShellForType(factory.EventType);
            _eventsPerStream[0] = _theEvent;
        }

        public void OnInput(object row)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("Received row for filtering: " + row.RenderAny());
            }

            if (!(row is EventBeanSPI))
            {
                _theEvent.Underlying = row;
            }
            else
            {
                _theEvent = (EventBeanSPI) row;
            }

            object pass = _factory.Filter.Evaluate(_eventsPerStream, true, _agentInstanceContext);
            if (pass != null && true.Equals(pass))
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Submitting row " + row.RenderAny());
                }

                if (_factory.IsSingleOutputPort)
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
                if (!_factory.IsSingleOutputPort)
                {
                    graphContext.SubmitPort(1, row);
                }
            }
        }
    }
} // end of namespace