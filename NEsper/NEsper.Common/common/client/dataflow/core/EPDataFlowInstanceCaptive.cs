///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.epl.dataflow.runnables;

namespace com.espertech.esper.common.client.dataflow.core
{
    /// <summary>
    ///     Holder for captive data flow execution.
    /// </summary>
    public class EPDataFlowInstanceCaptive
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="emitters">any emitters that are part of the data flow</param>
        /// <param name="runnables">any runnables that represent source operators</param>
        public EPDataFlowInstanceCaptive(
            IDictionary<string, EPDataFlowEmitterOperator> emitters, IList<GraphSourceRunnable> runnables)
        {
            Emitters = emitters;
            Runnables = runnables;
        }

        /// <summary>
        ///     Map of named emitters.
        /// </summary>
        /// <value>emitters</value>
        public IDictionary<string, EPDataFlowEmitterOperator> Emitters { get; }

        /// <summary>
        ///     List of operator source runnables.
        /// </summary>
        /// <returns>runnables</returns>
        public IList<GraphSourceRunnable> Runnables { get; }
    }
} // end of namespace