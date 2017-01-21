///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.@join.exec.@base;
using com.espertech.esper.epl.@join.plan;
using com.espertech.esper.epl.@join.rep;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.virtualdw
{
    public class JoinExecTableLookupStrategyVirtualDW : JoinExecTableLookupStrategy
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly String _namedWindowName;
        private readonly VirtualDataWindowLookup _externalIndex;
        private readonly ExternalEvaluator[] _evaluators;
        private readonly EventBean[] _eventsPerStream;
        private readonly int _lookupStream;
    
        public JoinExecTableLookupStrategyVirtualDW(String namedWindowName, VirtualDataWindowLookup externalIndex, TableLookupKeyDesc keyDescriptor, int lookupStream)
        {
            _namedWindowName = namedWindowName;
            _externalIndex = externalIndex;
            _evaluators = new ExternalEvaluator[keyDescriptor.Hashes.Count + keyDescriptor.Ranges.Count];
            _eventsPerStream = new EventBean[lookupStream + 1];
            _lookupStream = lookupStream;
    
            var count = 0;
            foreach (var hashKey in keyDescriptor.Hashes)
            {
                var evaluator = hashKey.KeyExpr.ExprEvaluator;
                _evaluators[count] = new ExternalEvaluatorHashRelOp(evaluator);
                count++;
            }
            foreach (var rangeKey in keyDescriptor.Ranges)
            {
                if (rangeKey.RangeType.IsRange()) {
                    var range = (QueryGraphValueEntryRangeIn) rangeKey;
                    var evaluatorStart = range.ExprStart.ExprEvaluator;
                    var evaluatorEnd = range.ExprEnd.ExprEvaluator;
                    _evaluators[count] = new ExternalEvaluatorBtreeRange(evaluatorStart, evaluatorEnd);
                }
                else {
                    var relOp = (QueryGraphValueEntryRangeRelOp) rangeKey;
                    var evaluator = relOp.Expression.ExprEvaluator;
                    _evaluators[count] = new ExternalEvaluatorHashRelOp(evaluator);
                }
                count++;
            }
        }
    
        public ICollection<EventBean> Lookup(EventBean theEvent, Cursor cursor, ExprEvaluatorContext context)
        {
            var events = new Mutable<ISet<EventBean>>();

            using (Instrument.With(
                i => i.QIndexJoinLookup(this, null),
                i => i.AIndexJoinLookup(events.Value, null)))
            {
                _eventsPerStream[_lookupStream] = theEvent;

                var keys = new Object[_evaluators.Length];
                for (var i = 0; i < _evaluators.Length; i++)
                {
                    keys[i] = _evaluators[i].Evaluate(_eventsPerStream, context);
                }

                try
                {
                    events.Value = _externalIndex.Lookup(keys, _eventsPerStream);
                }
                catch (Exception ex)
                {
                    Log.Warn(
                        "Exception encountered invoking virtual data window external index for window '" +
                        _namedWindowName + "': " + ex.Message, ex);
                }

                return events.Value;
            }
        }
    
        public String ToQueryPlan()
        {
            return GetType().FullName + " external index " + _externalIndex;
        }

        public LookupStrategyDesc StrategyDesc
        {
            get { return new LookupStrategyDesc(LookupStrategyType.VDW, null); }
        }

        private interface ExternalEvaluator
        {
            Object Evaluate(EventBean[] events, ExprEvaluatorContext context);
        }

        internal class ExternalEvaluatorHashRelOp : ExternalEvaluator
        {
            private readonly ExprEvaluator _hashKeysEval;
    
            internal ExternalEvaluatorHashRelOp(ExprEvaluator hashKeysEval)
            {
                _hashKeysEval = hashKeysEval;
            }
    
            public Object Evaluate(EventBean[] events, ExprEvaluatorContext context)
            {
                return _hashKeysEval.Evaluate(new EvaluateParams(events, true, context));
            }
        }

        internal class ExternalEvaluatorBtreeRange : ExternalEvaluator
        {
            private readonly ExprEvaluator _startEval;
            private readonly ExprEvaluator _endEval;

            internal ExternalEvaluatorBtreeRange(ExprEvaluator startEval, ExprEvaluator endEval)
            {
                _startEval = startEval;
                _endEval = endEval;
            }
    
            public Object Evaluate(EventBean[] events, ExprEvaluatorContext context)
            {
                var evaluateParams = new EvaluateParams(events, true, context);
                var start = _startEval.Evaluate(evaluateParams);
                var end = _endEval.Evaluate(evaluateParams);
                return new VirtualDataWindowKeyRange(start, end);
            }
        }
    }
}
