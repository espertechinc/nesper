///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


namespace com.espertech.esper.common.@internal.statement.dispatch
{
    /// <summary>
    /// Service for dispatching internally (for operators/views processing results of prior operators/views)
    /// and externally (dispatch events to UpdateListener implementations).
    /// <para>
    /// The service accepts Dispatchable implementations to its internal and external lists.
    /// When a client invokes dispatch the implementation first invokes all internal Dispatchable
    /// instances then all external Dispatchable instances. Dispatchables are invoked
    /// in the same order they are added. Any dispatchable added twice is dispatched once.
    /// </para>
    /// <para>
    /// Note: Each execution thread owns its own dispatch queue.
    /// </para>
    /// <para>
    /// Note: Dispatchs could result in further call to the dispatch service. This is because listener code
    /// that is invoked as a result of a dispatch may create patterns that fire as soon as they are Started
    /// resulting in further dispatches within the same thread. Thus the implementation class must be careful
    /// with the use of iterators to avoid ConcurrentModificationException errors.
    /// </para>
    /// </summary>
    public interface DispatchService
    {
        /// <summary> Add a Dispatchable implementation.</summary>
        /// <param name="dispatchable">to execute later
        /// </param>
        void AddExternal(Dispatchable dispatchable);

        /// <summary> Execute all Dispatchable implementations added to the service since the last invocation of this method.</summary>
        void Dispatch();
    }
}