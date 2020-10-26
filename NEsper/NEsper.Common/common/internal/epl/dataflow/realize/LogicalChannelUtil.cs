///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.dataflow.realize
{
    public class LogicalChannelUtil
    {
        public static List<LogicalChannelBinding> GetBindingsConsuming(
            int producerOpNum,
            IList<LogicalChannelBinding> bindings)
        {
            return bindings.Where(binding => binding.LogicalChannel.OutputPort.ProducingOpNum == producerOpNum)
                .ToList();
        }

        public static string PrintChannels(IList<LogicalChannel> channels)
        {
            var writer = new StringWriter();
            writer.Write("\n");
            foreach (LogicalChannel channel in channels) {
                writer.WriteLine(channel);
            }

            return writer.ToString();
        }

        public static List<LogicalChannelProducingPortCompiled> GetOutputPortByStreamName(
            ICollection<int> incomingOpNums,
            string[] inputStreamNames,
            IDictionary<int, IList<LogicalChannelProducingPortCompiled>> compiledOutputPorts)
        {
            var ports = new List<LogicalChannelProducingPortCompiled>();
            foreach (int @operator in incomingOpNums) {
                var opPorts = compiledOutputPorts.Get(@operator);
                if (opPorts != null) { // Can be null if referring to itself
                    foreach (LogicalChannelProducingPortCompiled opPort in opPorts) {
                        ports.AddRange(
                            inputStreamNames.Where(name => name == opPort.StreamName).Select(streamName => opPort));
                    }
                }
            }

            return ports;
        }
    }
}