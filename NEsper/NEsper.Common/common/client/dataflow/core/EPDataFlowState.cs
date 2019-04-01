///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.dataflow.core
{
    /// <summary>Data flow instance states. </summary>
    public enum EPDataFlowState {
        /// <summary>Start state: the state a data flow instance is in when it gets instantiated. </summary>
        INSTANTIATED,
    
        /// <summary>Running means the data flow instance is currently executing. </summary>
        RUNNING,
    
        /// <summary>Complete means the data flow instance completed. </summary>
        COMPLETE,
    
        /// <summary>Cancelled means the data flow instance was cancelled. </summary>
        CANCELLED,
    }
}
