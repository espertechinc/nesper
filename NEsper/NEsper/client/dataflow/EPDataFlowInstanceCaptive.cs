///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.dataflow.ops;
using com.espertech.esper.dataflow.runnables;

namespace com.espertech.esper.client.dataflow
{
    /// <summary>Holder for captive data flow execution. </summary>
    public class EPDataFlowInstanceCaptive
    {
        /// <summary>Ctor. </summary>
        /// <param name="emitters">any emitters that are part of the data flow</param>
        /// <param name="runnables">any runnables that represent source operators</param>
        public EPDataFlowInstanceCaptive(IDictionary<String, Emitter> emitters, IList<GraphSourceRunnable> runnables)
        {
            Emitters = emitters;
            Runnables = runnables;
        }

        /// <summary>Map of named emitters. </summary>
        /// <value>emitters</value>
        public IDictionary<string, Emitter> Emitters { get; private set; }

        /// <summary>List of operator source runnables. </summary>
        /// <value>runnables</value>
        public IList<GraphSourceRunnable> Runnables { get; private set; }
    }
}
