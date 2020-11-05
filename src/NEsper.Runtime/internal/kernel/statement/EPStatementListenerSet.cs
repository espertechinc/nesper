///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.runtime.@internal.kernel.statement
{
    /// <summary>
    ///     Provides update listeners for use by statement instances, and the management methods around these.
    ///     <para>
    ///         The collection of update listeners is based on copy-on-write:
    ///         When the runtime dispatches events to a set of listeners, then while iterating
    ///         through the set there may be listeners added or removed (the listener may remove
    ///         itself).  Additionally, events may be dispatched by multiple threads to the same
    ///         listener.
    ///     </para>
    /// </summary>
    public class EPStatementListenerSet : EPStatementHandlerBase
    {
        private static readonly UpdateListener[] EMPTY_UPDATE_LISTENER_ARRAY = new UpdateListener[0];

        private volatile UpdateListener[] listeners;

        /// <summary>
        ///     Ctor.
        /// </summary>
        public EPStatementListenerSet()
        {
            listeners = EMPTY_UPDATE_LISTENER_ARRAY;
        }

        public EPStatementListenerSet(UpdateListener[] listeners)
        {
            this.listeners = listeners;
        }

        /// <summary>
        ///     Returns the set of listeners to the statement.
        /// </summary>
        /// <returns>statement listeners</returns>
        public UpdateListener[] Listeners {
            get => listeners;
        }
        
        /// <summary>
        ///     Set the update listener set to use.
        /// </summary>
        /// <param name="listenerSet">a collection of update listeners</param>
        public void SetListeners(EPStatementListenerSet listenerSet)
        {
            listeners = listenerSet.Listeners;
        }

        /// <summary>
        ///     Add a listener to the statement.
        /// </summary>
        /// <param name="listener">to add</param>
        public void AddListener(UpdateListener listener)
        {
            lock (this) {
                if (listener == null) {
                    throw new ArgumentException("Null listener reference supplied");
                }

                foreach (var existing in listeners) {
                    if (existing == listener) {
                        return;
                    }
                }

                listeners = (UpdateListener[]) CollectionUtil.ArrayExpandAddSingle(listeners, listener);
            }
        }

        /// <summary>
        ///     Remove a listeners to a statement.
        /// </summary>
        /// <param name="listener">to remove</param>
        public void RemoveListener(UpdateListener listener)
        {
            lock (this) {
                if (listener == null) {
                    throw new ArgumentException("Null listener reference supplied");
                }

                var index = -1;
                for (var i = 0; i < listeners.Length; i++) {
                    if (listeners[i] == listener) {
                        index = i;
                        break;
                    }
                }

                if (index == -1) {
                    return;
                }

                listeners = (UpdateListener[]) CollectionUtil.ArrayShrinkRemoveSingle(listeners, index);
            }
        }

        /// <summary>
        ///     Remove all listeners to a statement.
        /// </summary>
        public void RemoveAllListeners()
        {
            lock (this) {
                listeners = EMPTY_UPDATE_LISTENER_ARRAY;
            }
        }

        /// <summary>
        ///     Remove all listeners to a statement.
        /// </summary>
        public void RemoveAllListeners(Func<UpdateListener, bool> listenerPredicate)
        {
            lock (this) {
                listeners = listeners.Where(listenerPredicate).ToArray();
            }
        }
    }
} // end of namespace