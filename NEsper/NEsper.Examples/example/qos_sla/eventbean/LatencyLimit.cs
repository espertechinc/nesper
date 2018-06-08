///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace NEsper.Examples.QoS_SLA.eventbean
{
    public class LatencyLimit
    {
        private String _operationName;
        private String _customerId;
        private long _latencyThreshold;

        public LatencyLimit(String operationName, String customerId, long latencyThreshold)
        {
            this._operationName = operationName;
            this._customerId = customerId;
            this._latencyThreshold = latencyThreshold;
        }

        public String OperationName
        {
            get { return _operationName; }
        }

        public String CustomerId
        {
            get { return _customerId; }
        }

        public long LatencyThreshold
        {
            get { return _latencyThreshold; }
        }
    }
}
