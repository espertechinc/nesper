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
using com.espertech.esper.compat.logging;

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
    public sealed class IterablesListIterator : IEnumerator<EventBean> {
        private readonly IEnumerator<Iterable<EventBean>> listIterator;
        private IEnumerator<EventBean> currentIterator;
    
        /// <summary>
        /// Constructor - takes a list of Iterable that supply the iterators to iterate over.
        /// </summary>
        /// <param name="iteratorOfIterables">super-iterate of iterables</param>
        public IterablesListIterator(Iterator<Iterable<EventBean>> iteratorOfIterables) {
            listIterator = iteratorOfIterables;
            NextIterable();
        }
    
    
        public EventBean Next() {
            if (currentIterator == null) {
                throw new NoSuchElementException();
            }
            if (currentIterator.HasNext()) {
                return CurrentIterator.Next();
            }
    
            NextIterable();
    
            if (currentIterator == null) {
                throw new NoSuchElementException();
            }
            return CurrentIterator.Next();
        }
    
        public bool HasNext() {
            if (currentIterator == null) {
                return false;
            }
    
            if (currentIterator.HasNext()) {
                return true;
            }
    
            NextIterable();
    
            if (currentIterator == null) {
                return false;
            }
    
            return true;
        }
    
        public void Remove() {
            throw new UnsupportedOperationException();
        }
    
        private void NextIterable() {
            while (listIterator.HasNext()) {
                Iterable<EventBean> iterable = listIterator.Next();
                currentIterator = iterable.GetEnumerator();
                if (currentIterator.HasNext()) {
                    return;
                }
            }
    
            currentIterator = null;
        }
    }
    
    
} // end of namespace
