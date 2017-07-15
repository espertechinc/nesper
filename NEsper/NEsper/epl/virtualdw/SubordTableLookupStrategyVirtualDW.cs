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
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.@join.plan;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.virtualdw
{
    public class SubordTableLookupStrategyVirtualDW : SubordTableLookupStrategy
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly String _namedWindowName;
        private readonly VirtualDataWindowLookup _externalIndex;
        private readonly ExternalEvaluator[] _evaluators;
        private readonly bool _nwOnTrigger;
        private readonly EventBean[] _eventsLocal;

        public SubordTableLookupStrategyVirtualDW(
            String namedWindowName,
            VirtualDataWindowLookup externalIndex,
            IList<SubordPropHashKey> hashKeys,
            CoercionDesc hashKeyCoercionTypes,
            IList<SubordPropRangeKey> rangeKeys,
            CoercionDesc rangeKeyCoercionTypes,
            bool nwOnTrigger,
            int numOuterStreams)
        {
            _namedWindowName = namedWindowName;
            _externalIndex = externalIndex;
            _evaluators = new ExternalEvaluator[hashKeys.Count + rangeKeys.Count];
            _nwOnTrigger = nwOnTrigger;
            _eventsLocal = new EventBean[numOuterStreams + 1];

            var count = 0;
            foreach (var hashKey in hashKeys)
            {
                var evaluator = hashKey.HashKey.KeyExpr.ExprEvaluator;
                _evaluators[count] = new ExternalEvaluatorHashRelOp(evaluator, hashKeyCoercionTypes.CoercionTypes[count]);
                count++;
            }
            for (var i = 0; i < rangeKeys.Count; i++)
            {
                SubordPropRangeKey rangeKey = rangeKeys[i];
                if (rangeKey.RangeInfo.RangeType.IsRange())
                {
                    var range = (QueryGraphValueEntryRangeIn)rangeKey.RangeInfo;
                    var evaluatorStart = range.ExprStart.ExprEvaluator;
                    var evaluatorEnd = range.ExprEnd.ExprEvaluator;
                    _evaluators[count] = new ExternalEvaluatorBtreeRange(evaluatorStart, evaluatorEnd, rangeKeyCoercionTypes.CoercionTypes[i]);
                }
                else
                {
                    var relOp = (QueryGraphValueEntryRangeRelOp)rangeKey.RangeInfo;
                    var evaluator = relOp.Expression.ExprEvaluator;
                    _evaluators[count] = new ExternalEvaluatorHashRelOp(evaluator, rangeKeyCoercionTypes.CoercionTypes[i]);
                }
                count++;
            }
        }

        public ICollection<EventBean> Lookup(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            EventBean[] events;
            if (_nwOnTrigger)
            {
                events = eventsPerStream;
            }
            else
            {
                Array.Copy(eventsPerStream, 0, _eventsLocal, 1, eventsPerStream.Length);
                events = _eventsLocal;
            }
            var keys = new Object[_evaluators.Length];
            for (var i = 0; i < _evaluators.Length; i++)
            {
                keys[i] = _evaluators[i].Evaluate(events, context);
            }

            ISet<EventBean> data = null;
            try
            {
                data = _externalIndex.Lookup(keys, eventsPerStream);
            }
            catch (Exception ex)
            {
                Log.Warn("Exception encountered invoking virtual data window external index for window '" + _namedWindowName + "': " + ex.Message, ex);
            }
            return data;
        }

        public String ToQueryPlan()
        {
            return GetType().FullName + " external index " + _externalIndex;
        }

        public LookupStrategyDesc StrategyDesc
        {
            get { return new LookupStrategyDesc(LookupStrategyType.VDW, null); }
        }

        internal interface ExternalEvaluator
        {
            Object Evaluate(EventBean[] events, ExprEvaluatorContext context);
        }

        internal class ExternalEvaluatorHashRelOp : ExternalEvaluator
        {
            private readonly ExprEvaluator _hashKeysEval;
            private readonly Type _coercionType;

            internal ExternalEvaluatorHashRelOp(ExprEvaluator hashKeysEval, Type coercionType)
            {
                _hashKeysEval = hashKeysEval;
                _coercionType = coercionType;
            }

            public Object Evaluate(EventBean[] events, ExprEvaluatorContext context)
            {
                return EventBeanUtility.Coerce(
                    _hashKeysEval.Evaluate(new EvaluateParams(events, true, context)),
                    _coercionType);
            }
        }

        internal class ExternalEvaluatorBtreeRange : ExternalEvaluator
        {
            private readonly ExprEvaluator _startEval;
            private readonly ExprEvaluator _endEval;
            private readonly Type _coercionType;

            internal ExternalEvaluatorBtreeRange(ExprEvaluator startEval, ExprEvaluator endEval, Type coercionType)
            {
                _startEval = startEval;
                _endEval = endEval;
                _coercionType = coercionType;
            }

            public Object Evaluate(EventBean[] events, ExprEvaluatorContext context)
            {
                var evaluateParams = new EvaluateParams(events, true, context);
                var start = EventBeanUtility.Coerce(_startEval.Evaluate(evaluateParams), _coercionType);
                var end = EventBeanUtility.Coerce(_endEval.Evaluate(evaluateParams), _coercionType);
                return new VirtualDataWindowKeyRange(start, end);
            }
        }
    }
}
