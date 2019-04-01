///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.core
{
    /// <summary>Interface for processors of select-clause items, implementors are computing results based on matching events. </summary>
    public class SelectExprProcessorWDeliveryCallback : SelectExprProcessor
    {
        private readonly BindProcessor _bindProcessor;
        private readonly EventType _eventType;
        private readonly SelectExprProcessorDeliveryCallback _selectExprProcessorCallback;

        public SelectExprProcessorWDeliveryCallback(EventType eventType,
                                                    BindProcessor bindProcessor,
                                                    SelectExprProcessorDeliveryCallback selectExprProcessorCallback)
        {
            _eventType = eventType;
            _bindProcessor = bindProcessor;
            _selectExprProcessorCallback = selectExprProcessorCallback;
        }

        #region SelectExprProcessor Members

        public EventType ResultEventType
        {
            get { return _eventType; }
        }

        public EventBean Process(EventBean[] eventsPerStream,
                                 bool isNewData,
                                 bool isSynthesize,
                                 ExprEvaluatorContext exprEvaluatorContext)
        {
            Object[] columns = _bindProcessor.Process(eventsPerStream, isNewData, exprEvaluatorContext);
            return _selectExprProcessorCallback.Selected(columns);
        }

        #endregion
    }
}