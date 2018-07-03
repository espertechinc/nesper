///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// A select expression processor that check what type of result (synthetic and 
    /// natural) event is expected and produces.
    /// </summary>
    public class SelectExprResultProcessor : SelectExprProcessor
    {
        private readonly StatementResultService _statementResultService;
        private readonly SelectExprProcessor _syntheticProcessor;
        private readonly BindProcessor _bindProcessor;

        /// <summary>Ctor. </summary>
        /// <param name="statementResultService">for awareness of listeners and subscribers handles output results</param>
        /// <param name="syntheticProcessor">is the processor generating synthetic events according to the select clause</param>
        /// <param name="bindProcessor">for generating natural object column results</param>
        public SelectExprResultProcessor(StatementResultService statementResultService,
                                         SelectExprProcessor syntheticProcessor,
                                         BindProcessor bindProcessor)
        {
            _statementResultService = statementResultService;
            _syntheticProcessor = syntheticProcessor;
            _bindProcessor = bindProcessor;
        }

        public EventType ResultEventType
        {
            get { return _syntheticProcessor.ResultEventType; }
        }

        public EventBean Process(EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QSelectClause(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext); }
            if ((isSynthesize) && (!_statementResultService.IsMakeNatural))
            {
                if (InstrumentationHelper.ENABLED)
                {
                    EventBean result = _syntheticProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                    InstrumentationHelper.Get().ASelectClause(isNewData, result, null);
                    return result;
                }
                return _syntheticProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
            }

            EventBean syntheticEvent = null;
            EventType syntheticEventType = null;
            if (_statementResultService.IsMakeSynthetic || isSynthesize)
            {
                syntheticEvent = _syntheticProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);

                if (!_statementResultService.IsMakeNatural)
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ASelectClause(isNewData, syntheticEvent, null); }
                    return syntheticEvent;
                }

                syntheticEventType = _syntheticProcessor.ResultEventType;
            }

            if (!_statementResultService.IsMakeNatural)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ASelectClause(isNewData, null, null); }
                return null; // neither synthetic nor natural required, be cheap and generate no output event
            }

            Object[] parameters = _bindProcessor.Process(eventsPerStream, isNewData, exprEvaluatorContext);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ASelectClause(isNewData, null, parameters); }
            return new NaturalEventBean(syntheticEventType, parameters, syntheticEvent);
        }
    }
}
