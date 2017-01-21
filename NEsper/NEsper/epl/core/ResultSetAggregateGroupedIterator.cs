///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// GetEnumerator for group-by with aggregation.
    /// </summary>
    public class ResultSetAggregateGroupedIterator : IEnumerator<EventBean>
    {
        private readonly IEnumerator<EventBean> _sourceIterator;
        private readonly ResultSetProcessorAggregateGrouped _resultSetProcessor;
        private readonly AggregationService _aggregationService;
        private EventBean _currResult;
        private bool _iterate;
        private readonly EventBean[] _eventsPerStream;
        private readonly ExprEvaluatorContext _exprEvaluatorContext;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="sourceIterator">is the parent iterator</param>
        /// <param name="resultSetProcessor">for constructing result rows</param>
        /// <param name="aggregationService">for pointing to the right aggregation row</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        public ResultSetAggregateGroupedIterator(
            IEnumerator<EventBean> sourceIterator,
            ResultSetProcessorAggregateGrouped resultSetProcessor,
            AggregationService aggregationService,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            _sourceIterator = sourceIterator;
            _resultSetProcessor = resultSetProcessor;
            _aggregationService = aggregationService;
            _eventsPerStream = new EventBean[1];
            _exprEvaluatorContext = exprEvaluatorContext;
            _currResult = null;
            _iterate = true;
        }

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        object IEnumerator.Current
        {
            get { return Current; }
        }

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        public EventBean Current
        {
            get { return _currResult; }
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>
        /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
        /// </returns>
        public bool MoveNext()
        {
            while (_iterate && _sourceIterator.MoveNext())
            {
                var candidate = _sourceIterator.Current;
                _eventsPerStream[0] = candidate;
    
                var groupKey = _resultSetProcessor.GenerateGroupKey(_eventsPerStream, true);
                _aggregationService.SetCurrentAccess(groupKey, _exprEvaluatorContext.AgentInstanceId, null);
    
                bool? pass = true;
                if (_resultSetProcessor.OptionalHavingNode != null)
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(_eventsPerStream);}
                    pass = _resultSetProcessor.OptionalHavingNode.Evaluate(new EvaluateParams(_eventsPerStream, true, _exprEvaluatorContext)).AsBoxedBoolean();
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(pass);}
                }
               
                if (false.Equals(pass))
                {
                    continue;
                }
    
                _currResult = _resultSetProcessor.SelectExprProcessor.Process(_eventsPerStream, true, true, _exprEvaluatorContext);
                return true;
            }

            return _iterate = false;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        /// <exception cref="System.NotSupportedException"></exception>
        public void Reset()
        {
            throw new NotSupportedException();
        }
    }
}
