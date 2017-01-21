///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.client.dataflow
{
    /// <summary>
    /// Collector for use with the <seealso cref="com.espertech.esper.dataflow.ops.EPStatementSource" /> operator.
    /// </summary>
    public interface EPDataFlowIRStreamCollector
    {
        /// <summary>
        /// Collect: use the context to transform statement output Event(s) to data flow Event(s).
        /// </summary>
        /// <param name="context">contains event bean, emitter and related information</param>
        void Collect(EPDataFlowIRStreamCollectorContext context);
    }
}
