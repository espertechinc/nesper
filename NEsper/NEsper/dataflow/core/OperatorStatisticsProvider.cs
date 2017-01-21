///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client.dataflow;
using com.espertech.esper.dataflow.util;

namespace com.espertech.esper.dataflow.core
{
    public class OperatorStatisticsProvider : EPDataFlowInstanceStatistics
    {
        private readonly long[][] _submitCounts;
        private readonly long[][] _cpuDelta;
        private readonly OperatorMetadataDescriptor[] _desc;

        public OperatorStatisticsProvider(IDictionary<int, OperatorMetadataDescriptor> operatorMetadata)
        {
            _submitCounts = new long[operatorMetadata.Count][];
            _cpuDelta = new long[operatorMetadata.Count][];
            _desc = new OperatorMetadataDescriptor[operatorMetadata.Count];
            foreach (var entry in operatorMetadata)
            {
                int opNum = entry.Key;
                _desc[opNum] = entry.Value;
                int numPorts = entry.Value.OperatorSpec.Output.Items.Count;
                _submitCounts[opNum] = new long[numPorts];
                _cpuDelta[opNum] = new long[numPorts];
            }
        }

        public IList<EPDataFlowInstanceOperatorStat> OperatorStatistics
        {
            get
            {
                var result = new List<EPDataFlowInstanceOperatorStat>(_submitCounts.Length);
                for (int i = 0; i < _submitCounts.Length; i++)
                {
                    long[] submittedPerPort = _submitCounts[i];
                    long submittedOverall = submittedPerPort.Sum();

                    long[] timePerPort = _cpuDelta[i];
                    long timeOverall = timePerPort.Sum();

                    var meta = _desc[i];
                    var stat = new EPDataFlowInstanceOperatorStat(
                        meta.OperatorName,
                        meta.OperatorPrettyPrint, i,
                        submittedOverall, submittedPerPort,
                        timeOverall, timePerPort);
                    result.Add(stat);
                }
                return result;
            }
        }

        public void CountSubmitPort(int producerOpNum, int portNumber)
        {
            _submitCounts[producerOpNum][portNumber]++;
        }

        public void CountSubmitPortWithTime(int producerOpNum, int portNumber, long nanoTimeDelta)
        {
            CountSubmitPort(producerOpNum, portNumber);
            _cpuDelta[producerOpNum][portNumber] += nanoTimeDelta;
        }
    }
}
