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
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.core.eval
{
    public class EvalSelectStreamWUnderlying
        : EvalSelectStreamBaseMap
        , SelectExprProcessor
    {
        private readonly bool _singleStreamWrapper;
        private readonly ExprEvaluator _underlyingExprEvaluator;
        private readonly bool _underlyingIsFragmentEvent;
        private readonly EventPropertyGetter _underlyingPropertyEventGetter;
        private readonly int _underlyingStreamNumber;
        private readonly IList<SelectExprStreamDesc> _unnamedStreams;
        private readonly TableMetadata _tableMetadata;

        public EvalSelectStreamWUnderlying(SelectExprContext selectExprContext,
                                           EventType resultEventType,
                                           IList<SelectClauseStreamCompiledSpec> namedStreams,
                                           bool usingWildcard,
                                           IList<SelectExprStreamDesc> unnamedStreams,
                                           bool singleStreamWrapper,
                                           bool underlyingIsFragmentEvent,
                                           int underlyingStreamNumber,
                                           EventPropertyGetter underlyingPropertyEventGetter,
                                           ExprEvaluator underlyingExprEvaluator,
                                           TableMetadata tableMetadata)
            : base(selectExprContext, resultEventType, namedStreams, usingWildcard)
        {
            _unnamedStreams = unnamedStreams;
            _singleStreamWrapper = singleStreamWrapper;
            _underlyingIsFragmentEvent = underlyingIsFragmentEvent;
            _underlyingStreamNumber = underlyingStreamNumber;
            _underlyingPropertyEventGetter = underlyingPropertyEventGetter;
            _underlyingExprEvaluator = underlyingExprEvaluator;
            _tableMetadata = tableMetadata;
        }

        public override EventBean ProcessSpecific(IDictionary<String, Object> props,
                                                  EventBean[] eventsPerStream,
                                                  bool isNewData,
                                                  ExprEvaluatorContext exprEvaluatorContext)
        {
            // In case of a wildcard and single stream that is itself a
            // wrapper bean, we also need to add the map properties
            if (_singleStreamWrapper)
            {
                var wrapper = (DecoratingEventBean)eventsPerStream[0];
                if (wrapper != null)
                {
                    IDictionary<String, Object> map = wrapper.DecoratingProperties;
                    props.PutAll(map);
                }
            }

            EventBean theEvent = null;
            if (_underlyingIsFragmentEvent)
            {
                EventBean eventBean = eventsPerStream[_underlyingStreamNumber];
                theEvent = (EventBean)eventBean.GetFragment(_unnamedStreams[0].StreamSelected.StreamName);
            }
            else if (_underlyingPropertyEventGetter != null)
            {
                object value = _underlyingPropertyEventGetter.Get(eventsPerStream[_underlyingStreamNumber]);
                if (value != null)
                {
                    theEvent = SelectExprContext.EventAdapterService.AdapterForObject(value);
                }
            }
            else if (_underlyingExprEvaluator != null)
            {
                object value = _underlyingExprEvaluator.Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
                if (value != null)
                {
                    theEvent = SelectExprContext.EventAdapterService.AdapterForObject(value);
                }
            }
            else
            {
                theEvent = eventsPerStream[_underlyingStreamNumber];
                if (_tableMetadata != null && theEvent != null)
                {
                    theEvent = _tableMetadata.EventToPublic.Convert(theEvent, eventsPerStream, isNewData, exprEvaluatorContext);
                }
            }

            // Using a wrapper bean since we cannot use the same event type else same-type filters match.
            // Wrapping it even when not adding properties is very inexpensive.
            return base.SelectExprContext.EventAdapterService.AdapterForTypedWrapper(theEvent, props, base.ResultEventType);
        }
    }
}