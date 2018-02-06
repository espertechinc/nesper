///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


namespace NEsper.Examples.QoS_SLA.eventbean
{
    public class OperationMeasurement
    {
        public OperationMeasurement(string operationName,
                                    string customerId,
                                    long latency,
                                    bool success)
        {
            OperationName = operationName;
            CustomerId = customerId;
            Latency = latency;
            Success = success;
        }

        public string OperationName { get; private set; }

        public string CustomerId { get; private set; }

        public long Latency { get; private set; }

        public bool Success { get; private set; }
    }
}
