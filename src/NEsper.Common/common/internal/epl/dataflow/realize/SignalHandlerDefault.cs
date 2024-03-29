///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.dataflow.util;

namespace com.espertech.esper.common.@internal.epl.dataflow.realize
{
    public class SignalHandlerDefault : SignalHandler
    {
        protected internal static readonly SignalHandlerDefault INSTANCE = new SignalHandlerDefault();

        public virtual void HandleSignal(EPDataFlowSignal signal)
        {
        }
    }
} // end of namespace