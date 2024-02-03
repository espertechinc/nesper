///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.runtime.@internal.dataflow.op.epstatementsource
{
    public class PortAndMessagePair
    {
        public PortAndMessagePair(
            int port,
            object message)
        {
            Port = port;
            Message = message;
        }

        public int Port { get; }

        public object Message { get; }
    }
} // end of namespace