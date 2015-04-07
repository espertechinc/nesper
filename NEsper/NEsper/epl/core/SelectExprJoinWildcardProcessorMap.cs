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
using com.espertech.esper.epl.expression;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Processor for select-clause expressions that handles wildcards. Computes results based on matching events.
    /// </summary>
    public class SelectExprJoinWildcardProcessorMap : SelectExprProcessor
    {
        private readonly EventAdapterService _eventAdapterService;
        private readonly EventType _resultEventType;
        private readonly String[] _streamNames;

        public SelectExprJoinWildcardProcessorMap(String[] streamNames,
                                                  EventType resultEventType,
                                                  EventAdapterService eventAdapterService)
        {
            _streamNames = streamNames;
            _resultEventType = resultEventType;
            _eventAdapterService = eventAdapterService;
        }

        #region SelectExprProcessor Members

        public EventBean Process(EventBean[] eventsPerStream,
                                 bool isNewData,
                                 bool isSynthesize,
                                 ExprEvaluatorContext exprEvaluatorContext)
        {
            IDictionary<String, Object> tuple = new Dictionary<String, Object>();
            for (int i = 0; i < _streamNames.Length; i++)
            {
                if (_streamNames[i] == null)
                {
                    throw new IllegalStateException("Event name for stream " + i + " is null");
                }
                tuple.Put(_streamNames[i], eventsPerStream[i]);
            }

            return _eventAdapterService.AdapterForTypedMap(tuple, _resultEventType);
        }

        #endregion

        public EventType ResultEventType
        {
            get { return _resultEventType; }
        }
    }
}