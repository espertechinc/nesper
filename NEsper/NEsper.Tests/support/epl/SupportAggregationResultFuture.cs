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
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.support.epl
{
    public class SupportAggregationResultFuture : AggregationResultFuture
    {
        private readonly Object[] _values;

        public SupportAggregationResultFuture(Object[] values)
        {
            _values = values;
        }

        public object GetValue(int column, int agentInstanceId, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            return _values[column];
        }

        public ICollection<EventBean> GetCollectionOfEvents(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;
        }

        public EventBean GetEventBean(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;
        }

        public Object GetGroupKey(int agentInstanceId)
        {
            return null;
        }

        public ICollection<Object> GetGroupKeys(ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }

        public ICollection<Object> GetCollectionScalar(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;
        }
    }
}
