///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.dataflow.realize
{
    public class EPDataFlowEmitterWrapperWStatistics : EPDataFlowEmitter
    {
        private readonly bool _cpuStatistics;

        private readonly EPDataFlowEmitter _facility;
        private readonly int _producerOpNum;
        private readonly OperatorStatisticsProvider _statisticsProvider;

        public EPDataFlowEmitterWrapperWStatistics(
            EPDataFlowEmitter facility,
            int producerOpNum,
            OperatorStatisticsProvider statisticsProvider,
            bool cpuStatistics)
        {
            _facility = facility;
            _producerOpNum = producerOpNum;
            _statisticsProvider = statisticsProvider;
            _cpuStatistics = cpuStatistics;
        }

        public void Submit(object @object)
        {
            SubmitPort(0, @object);
        }

        public void SubmitSignal(EPDataFlowSignal signal)
        {
            _facility.SubmitSignal(signal);
        }

        public void SubmitPort(
            int portNumber,
            object @object)
        {
            if (!_cpuStatistics) {
                _facility.SubmitPort(portNumber, @object);
                _statisticsProvider.CountSubmitPort(_producerOpNum, portNumber);
            }
            else {
                var nanoTime = PerformanceObserver.NanoTime;
                _facility.SubmitPort(portNumber, @object);
                var nanoTimDelta = PerformanceObserver.NanoTime - nanoTime;
                _statisticsProvider.CountSubmitPortWithTime(_producerOpNum, portNumber, nanoTimDelta);
            }
        }
    }
} // end of namespace