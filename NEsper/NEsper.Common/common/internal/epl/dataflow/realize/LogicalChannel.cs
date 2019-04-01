///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.epl.dataflow.realize
{
    /// <summary>Models a pipe between two operators. </summary>
    public class LogicalChannel
    {
        public LogicalChannel(int channelId, String consumingOpName, int consumingOpNum, int consumingOpStreamNum, String consumingOpStreamName, String consumingOptStreamAliasName, String consumingOpPrettyPrint, LogicalChannelProducingPortCompiled outputPort)
        {
            ChannelId = channelId;
            ConsumingOpName = consumingOpName;
            ConsumingOpNum = consumingOpNum;
            ConsumingOpStreamNum = consumingOpStreamNum;
            ConsumingOpStreamName = consumingOpStreamName;
            ConsumingOptStreamAliasName = consumingOptStreamAliasName;
            ConsumingOpPrettyPrint = consumingOpPrettyPrint;
            OutputPort = outputPort;
        }

        public int ChannelId { get; private set; }

        public string ConsumingOpName { get; private set; }

        public string ConsumingOpStreamName { get; private set; }

        public string ConsumingOptStreamAliasName { get; private set; }

        public int ConsumingOpStreamNum { get; private set; }

        public int ConsumingOpNum { get; private set; }

        public LogicalChannelProducingPortCompiled OutputPort { get; private set; }

        public string ConsumingOpPrettyPrint { get; private set; }

        public override String ToString()
        {
            return "LogicalChannel{" +
                    "channelId=" + ChannelId +
                    ", produced=" + OutputPort +
                    ", consumed='" + ConsumingOpPrettyPrint + '\'' +
                    '}';
        }
    }
}