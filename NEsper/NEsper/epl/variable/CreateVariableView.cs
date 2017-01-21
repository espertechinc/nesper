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
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.events;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.variable
{
    /// <summary>View for handling create-variable syntax. 
    /// <para/> The view posts to listeners when a variable changes, if it has subviews. 
    /// <para/> The view returns the current variable value for the iterator. 
    /// <para/> The event type for such posted events is a single field Map with the variable value. 
    /// </summary>
    public class CreateVariableView
        : ViewSupport
    {
        private readonly EventAdapterService _eventAdapterService;
        private readonly VariableReader _reader;
        private readonly EventType _eventType;
        private readonly String _variableName;
        private readonly StatementResultService _statementResultService;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="statementId">The statement id.</param>
        /// <param name="eventAdapterService">for creating events</param>
        /// <param name="variableService">for looking up variables</param>
        /// <param name="variableName">is the name of the variable to create</param>
        /// <param name="statementResultService">for coordinating on whether insert and remove stream events should be posted</param>
        /// <param name="agentInstanceId"></param>
        public CreateVariableView(int statementId, EventAdapterService eventAdapterService, VariableService variableService, string variableName, StatementResultService statementResultService, int agentInstanceId)
        {
            _eventAdapterService = eventAdapterService;
            _variableName = variableName;
            _statementResultService = statementResultService;
            _reader = variableService.GetReader(variableName, agentInstanceId);
            _eventType = GetEventType(statementId, eventAdapterService, _reader.VariableMetaData);
        }

        public static EventType GetEventType(int statementId, EventAdapterService eventAdapterService, VariableMetaData variableMetaData)
        {
            var variableTypes = new Dictionary<String, Object>();
            variableTypes.Put(variableMetaData.VariableName, variableMetaData.VariableType);
            String outputEventTypeName = statementId + "_outcreatevar";
            return eventAdapterService.CreateAnonymousMapType(outputEventTypeName, variableTypes, true);
        }
    
        public void Update(Object newValue, Object oldValue)
        {
            if (_statementResultService.IsMakeNatural || _statementResultService.IsMakeSynthetic)
            {
                IDictionary<String, Object> valuesOld = new Dictionary<String, Object>();
                valuesOld.Put(_variableName, oldValue);
                EventBean eventOld = _eventAdapterService.AdapterForTypedMap(valuesOld, _eventType);
    
                IDictionary<String, Object> valuesNew = new Dictionary<String, Object>();
                valuesNew.Put(_variableName, newValue);
                EventBean eventNew = _eventAdapterService.AdapterForTypedMap(valuesNew, _eventType);
    
                UpdateChildren(new[] {eventNew}, new[] {eventOld});
            }
        }
    
        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            throw new UnsupportedOperationException("Update not supported");
        }

        public override EventType EventType
        {
            get { return _eventType; }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            Object value = _reader.Value;
            IDictionary<String, Object> values = new Dictionary<String, Object>();
            values.Put(_variableName, value);
            EventBean theEvent = _eventAdapterService.AdapterForTypedMap(values, _eventType);
            yield return theEvent;
        }
    }
}
