///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.dataflow.util
{
    public class LogicalChannelProducingPortDeclared
    {
        public LogicalChannelProducingPortDeclared(int producingOpNum,
                                                   String producingOpPrettyPrint,
                                                   String streamName,
                                                   int streamNumber,
                                                   GraphTypeDesc typeDesc,
                                                   bool hasPunctuation)
        {
            ProducingOpNum = producingOpNum;
            ProducingOpPrettyPrint = producingOpPrettyPrint;
            StreamName = streamName;
            StreamNumber = streamNumber;
            TypeDesc = typeDesc;
            HasPunctuation = hasPunctuation;
        }

        public string ProducingOpPrettyPrint { get; private set; }

        public int ProducingOpNum { get; private set; }

        public string StreamName { get; private set; }

        public int StreamNumber { get; private set; }

        public bool HasPunctuation { get; private set; }

        public GraphTypeDesc TypeDesc { get; private set; }

        public override String ToString()
        {
            return "LogicalChannelProducingPortSpec{" +
                   "op=" + ProducingOpPrettyPrint + '\'' +
                   ", streamName='" + StreamName + '\'' +
                   ", portNumber=" + StreamNumber +
                   ", hasPunctuation=" + HasPunctuation +
                   '}';
        }
    }
}