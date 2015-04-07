///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.dataflow.interfaces;

namespace com.espertech.esper.client.dataflow.io
{
    /// <summary>Context for use with {@link DataInputToObjectCollector} carries data input and emitter. </summary>
    public class DataInputToObjectCollectorContext {
        private EPDataFlowEmitter emitter;
        private DataInput dataInput;
    
        /// <summary>Returns the emitter. </summary>
        /// <returns>emitter</returns>
        public EPDataFlowEmitter GetEmitter() {
            return emitter;
        }
    
        /// <summary>Sets the emitter </summary>
        /// <param name="emitter">emitter</param>
        public void SetEmitter(EPDataFlowEmitter emitter) {
            this.emitter = emitter;
        }
    
        /// <summary>Returns the data input. </summary>
        /// <returns>data input</returns>
        public DataInput GetDataInput() {
            return dataInput;
        }
    
        /// <summary>Sets the data input. </summary>
        /// <param name="dataInput">data input</param>
        public void SetDataInput(DataInput dataInput) {
            this.dataInput = dataInput;
        }
    }
}
