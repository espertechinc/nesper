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
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.epl.join.exec.@base
{
    /// <summary>
    ///     Execution node for lookup in a table.
    /// </summary>
    public class TableLookupExecNodeTableLocking : TableLookupExecNode
    {
        private readonly ILockable _lock;

        public TableLookupExecNodeTableLocking(
            int indexedStream,
            JoinExecTableLookupStrategy lookupStrategy,
            ILockable @lock)
            : base(
                indexedStream,
                lookupStrategy)
        {
            _lock = @lock;
        }

        public override void Process(
            EventBean lookupEvent,
            EventBean[] prefillPath,
            ICollection<EventBean[]> result,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            // acquire table index lock
            exprEvaluatorContext.TableExprEvaluatorContext.AddAcquiredLock(_lock);

            // lookup events
            var joinedEvents = lookupStrategy.Lookup(lookupEvent, null, exprEvaluatorContext);
            if (joinedEvents == null) {
                return;
            }

            // process results
            ProcessResults(prefillPath, result, joinedEvents);
        }
    }
} // end of namespace