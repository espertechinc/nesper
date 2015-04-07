///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

using com.espertech.esper.client;

namespace com.espertech.esper.collection
{
    /// <summary>
    /// An iterator over a list of iterables.
    /// The IterablesListIterator iterator takes a list of Iterable instances as a parameter. The iterator will
    /// Start at the very first Iterable and obtain it's iterator. It then allows iteration over this first iterator
    /// until that iterator returns no next value. Then the IterablesListIterator iterator will obtain the next iterable and iterate
    /// over this next iterable's iterator until no more values can be obtained. This continues until the last Iterable
    /// in the order of the list of Iterables.
    /// </summary>

    public sealed class IterablesListIterator : IEnumerator<EventBean>
    {
        private readonly IEnumerator<IEnumerable<EventBean>> listIterator;
        private IEnumerator<EventBean> currentEnumerator;

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        /// <value></value>
        /// <returns>The element in the collection at the current position of the enumerator.</returns>
        public EventBean Current
        {
            get
            {
                if (currentEnumerator == null)
                {
                    throw new InvalidOperationException();
                }

                return currentEnumerator.Current;
            }
        }

        /// <summary>
        /// Constructor - takes a list of Iterable that supply the iterators to iterate over.
        /// </summary>
        /// <param name="iterables">is a list of Iterable instances for which iterators to iterator over</param>

        public IterablesListIterator(IEnumerable<IEnumerable<EventBean>> iterables)
        {
            listIterator = iterables.GetEnumerator();
            AdvanceChild();
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>
        /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
        
        public bool MoveNext()
        {
            while (currentEnumerator != null)
            {
                if (currentEnumerator.MoveNext())
                {
                    return true;
                }

                AdvanceChild();
            }

            return false;
        }

        /// <summary>
        /// Advances the currentListIterator to the next item in the
        /// parent enumerator.
        /// </summary>
        
        private void AdvanceChild()
        {
            currentEnumerator = null;

            if (listIterator.MoveNext())
            {
                currentEnumerator = listIterator.Current.GetEnumerator();
            }
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
        
        public void Reset()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        
        public void Dispose()
        {
        }

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        /// <value></value>
        /// <returns>The element in the collection at the current position of the enumerator.</returns>

        Object System.Collections.IEnumerator.Current
        {
            get { return this.Current; }
        }
    }
}
