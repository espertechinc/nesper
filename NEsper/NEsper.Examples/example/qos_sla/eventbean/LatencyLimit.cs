///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace com.espertech.esper.example.qos_sla.eventbean
{
    public class LatencyLimit
    {
        private String operationName;
        private String customerId;
        private long latencyThreshold;

        public LatencyLimit(String operationName, String customerId, long latencyThreshold)
        {
            this.operationName = operationName;
            this.customerId = customerId;
            this.latencyThreshold = latencyThreshold;
        }

        public String OperationName
        {
            get { return operationName; }
        }

        public String CustomerId
        {
            get { return customerId; }
        }

        public long LatencyThreshold
        {
            get { return latencyThreshold; }
        }
    }
}
