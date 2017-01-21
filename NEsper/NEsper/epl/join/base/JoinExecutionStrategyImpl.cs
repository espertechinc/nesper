///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.join.@base
{
    /// <summary>
    /// Join execution strategy based on a 3-step getSelectListEvents of composing a join 
    /// set, filtering the join set and indicating.
    /// </summary>
    public class JoinExecutionStrategyImpl : JoinExecutionStrategy
    {
        private readonly JoinSetComposer _composer;
        private readonly JoinSetProcessor _filter;
        private readonly JoinSetProcessor _indicator;
        private readonly ExprEvaluatorContext _staticExprEvaluatorContext;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="composer">determines join tuple set</param>
        /// <param name="filter">for filtering among tuples</param>
        /// <param name="indicator">for presenting the info to a view</param>
        /// <param name="staticExprEvaluatorContext">expression evaluation context for static evaluation (not for runtime eval)</param>
        public JoinExecutionStrategyImpl(JoinSetComposer composer, JoinSetProcessor filter, JoinSetProcessor indicator, ExprEvaluatorContext staticExprEvaluatorContext)
        {
            _composer = composer;
            _filter = filter;
            _indicator = indicator;
            _staticExprEvaluatorContext = staticExprEvaluatorContext;
        }
    
        public void Join(EventBean[][] newDataPerStream, EventBean[][] oldDataPerStream)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QJoinExexStrategy();}
            var joinSet = _composer.Join(newDataPerStream, oldDataPerStream, _staticExprEvaluatorContext);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AJoinExecStrategy(joinSet);}

            _filter.Process(joinSet.First, joinSet.Second, _staticExprEvaluatorContext);
    
            if ( (!joinSet.First.IsEmpty()) || (!joinSet.Second.IsEmpty()) ) {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QJoinExecProcess(joinSet);}
                _indicator.Process(joinSet.First, joinSet.Second, _staticExprEvaluatorContext);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AJoinExecProcess();}
            }
        }
    
        public ISet<MultiKey<EventBean>> StaticJoin()
        {
            var joinSet = _composer.StaticJoin();
            _filter.Process(joinSet, null, _staticExprEvaluatorContext);
            return joinSet;
        }
    }
}
