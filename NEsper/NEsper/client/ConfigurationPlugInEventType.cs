///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.client
{
    /// <summary>
    /// Configuration for a plug-in event type, which is an event type resolved via
    /// plug-in event representation.
    /// </summary>
    [Serializable]
    public class ConfigurationPlugInEventType
    {
        /// <summary>
        /// Gets or sets the URIs to use to resolve the new event type against the plug-in event representations registered.
        /// </summary>
        /// <value>The event representation resolution URI is.</value>
        public IList<Uri> EventRepresentationResolutionURIs { get; set; }

        /// <summary>
        /// Gets or sets the optional initialization information that the plug-in event representation may use to set up the event type.
        /// </summary>
        /// <value>The initializer.</value>
        public object Initializer { get; set; }
    }
}
