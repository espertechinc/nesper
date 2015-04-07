///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.dataflow.util
{
    public class LogicalChannelBinding
    {
        public LogicalChannelBinding(LogicalChannel logicalChannel,
                                     LogicalChannelBindingMethodDesc consumingBindingDesc,
                                     LogicalChannelBindingMethodDesc consumingSignalBindingDesc)
        {
            LogicalChannel = logicalChannel;
            ConsumingBindingDesc = consumingBindingDesc;
            ConsumingSignalBindingDesc = consumingSignalBindingDesc;
        }

        public LogicalChannel LogicalChannel { get; private set; }

        public LogicalChannelBindingMethodDesc ConsumingBindingDesc { get; private set; }

        public LogicalChannelBindingMethodDesc ConsumingSignalBindingDesc { get; private set; }
    }
}