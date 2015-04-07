///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.client.dataflow.io
{
    /// <summary>Receives an object and writes to {@link java.io.DataOutput}. </summary>
    public interface ObjectToDataOutputCollector {
        /// <summary>Write the received object to {@link java.io.DataOutput}. </summary>
        /// <param name="context">the object and output</param>
        /// <throws>IOException when the write operation failed</throws>
        void Collect(ObjectToDataOutputCollectorContext context)
    }
}
