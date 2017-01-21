///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.plugin
{
    /// <summary>
    /// Context for use in <see cref="PlugInEventRepresentation"/> to provide information to
    /// help decide whether an event representation can handle the requested resolution
    /// URI for creating event object wrappers.
    /// </summary>
    public class PlugInEventBeanReflectorContext
    {
        private readonly Uri resolutionURI;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="uri">is the resolution URI provided as part of <see cref="EPRuntime.GetEventSender(Uri[])"/></param>
        public PlugInEventBeanReflectorContext(Uri uri)
        {
            this.resolutionURI = uri;
        }

        /// <summary>Returns the resolution URI. </summary>
        /// <returns>resolution URI</returns>
        public Uri ResolutionURI
        {
            get { return resolutionURI; }
        }
    }
}
