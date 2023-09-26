///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.dataflow.util;

namespace com.espertech.esper.common.@internal.epl.dataflow.realize
{
    public class LogicalChannelProducingPortDeclared
    {
        private readonly int producingOpNum;
        private readonly string producingOpPrettyPrint;
        private readonly string streamName;
        private readonly int streamNumber;
        private readonly GraphTypeDesc typeDesc;
        private readonly bool hasPunctuation;

        public LogicalChannelProducingPortDeclared(
            int producingOpNum,
            string producingOpPrettyPrint,
            string streamName,
            int streamNumber,
            GraphTypeDesc typeDesc,
            bool hasPunctuation)
        {
            this.producingOpNum = producingOpNum;
            this.producingOpPrettyPrint = producingOpPrettyPrint;
            this.streamName = streamName;
            this.streamNumber = streamNumber;
            this.typeDesc = typeDesc;
            this.hasPunctuation = hasPunctuation;
        }

        public string ProducingOpPrettyPrint => producingOpPrettyPrint;

        public int ProducingOpNum => producingOpNum;

        public string StreamName => streamName;

        public int StreamNumber => streamNumber;

        public bool HasPunctuation => hasPunctuation;

        public GraphTypeDesc TypeDesc => typeDesc;

        public override string ToString()
        {
            return "LogicalChannelProducingPortSpec{" +
                   "op=" +
                   producingOpPrettyPrint +
                   '\'' +
                   ", streamName='" +
                   streamName +
                   '\'' +
                   ", portNumber=" +
                   streamNumber +
                   ", hasPunctuation=" +
                   hasPunctuation +
                   '}';
        }
    }
} // end of namespace