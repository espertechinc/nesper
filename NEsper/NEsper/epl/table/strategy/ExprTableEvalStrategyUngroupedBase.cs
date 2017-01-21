///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.table.strategy
{
    public abstract class ExprTableEvalStrategyUngroupedBase
    {
        private readonly TableAndLockProviderUngrouped _provider;

        protected ExprTableEvalStrategyUngroupedBase(TableAndLockProviderUngrouped provider)
        {
            _provider = provider;
        }

        protected ObjectArrayBackedEventBean LockTableReadAndGet(ExprEvaluatorContext context)
        {
            var pair = _provider.Get();
            ExprTableEvalLockUtil.ObtainLockUnless(pair.Lock, context);
            return pair.Ungrouped.EventUngrouped;
        }
    }
} // end of namespace
