///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.runtime.@internal.kernel.statement
{
    /// <summary>
    ///     Implements the safe iterator. The class is passed a lock that is locked already, to release
    ///     when the close method closes the iterator.
    /// </summary>
    public class SafeEnumeratorWTableImpl<E> : SafeEnumeratorImpl<E>
    {
        private readonly TableExprEvaluatorContext tableExprEvaluatorContext;

        public SafeEnumeratorWTableImpl(
            IReaderWriterLock iteratorLock,
            IEnumerator<E> underlying,
            TableExprEvaluatorContext tableExprEvaluatorContext)
            : base(iteratorLock, underlying)
        {
            this.tableExprEvaluatorContext = tableExprEvaluatorContext;
        }

        public override void Dispose()
        {
            base.Dispose();
            tableExprEvaluatorContext.ReleaseAcquiredLocks();
        }
    }
} // end of namespace