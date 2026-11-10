///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    /// <summary>
    /// Zero-allocation typed equals index using a Dictionary keyed by a value-type primitive.
    /// Eliminates virtual object.Equals/GetHashCode dispatch on the hot filter lookup path.
    /// </summary>
    public abstract class FilterParamIndexEqualsTyped<T> : FilterParamIndexLookupableBase
        where T : struct
    {
        protected readonly Dictionary<T, EventEvaluator> TypedMap;
        protected readonly IReaderWriterLock ConstantsMapRwLock;

        protected FilterParamIndexEqualsTyped(
            ExprFilterSpecLookupable lookupable,
            IReaderWriterLock readWriteLock)
            : base(FilterOperator.EQUAL, lookupable)
        {
            TypedMap = new Dictionary<T, EventEvaluator>();
            ConstantsMapRwLock = readWriteLock;
        }

        /// <summary>
        /// Evaluator for a null filter constant, such as a correlated pattern value taken from an
        /// earlier event whose property is null.  A value-type key cannot hold null, so it is kept
        /// apart from <see cref="TypedMap"/>.  It never matches an event (null requires "is"), but
        /// the filter must still be registered and removable.
        /// </summary>
        private EventEvaluator _nullEvaluator;

        protected abstract T Unbox(object value);

        public override EventEvaluator Get(object filterConstant)
        {
            if (filterConstant == null) {
                return _nullEvaluator;
            }

            TypedMap.TryGetValue(Unbox(filterConstant), out var ev);
            return ev;
        }

        public override void Put(object filterConstant, EventEvaluator evaluator)
        {
            if (filterConstant == null) {
                _nullEvaluator = evaluator;
                return;
            }

            TypedMap[Unbox(filterConstant)] = evaluator;
        }

        public override void Remove(object filterConstant)
        {
            if (filterConstant == null) {
                _nullEvaluator = null;
                return;
            }

            TypedMap.Remove(Unbox(filterConstant));
        }

        public override int CountExpensive => TypedMap.Count + (_nullEvaluator == null ? 0 : 1);

        public override bool IsEmpty => TypedMap.Count == 0 && _nullEvaluator == null;

        public override IReaderWriterLock ReadWriteLock => ConstantsMapRwLock;

        public override void GetTraverseStatement(
            EventTypeIndexTraverse traverse,
            ICollection<int> statementIds,
            ArrayDeque<FilterItem> evaluatorStack)
        {
            if (_nullEvaluator != null) {
                evaluatorStack.Add(new FilterItem(Lookupable.Expression, FilterOperator, null, this));
                _nullEvaluator.GetTraverseStatement(traverse, statementIds, evaluatorStack);
                evaluatorStack.RemoveLast();
            }

            foreach (var entry in TypedMap) {
                evaluatorStack.Add(new FilterItem(Lookupable.Expression, FilterOperator, entry.Key, this));
                entry.Value.GetTraverseStatement(traverse, statementIds, evaluatorStack);
                evaluatorStack.RemoveLast();
            }
        }
    }
}
