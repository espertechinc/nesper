///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client.dataflow;

namespace com.espertech.esper.dataflow.core
{
    public class SignalHandlerDefault : SignalHandler
    {
        internal static SignalHandlerDefault INSTANCE = new SignalHandlerDefault(); 
    
        public virtual void HandleSignal(EPDataFlowSignal signal)
        {
        }
    }
}
