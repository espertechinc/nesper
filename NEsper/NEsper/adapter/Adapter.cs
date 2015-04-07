///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.adapter
{
    /// <summary>
    /// An Adapter takes some external data, converts it into events, and sends
    /// it into the runtime engine.
    /// </summary>
    public interface Adapter : IDisposable
    {
        /// <summary>Start the sending of events into the runtime egine.  </summary>
        /// <throws>EPException in case of errors processing the events</throws>
        void Start();

        /// <summary>Pause the sending of events after a Adapter has been started.  </summary>
        /// <throws>EPException if this Adapter has already been stopped</throws>
        void Pause();

        /// <summary>Resume sending events after the Adapter has been paused.  </summary>
        /// <throws>EPException in case of errors processing the events</throws>
        void Resume();

        /// <summary>Stop sending events and return the Adapter to the OPENED state, ready to be started once again.  </summary>
        /// <throws>EPException in case of errors releasing resources</throws>
        void Stop();

        /// <summary>Get the state of this Adapter.  </summary>
        /// <returns>state</returns>
        AdapterState GetState();
    }
}
