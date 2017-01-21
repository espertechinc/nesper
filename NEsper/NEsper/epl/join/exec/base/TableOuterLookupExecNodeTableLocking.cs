///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.join.exec.@base
{
    public class TableOuterLookupExecNodeTableLocking : TableOuterLookupExecNode
    {
        private readonly ILockable _lock;
    
        public TableOuterLookupExecNodeTableLocking(int indexedStream, JoinExecTableLookupStrategy lookupStrategy, ILockable @lock)
            : base(indexedStream, lookupStrategy)
        {
            _lock = @lock;
        }
    
        public override void Process(EventBean lookupEvent, EventBean[] prefillPath, ICollection<EventBean[]> result, ExprEvaluatorContext exprEvaluatorContext)
        {
            // acquire table index lock
            exprEvaluatorContext.TableExprEvaluatorContext.AddAcquiredLock(_lock);
    
            // lookup events
            var joinedEvents = LookupStrategy.Lookup(lookupEvent, null, exprEvaluatorContext);
    
            // process results
            base.ProcessResults(prefillPath, result, joinedEvents);
        }
    }
}
