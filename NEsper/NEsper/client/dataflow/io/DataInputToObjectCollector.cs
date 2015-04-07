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
    /// <summary>Collects an object from {@link java.io.DataInput} and emits the object to an emitter. </summary>
    public interface DataInputToObjectCollector {
        /// <summary>Reads provided {@link java.io.DataInput} and emits an object using the provided emitter. </summary>
        /// <param name="context">contains input and emitter</param>
        /// <throws>IOException when the read operation cannot be completed</throws>
        void Collect(DataInputToObjectCollectorContext context)
    }
}
