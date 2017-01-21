///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.dataflow;
using com.espertech.esper.compat;
using com.espertech.esper.dataflow.interfaces;

namespace com.espertech.esper.dataflow.core
{
    public class EPDataFlowEmitterWrapperWStatistics : EPDataFlowEmitter
    {
        private readonly EPDataFlowEmitter _facility;
        private readonly int _producerOpNum;
        private readonly OperatorStatisticsProvider _statisticsProvider;
        private readonly bool _cpuStatistics;

        public EPDataFlowEmitterWrapperWStatistics(EPDataFlowEmitter facility, int producerOpNum, OperatorStatisticsProvider statisticsProvider, bool cpuStatistics)
        {
            _facility = facility;
            _producerOpNum = producerOpNum;
            _statisticsProvider = statisticsProvider;
            _cpuStatistics = cpuStatistics;
        }

        public void Submit(Object @object)
        {
            SubmitPort(0, @object);
        }

        public void SubmitSignal(EPDataFlowSignal signal)
        {
            _facility.SubmitSignal(signal);
        }

        public void SubmitPort(int portNumber, Object @object)
        {
            if (!_cpuStatistics)
            {
                _facility.SubmitPort(portNumber, @object);
                _statisticsProvider.CountSubmitPort(_producerOpNum, portNumber);
            }
            else
            {
                long nanoTime = PerformanceObserver.NanoTime;
                _facility.SubmitPort(portNumber, @object);
                long nanoTimDelta = PerformanceObserver.NanoTime - nanoTime;
                _statisticsProvider.CountSubmitPortWithTime(_producerOpNum, portNumber, nanoTimDelta);
            }
        }
    }
}
