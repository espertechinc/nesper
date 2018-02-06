///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// MapIndex that simply maintains a list of bool expressions.
    /// </summary>
    public sealed class FilterParamIndexBooleanExpr : FilterParamIndexBase
    {
        private readonly IDictionary<ExprNodeAdapterBase, EventEvaluator> _evaluatorsMap;
        private readonly IReaderWriterLock _constantsMapRwLock;

        /// <summary>
        /// Constructs the index for multiple-exact matches.
        /// </summary>
        public FilterParamIndexBooleanExpr(IReaderWriterLock readWriteLock)
            : base(FilterOperator.BOOLEAN_EXPRESSION)
        {
            _evaluatorsMap = new LinkedHashMap<ExprNodeAdapterBase, EventEvaluator>();
            _constantsMapRwLock = readWriteLock;
        }

        public override EventEvaluator this[object filterConstant]
        {
            get { return Get(filterConstant); }
            set { Put(filterConstant, value); }
        }

        public EventEvaluator Get(Object filterConstant)
        {
            var keyValues = (ExprNodeAdapterBase)filterConstant;
            return _evaluatorsMap.Get(keyValues);
        }

        public void Put(Object filterConstant, EventEvaluator evaluator)
        {
            var keys = (ExprNodeAdapterBase)filterConstant;
            _evaluatorsMap.Put(keys, evaluator);
        }

        public override void Remove(Object filterConstant)
        {
            var keys = (ExprNodeAdapterBase)filterConstant;
            _evaluatorsMap.Delete(keys);
        }

        public override int Count
        {
            get { return _evaluatorsMap.Count; }
        }

        public override bool IsEmpty
        {
            get { return _evaluatorsMap.IsEmpty(); }
        }

        public override IReaderWriterLock ReadWriteLock
        {
            get { return _constantsMapRwLock; }
        }

        public override void MatchEvent(EventBean theEvent, ICollection<FilterHandle> matches)
        {
            using (Instrument.With(
                i => i.QFilterBoolean(this),
                i => i.AFilterBoolean()))
            {
                using (_constantsMapRwLock.AcquireReadLock())
                {
                    if (InstrumentationHelper.ENABLED)
                    {
                        var i = -1;
                        foreach (var evals in _evaluatorsMap)
                        {
                            i++;
                            InstrumentationHelper.Get().QFilterBooleanExpr(i, evals);
                            var result = evals.Key.Evaluate(theEvent);
                            InstrumentationHelper.Get().AFilterBooleanExpr(result);
                            if (result)
                            {
                                evals.Value.MatchEvent(theEvent, matches);
                            }
                        }
                    }
                    else
                    {
                        foreach (KeyValuePair<ExprNodeAdapterBase, EventEvaluator> evals in _evaluatorsMap)
                        {
                            if (evals.Key.Evaluate(theEvent))
                            {
                                evals.Value.MatchEvent(theEvent, matches);
                            }
                        }
                    }
                }
            }
        }
    }
}
