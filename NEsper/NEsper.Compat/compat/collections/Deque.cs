///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.compat.collections
{
    public interface Deque<T> : ICollection<T>, IVisitable<T>
    {
        void AddLast(T value);
        void AddFirst(T value);

        T RemoveFirst();
        T RemoveLast();

        /// <summary>
        /// Retrieves and removes the head of the queue represented by this deque or returns null if deque is empty.
        /// </summary>
        /// <returns></returns>
        T Poll();

        /// <summary>
        /// Retrieves, but does not remove, the head of the queue represented by this deque, or returns null if this deque is empty.
        /// </summary>
        /// <returns></returns>
        T Peek();

        T First { get; }
        T Last { get; }
    }
}
