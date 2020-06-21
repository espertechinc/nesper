///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    /// <summary>
    /// Index for filter parameter constants to match using the equals (=) operator. The implementation is based on a
    /// regular HashMap.
    /// </summary>
    public abstract class FilterParamIndexEqualsBase : FilterParamIndexLookupableBase
    {
        protected readonly IDictionary<object, EventEvaluator> ConstantsMap;
        protected readonly IReaderWriterLock ConstantsMapRwLock;

        protected FilterParamIndexEqualsBase(ExprFilterSpecLookupable lookupable, IReaderWriterLock readWriteLock, FilterOperator filterOperator)
            : base(filterOperator, lookupable)
        {
            ConstantsMap = new Dictionary<object, EventEvaluator>().WithNullKeySupport();
            ConstantsMapRwLock = readWriteLock;
        }

        public override EventEvaluator Get(object filterConstant)
        {
            return ConstantsMap.Get(filterConstant);
        }

        public override void Put(object filterConstant, EventEvaluator evaluator)
        {
            ConstantsMap.Put(filterConstant, evaluator);
        }

        public override void Remove(object filterConstant)
        {
            ConstantsMap.Remove(filterConstant);
        }

        public override int CountExpensive => ConstantsMap.Count;

        public override bool IsEmpty => ConstantsMap.IsEmpty();

        public override IReaderWriterLock ReadWriteLock => ConstantsMapRwLock;

        public override void GetTraverseStatement(
            EventTypeIndexTraverse traverse, 
            ICollection<int> statementIds, 
            ArrayDeque<FilterItem> evaluatorStack)
        {
            foreach (var entry in ConstantsMap)
            {
                evaluatorStack.Add(new FilterItem(Lookupable.Expression, FilterOperator, entry.Key, this));
                entry.Value.GetTraverseStatement(traverse, statementIds, evaluatorStack);
                evaluatorStack.RemoveLast();
            }
        }
    }
}