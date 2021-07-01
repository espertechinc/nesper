///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

namespace com.espertech.esper.compat.threading.locks
{
    public class TelemetryLockCategory
    {
        /// <summary>
        /// List of telemetry events for this category.
        /// </summary>
        private readonly LinkedList<TelemetryEventArgs> _telemetryEvents =
            new LinkedList<TelemetryEventArgs>();

        /// <summary>
        /// Gets the telemetry events.
        /// </summary>
        /// <value>The events.</value>
        public ICollection<TelemetryEventArgs> Events
        {
            get
            {
                lock(this) {
                    return _telemetryEvents.ToArray();
                }
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Called when a lock is released.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TelemetryEventArgs"/> instance containing the event data.</param>
        public void OnLockReleased(object sender, TelemetryEventArgs e)
        {
            lock (this) {
                _telemetryEvents.AddLast(e);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryLockCategory"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public TelemetryLockCategory(string name)
        {
            Name = name;
        }
    }
}
