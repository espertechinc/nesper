///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.client.dataflow
{
    /// <summary>
    /// Data flow instanve.
    /// </summary>
    public interface EPDataFlowInstance
    {
        /// <summary>Returns the data flow name. </summary>
        /// <value>name</value>
        string DataFlowName { get; }

        /// <summary>Returns the state. </summary>
        /// <value>state</value>
        EPDataFlowState State { get; }

        /// <summary>Blocking execution of the data flow instance. </summary>
        /// <throws>IllegalStateException thrown to indicate that the state is not instantiated.</throws>
        /// <throws>EPDataFlowExecutionException thrown when an execution exception occurs</throws>
        /// <throws>EPDataFlowCancellationException throw to indicate the data flow was cancelled.</throws>
        void Run();

        /// <summary>Non-Blocking execution of the data flow instance. </summary>
        /// <throws>IllegalStateException thrown to indicate that the state is not instantiated.</throws>
        void Start();
    
        /// <summary>Captive execution of the data flow instance. </summary>
        /// <returns>runnables and emitters</returns>
        EPDataFlowInstanceCaptive StartCaptive();

        /// <summary>Join an executing data flow instance. </summary>
        /// <throws>IllegalStateException thrown if it cannot be joined</throws>
        /// <throws>InterruptedException thrown if interrupted</throws>
        void Join();
    
        /// <summary>Cancel execution. </summary>
        void Cancel();

        /// <summary>Get data flow instance statistics, required instantiation with statistics option, use <seealso cref="EPDataFlowInstantiationOptions" /> to turn on stats. </summary>
        /// <value>stats</value>
        EPDataFlowInstanceStatistics Statistics { get; }

        /// <summary>Returns the user object associated, if any. Use <seealso cref="EPDataFlowInstantiationOptions" /> to associate. </summary>
        /// <value>user object</value>
        object UserObject { get; }

        /// <summary>Returns the instance id associated, if any. Use <seealso cref="EPDataFlowInstantiationOptions" /> to associate. </summary>
        /// <value>instance if</value>
        string InstanceId { get; }

        /// <summary>Returns runtime parameters provided at instantiation time, or null if none have been provided. </summary>
        /// <value>runtime parameters</value>
        IDictionary<string, object> Parameters { get; }
    }
}
