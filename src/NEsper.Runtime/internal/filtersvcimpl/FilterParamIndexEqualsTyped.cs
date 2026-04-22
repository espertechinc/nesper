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

        protected abstract T Unbox(object value);

        public override EventEvaluator Get(object filterConstant)
        {
            TypedMap.TryGetValue(Unbox(filterConstant), out var ev);
            return ev;
        }

        public override void Put(object filterConstant, EventEvaluator evaluator) =>
            TypedMap[Unbox(filterConstant)] = evaluator;

        public override void Remove(object filterConstant) =>
            TypedMap.Remove(Unbox(filterConstant));

        public override int CountExpensive => TypedMap.Count;

        public override bool IsEmpty => TypedMap.Count == 0;

        public override IReaderWriterLock ReadWriteLock => ConstantsMapRwLock;

        public override void GetTraverseStatement(
            EventTypeIndexTraverse traverse,
            ICollection<int> statementIds,
            ArrayDeque<FilterItem> evaluatorStack)
        {
            foreach (var entry in TypedMap) {
                evaluatorStack.Add(new FilterItem(Lookupable.Expression, FilterOperator, entry.Key, this));
                entry.Value.GetTraverseStatement(traverse, statementIds, evaluatorStack);
                evaluatorStack.RemoveLast();
            }
        }
    }
}
