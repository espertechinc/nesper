///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.runtime.@internal.metrics.instrumentation;

using static com.espertech.esper.common.@internal.compile.stage2.FilterSpecCompilerIndexPlanner;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    /// <summary>
    ///     Index that simply maintains a list of boolean expressions.
    /// </summary>
    public class FilterParamIndexBooleanExpr : FilterParamIndexBase
    {
        private readonly IDictionary<ExprNodeAdapterBase, EventEvaluator> evaluatorsMap;

        public FilterParamIndexBooleanExpr(IReaderWriterLock readWriteLock)
            : base(FilterOperator.BOOLEAN_EXPRESSION)
        {
            evaluatorsMap = new LinkedHashMap<ExprNodeAdapterBase, EventEvaluator>();
            ReadWriteLock = readWriteLock;
        }

        public override bool IsEmpty => evaluatorsMap.IsEmpty();

        public override IReaderWriterLock ReadWriteLock { get; }

        public override EventEvaluator Get(object filterConstant)
        {
            var keyValues = (ExprNodeAdapterBase) filterConstant;
            return evaluatorsMap.Get(keyValues);
        }

        public override void Put(
            object filterConstant,
            EventEvaluator evaluator)
        {
            var keys = (ExprNodeAdapterBase) filterConstant;
            evaluatorsMap.Put(keys, evaluator);
        }

        public override void Remove(object filterConstant)
        {
            var keys = (ExprNodeAdapterBase) filterConstant;
            evaluatorsMap.Remove(keys);
        }

        public bool RemoveMayNotExist(object filterForValue)
        {
            return evaluatorsMap.Remove((ExprNodeAdapterBase) filterForValue);
        }

        public override int CountExpensive => evaluatorsMap.Count;

        public override void MatchEvent(
            EventBean theEvent,
            ICollection<FilterHandle> matches,
            ExprEvaluatorContext ctx)
        {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QFilterBoolean(this);
            }

            using (ReadWriteLock.ReadLock.Acquire())
            {
                if (InstrumentationHelper.ENABLED)
                {
                    var i = -1;
                    foreach (KeyValuePair<ExprNodeAdapterBase, EventEvaluator> evals in evaluatorsMap)
                    {
                        i++;
                        InstrumentationHelper.Get().QFilterBooleanExpr(i, evals);
                        var result = evals.Key.Evaluate(theEvent);
                        InstrumentationHelper.Get().AFilterBooleanExpr(result);
                        if (result)
                        {
                            evals.Value.MatchEvent(theEvent, matches, ctx);
                        }
                    }
                }
                else
                {
                    foreach (KeyValuePair<ExprNodeAdapterBase, EventEvaluator> evals in evaluatorsMap)
                    {
                        if (evals.Key.Evaluate(theEvent))
                        {
                            evals.Value.MatchEvent(theEvent, matches, ctx);
                        }
                    }
                }
            }

            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().AFilterBoolean();
            }
        }

        public override void GetTraverseStatement(
            EventTypeIndexTraverse traverse,
            ICollection<int> statementIds,
            ArrayDeque<FilterItem> evaluatorStack)
        {
            foreach (KeyValuePair<ExprNodeAdapterBase, EventEvaluator> entry in evaluatorsMap) {
                evaluatorStack.Add(new FilterItem(PROPERTY_NAME_BOOLEAN_EXPRESSION, FilterOperator, entry));
                entry.Value.GetTraverseStatement(traverse, statementIds, evaluatorStack);
                evaluatorStack.RemoveLast();
            }
        }
    }
} // end of namespace