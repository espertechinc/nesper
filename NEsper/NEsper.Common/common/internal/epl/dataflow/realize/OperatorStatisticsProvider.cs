///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.@internal.epl.dataflow.util;

namespace com.espertech.esper.common.@internal.epl.dataflow.realize
{
    public class OperatorStatisticsProvider : EPDataFlowInstanceStatistics
    {
        private readonly long[][] cpuDelta;
        private readonly OperatorMetadataDescriptor[] desc;
        private readonly long[][] submitCounts;

        public OperatorStatisticsProvider(IDictionary<int, OperatorMetadataDescriptor> operatorMetadata)
        {
            submitCounts = new long[operatorMetadata.Count][];
            cpuDelta = new long[operatorMetadata.Count][];
            desc = new OperatorMetadataDescriptor[operatorMetadata.Count];
            foreach (var entry in operatorMetadata) {
                var opNum = entry.Key;
                desc[opNum] = entry.Value;
                var numPorts = entry.Value.NumOutputPorts;
                submitCounts[opNum] = new long[numPorts];
                cpuDelta[opNum] = new long[numPorts];
            }
        }

        public IList<EPDataFlowInstanceOperatorStat> OperatorStatistics {
            get {
                IList<EPDataFlowInstanceOperatorStat> result =
                    new List<EPDataFlowInstanceOperatorStat>(submitCounts.Length);
                for (var i = 0; i < submitCounts.Length; i++) {
                    var submittedPerPort = submitCounts[i];
                    long submittedOverall = 0;
                    foreach (var port in submittedPerPort) {
                        submittedOverall += port;
                    }

                    var timePerPort = cpuDelta[i];
                    long timeOverall = 0;
                    foreach (var port in timePerPort) {
                        timeOverall += port;
                    }

                    var meta = desc[i];
                    var stat = new EPDataFlowInstanceOperatorStat(
                        meta.OperatorName,
                        meta.OperatorPrettyPrint,
                        i,
                        submittedOverall,
                        submittedPerPort,
                        timeOverall,
                        timePerPort);
                    result.Add(stat);
                }

                return result;
            }
        }

        public void CountSubmitPort(
            int producerOpNum,
            int portNumber)
        {
            submitCounts[producerOpNum][portNumber]++;
        }

        public void CountSubmitPortWithTime(
            int producerOpNum,
            int portNumber,
            long nanoTimeDelta)
        {
            CountSubmitPort(producerOpNum, portNumber);
            cpuDelta[producerOpNum][portNumber] += nanoTimeDelta;
        }
    }
} // end of namespace