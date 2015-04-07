///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;

namespace com.espertech.esper.adapter
{
    /// <summary>
    /// An Adapter takes some external data, converts it into events, and sends it into
    /// the runtime engine.
    /// </summary>
    public interface AdapterSPI
    {
        /// <summary>Gets or sets the engine instance. </summary>
        /// <returns>engine</returns>
        EPServiceProvider EPServiceProvider { get; set; }
    }
}
