///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    /// <summary>
    ///     MapIndex for filter parameter constants for the range operators (range open/closed/half).
    ///     The implementation is based on the SortedMap implementation of TreeMap and stores only
    ///     expression parameter values of type DoubleRange.
    /// </summary>
    public abstract class FilterParamIndexDoubleRangeBase : FilterParamIndexLookupableBase
    {
        private readonly IDictionary<DoubleRange, EventEvaluator> _rangesNullEndpoints;

        protected readonly OrderedDictionary<DoubleRange, EventEvaluator> Ranges;
        protected double LargestRangeValueDouble = double.MinValue;

        protected FilterParamIndexDoubleRangeBase(
            ExprFilterSpecLookupable lookupable,
            IReaderWriterLock readWriteLock,
            FilterOperator filterOperator)
            : base(filterOperator, lookupable)
        {
            Ranges = new OrderedDictionary<DoubleRange, EventEvaluator>(new DoubleRangeComparator());
            _rangesNullEndpoints = new Dictionary<DoubleRange, EventEvaluator>();
            ReadWriteLock = readWriteLock;
        }

        public override int CountExpensive => Ranges.Count;

        public override bool IsEmpty => Ranges.IsEmpty();

        public override IReaderWriterLock ReadWriteLock { get; }

        public override EventEvaluator Get(object expressionValue)
        {
            if (!(expressionValue is DoubleRange)) {
                throw new ArgumentException("Supplied expressionValue must be of type DoubleRange");
            }

            var range = (DoubleRange) expressionValue;
            if (range.Max == null || range.Min == null) {
                return _rangesNullEndpoints.Get(range);
            }

            return Ranges.Get(range);
        }

        public override void Put(
            object expressionValue,
            EventEvaluator matcher)
        {
            if (!(expressionValue is DoubleRange)) {
                throw new ArgumentException("Supplied expressionValue must be of type DoubleRange");
            }

            var range = (DoubleRange) expressionValue;
            if (range.Max == null || range.Min == null) {
                _rangesNullEndpoints.Put(range, matcher); // endpoints null - we don't enter
                return;
            }

            if (Math.Abs(range.Max.Value - range.Min.Value) > LargestRangeValueDouble) {
                LargestRangeValueDouble = Math.Abs(range.Max.Value - range.Min.Value);
            }

            Ranges.Put(range, matcher);
        }

        public override void Remove(object filterConstant)
        {
            var range = (DoubleRange) filterConstant;

            if (range.Max == null || range.Min == null) {
                _rangesNullEndpoints.Delete(range);
            }
            else {
                Ranges.Delete(range);
            }
        }

        public override void GetTraverseStatement(
            EventTypeIndexTraverse traverse,
            ICollection<int> statementIds,
            ArrayDeque<FilterItem> evaluatorStack)
        {
            foreach (var entry in Ranges) {
                evaluatorStack.Add(new FilterItem(Lookupable.Expression, FilterOperator, entry.Key));
                entry.Value.GetTraverseStatement(traverse, statementIds, evaluatorStack);
                evaluatorStack.RemoveLast();
            }
        }
    }
}