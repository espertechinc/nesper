///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.client
{
	/// <summary>
	/// Interface to add and remove Update listeners.
	/// </summary>

	public interface EPListenable
	{
	    /// <summary>
	    /// Occurs whenever new events are available or old events are removed.
	    /// </summary>
	    event UpdateEventHandler Events;

        /// <summary>
        /// Removes all event handlers.
        /// </summary>
        void RemoveAllEventHandlers();

        #if UPDATE_LISTENERS


        /// <summary> Add an listener that observes events.</summary>
		/// <param name="listener">to add
		/// </param>
        void AddListener( UpdateListener listener );

		/// <summary> Remove an listener that observes events.</summary>
		/// <param name="listener">to remove
		/// </param>
        void RemoveListener( UpdateListener listener );

		/// <summary> Remove all listeners.</summary>
        void RemoveAllListeners();

        /// <summary>
        /// Add a statement-aware listener that observes events.
        /// </summary>
        /// <param name="listener">The listener.</param>
        void AddListener(StatementAwareUpdateListener listener);

        /// <summary>
        /// Remove a statement-aware listener that observes events.
        /// </summary>
        void RemoveListener(StatementAwareUpdateListener listener);

        /// <summary>
        /// Returns an enumerable of statement-aware Update listeners.
        /// </summary>
        IEnumerable<StatementAwareUpdateListener> StatementAwareListeners { get;}

        /// <summary>
        /// Gets the Update listeners.
        /// </summary>
        /// <value>The Update listeners.</value>
        IEnumerable<UpdateListener> UpdateListeners { get; }

        #endif
    }
}
