///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.runtime.client.util
{
    /// <summary>
    ///     An Adapter takes some external data, converts it into events, and sends it
    ///     into the runtime runtime.
    /// </summary>
    public interface AdapterSPI : Adapter
    {
        /// <summary>
        ///     Returns the runtime instance.
        /// </summary>
        /// <returns>runtime</returns>
        EPRuntime Runtime { get; set; }
    }
} // end of namespace