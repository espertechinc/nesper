///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


namespace com.espertech.esper.compat.collections
{
    public interface IBlockingQueue<T>
    {
        /// <summary>
        /// Gets the number of items in the queue.
        /// </summary>
        /// <value>The count.</value>
        int Count { get; }
        /// <summary>
        /// Clears all items from the queue
        /// </summary>
        void Clear();
        /// <summary>
        /// Pushes an item onto the queue.  If the queue has reached
        /// capacity, the call will pend until the queue has space to
        /// receive the request.
        /// </summary>
        /// <param name="item"></param>
        void Push(T item);
        /// <summary>
        /// Pops an item off the queue.  If there is nothing on the queue
        /// the call will pend until there is an item on the queue.
        /// </summary>
        /// <returns></returns>
        T Pop();

        /// <summary>
        /// Pops an item off the queue.  If there is nothing on the queue
        /// the call will pend until there is an item on the queue or
        /// the timeout has expired.  If the timeout has expired, the
        /// method will return false.
        /// </summary>
        /// <param name="maxTimeoutInMillis">The max timeout in millis.</param>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        bool Pop(int maxTimeoutInMillis, out T item);

        /// <summary>
        /// Determines whether this instance is empty.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </returns>
        bool IsEmpty();
    }
}
