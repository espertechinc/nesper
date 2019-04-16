///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.context.util
{
    /// <summary>
    /// Interface for a service that routes events within the engine for further processing.
    /// </summary>
    public interface InternalEventRouteDest
    {
        /// <summary>
        /// Route the event such that the event is processed as required.
        /// </summary>
        /// <param name="theEvent">to route</param>
        /// <param name="statementHandle">provides statement resources</param>
        /// <param name="addToFront">if set to <c>true</c> [add to front].</param>
        void Route(
            EventBean theEvent,
            EPStatementHandle statementHandle,
            bool addToFront);

        InternalEventRouter InternalEventRouter { set; }

        void ProcessThreadWorkQueue();

        void Dispatch();

        string EngineURI { get; }
    }
}