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
using com.espertech.esper.view;

namespace com.espertech.esper.epl.variable
{
    /// <summary>
    /// A view that handles the setting of variables upon receipt of a triggering event.
    /// <para/>
    /// Variables are updated atomically and thus a separate commit actually updates the new 
    /// variable values, or a rollback if an exception occured during validation.
    /// </summary>
    public class OnSetVariableView : ViewSupport
    {
        private readonly OnSetVariableViewFactory _factory;
        private readonly ExprEvaluatorContext _exprEvaluatorContext;
    
        private readonly EventBean[] _eventsPerStream = new EventBean[1];
    
        public OnSetVariableView(OnSetVariableViewFactory factory, ExprEvaluatorContext exprEvaluatorContext) {
            _factory = factory;
            _exprEvaluatorContext = exprEvaluatorContext;
        }
    
        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if ((newData == null) || (newData.Length == 0))
            {
                return;
            }
    
            IDictionary<String, Object> values = null;
            var produceOutputEvents = (_factory.StatementResultService.IsMakeNatural || _factory.StatementResultService.IsMakeSynthetic);
    
            if (produceOutputEvents)
            {
                values = new Dictionary<String, Object>();
            }
    
            _eventsPerStream[0] = newData[newData.Length - 1];
            _factory.VariableReadWritePackage.WriteVariables(_factory.VariableService, _eventsPerStream, values, _exprEvaluatorContext);
            
            if (values != null)
            {
                EventBean[] newDataOut = new EventBean[1];
                newDataOut[0] = _factory.EventAdapterService.AdapterForTypedMap(values, _factory.EventType);
                this.UpdateChildren(newDataOut, null);
            }
        }

        public override EventType EventType
        {
            get { return _factory.EventType; }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            IDictionary<String, Object> values = _factory.VariableReadWritePackage.Iterate(
                _exprEvaluatorContext.AgentInstanceId);
            EventBean theEvent = _factory.EventAdapterService.AdapterForTypedMap(values, _factory.EventType);
            yield return theEvent;
        }
    }
}
