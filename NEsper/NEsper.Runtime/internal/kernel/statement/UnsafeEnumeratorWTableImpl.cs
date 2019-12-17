///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.table.core;

namespace com.espertech.esper.runtime.@internal.kernel.statement
{
    /// <summary>
    /// Implements the iterator with table evaluation concern.
    /// </summary>
    public class UnsafeEnumeratorWTableImpl<TE> : IEnumerator<TE>
    {
        private readonly TableExprEvaluatorContext tableExprEvaluatorContext;
        private readonly IEnumerator<TE> inner;

        public UnsafeEnumeratorWTableImpl(
            TableExprEvaluatorContext tableExprEvaluatorContext,
            IEnumerator<TE> inner)
        {
            this.tableExprEvaluatorContext = tableExprEvaluatorContext;
            this.inner = inner;
        }

        public void Dispose()
        {
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public bool MoveNext()
        {
            return inner.MoveNext();
        }

        public TE Current {
            get {
                TE e = inner.Current;
                tableExprEvaluatorContext.ReleaseAcquiredLocks();
                return e;
            }
        }

        object IEnumerator.Current => Current;
    }
} // end of namespace