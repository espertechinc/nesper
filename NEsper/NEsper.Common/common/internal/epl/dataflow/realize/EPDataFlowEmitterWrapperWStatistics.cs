///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;

namespace com.espertech.esper.common.@internal.epl.dataflow.realize
{
    public class EPDataFlowEmitterWrapperWStatistics : EPDataFlowEmitter
    {
        private readonly bool cpuStatistics;

        private readonly EPDataFlowEmitter facility;
        private readonly int producerOpNum;
        private readonly OperatorStatisticsProvider statisticsProvider;

        public EPDataFlowEmitterWrapperWStatistics(
            EPDataFlowEmitter facility,
            int producerOpNum,
            OperatorStatisticsProvider statisticsProvider,
            bool cpuStatistics)
        {
            this.facility = facility;
            this.producerOpNum = producerOpNum;
            this.statisticsProvider = statisticsProvider;
            this.cpuStatistics = cpuStatistics;
        }

        public void Submit(object @object)
        {
            SubmitPort(0, @object);
        }

        public void SubmitSignal(EPDataFlowSignal signal)
        {
            facility.SubmitSignal(signal);
        }

        public void SubmitPort(
            int portNumber,
            object @object)
        {
            if (!cpuStatistics) {
                facility.SubmitPort(portNumber, @object);
                statisticsProvider.CountSubmitPort(producerOpNum, portNumber);
            }
            else {
                long nanoTime = System.NanoTime();
                facility.SubmitPort(portNumber, @object);
                var nanoTimDelta = System.NanoTime() - nanoTime;
                statisticsProvider.CountSubmitPortWithTime(producerOpNum, portNumber, nanoTimDelta);
            }
        }
    }
} // end of namespace