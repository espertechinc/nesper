///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.dataflow.util;

namespace com.espertech.esper.common.client.dataflow.core
{
    /// <summary>
    /// Emitter for use with data flow operators
    /// </summary>
    public interface EPDataFlowEmitterOperator
    {
        /// <summary>
        /// Returns emitter name
        /// </summary>
        /// <value>name</value>
        string Name { get; }

        /// <summary>
        /// Submit an underlying event
        /// </summary>
        /// <param name="underlying">to process</param>
        void Submit(object underlying);

        /// <summary>
        /// Submit a signal
        /// </summary>
        /// <param name="signal">to process</param>
        void SubmitSignal(EPDataFlowSignal signal);

        /// <summary>
        /// Submit an underlying event to a given port
        /// </summary>
        /// <param name="object">to process</param>
        /// <param name="portNumber">port</param>
        void SubmitPort(
            int portNumber,
            object @object);
    }
} // end of namespace