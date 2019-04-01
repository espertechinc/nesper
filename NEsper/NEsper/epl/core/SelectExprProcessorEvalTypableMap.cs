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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.core
{
    public class SelectExprProcessorEvalTypableMap : ExprEvaluator
    {
        private readonly EventAdapterService _eventAdapterService;
        private readonly ExprEvaluator _innerEvaluator;
        private readonly EventType _mapType;

        public SelectExprProcessorEvalTypableMap(
            EventType mapType,
            ExprEvaluator innerEvaluator,
            EventAdapterService eventAdapterService)
        {
            _mapType = mapType;
            _innerEvaluator = innerEvaluator;
            _eventAdapterService = eventAdapterService;
        }

        public ExprEvaluator InnerEvaluator
        {
            get { return _innerEvaluator; }
        }

        public Object Evaluate(EvaluateParams evaluateParams)
        {
            var values = (IDictionary<string, Object>) _innerEvaluator.Evaluate(evaluateParams);
            if (values == null)
            {
                return _eventAdapterService.AdapterForTypedMap(Collections.EmptyDataMap, _mapType);
            }
            return _eventAdapterService.AdapterForTypedMap(values, _mapType);
        }

        public Type ReturnType
        {
            get { return typeof (IDictionary<string, object>); }
        }
    }
} // end of namespace