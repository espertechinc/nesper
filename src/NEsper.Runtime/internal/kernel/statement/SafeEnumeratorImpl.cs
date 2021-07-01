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
using com.espertech.esper.common.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.runtime.@internal.kernel.statement
{
    /// <summary>
    /// Implements the safe iterator. The class is passed a lock that is locked already, to release
    /// when the close method closes the iterator.
    /// </summary>
    public class SafeEnumeratorImpl<E> : SafeEnumerator<E>
    {
        private IDisposable iteratorLock;
        private readonly IEnumerator<E> underlying;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="iteratorLock">for locking resources to safely-iterate over</param>
        /// <param name="underlying">is the underlying iterator to protect</param>
        public SafeEnumeratorImpl(IReaderWriterLock iteratorLock, IEnumerator<E> underlying)
        {
            // acquire a unique lock just for the iterator
            this.iteratorLock = iteratorLock.AcquireReadLock();
            this.underlying = underlying;
        }

        public virtual void Dispose()
        {
            if (iteratorLock != null)
            {
                iteratorLock.Dispose();
                iteratorLock = null;
            }
        }

        public bool MoveNext()
        {
            return underlying.MoveNext();
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator.Current => Current;

        public E Current => underlying.Current;

        public void Remove()
        {
            throw new UnsupportedOperationException("Remove operation not supported");
        }
    }
} // end of namespace