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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    public class SupportFilterParamIndex : FilterParamIndexLookupableBase
    {
        public SupportFilterParamIndex(ExprFilterSpecLookupable lookupable) : base(FilterOperator.EQUAL, lookupable)
        {
        }

        public override EventEvaluator Get(object expressionValue)
        {
            return null;
        }

        public override void Put(
            object expressionValue,
            EventEvaluator evaluator)
        {
        }

        public override void Remove(object expressionValue)
        {
        }

        public override int CountExpensive => 0;

        public override bool IsEmpty => true;

        public override IReaderWriterLock ReadWriteLock => null;

        public override void MatchEvent(
            EventBean theEvent,
            ICollection<FilterHandle> matches,
            ExprEvaluatorContext ctx)
        {
        }

        public override void GetTraverseStatement(
            EventTypeIndexTraverse traverse,
            ICollection<int> statementIds,
            ArrayDeque<FilterItem> evaluatorStack)
        {
            throw new UnsupportedOperationException();
        }
    }
} // end of namespace
