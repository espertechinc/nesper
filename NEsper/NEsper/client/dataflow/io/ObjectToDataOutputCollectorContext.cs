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
    /// <summary>Context for use with {@link ObjectToDataOutputCollector} carries object and data output. </summary>
    public class ObjectToDataOutputCollectorContext {
        private DataOutput dataOutput;
        private Object event;
    
        /// <summary>Returns the data output </summary>
        /// <returns>data output</returns>
        public DataOutput GetDataOutput() {
            return dataOutput;
        }
    
        /// <summary>Sets the data output </summary>
        /// <param name="dataOutput">data output</param>
        public void SetDataOutput(DataOutput dataOutput) {
            this.dataOutput = dataOutput;
        }
    
        /// <summary>Returns the event object. </summary>
        /// <returns>event object</returns>
        public Object GetEvent() {
            return @event;
        }
    
        /// <summary>Sets the event object. </summary>
        /// <param name="event">event object</param>
        public void SetEvent(Object @event) {
            this.event = @event;
        }
    }
}
