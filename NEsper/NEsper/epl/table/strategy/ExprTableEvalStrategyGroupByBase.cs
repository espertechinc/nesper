///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.table.strategy
{
    public abstract class ExprTableEvalStrategyGroupByBase
    {
        private readonly TableAndLockProviderGrouped _provider;

        protected ExprTableEvalStrategyGroupByBase(TableAndLockProviderGrouped provider)
        {
            _provider = provider;
        }

        protected ObjectArrayBackedEventBean LockTableReadAndGet(object group, ExprEvaluatorContext context)
        {
            TableAndLockGrouped tableAndLockGrouped = _provider.Get();
            ExprTableEvalLockUtil.ObtainLockUnless(tableAndLockGrouped.Lock, context);
            return tableAndLockGrouped.Grouped.GetRowForGroupKey(group);
        }

        protected TableStateInstanceGrouped LockTableRead(ExprEvaluatorContext context)
        {
            TableAndLockGrouped tableAndLockGrouped = _provider.Get();
            ExprTableEvalLockUtil.ObtainLockUnless(tableAndLockGrouped.Lock, context);
            return tableAndLockGrouped.Grouped;
        }
    }
} // end of namespace
