///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextControllerInitTermInstance
    {
        public ContextControllerInitTermInstance(ContextControllerInstanceHandle instanceHandle,
                                                 IDictionary<String, Object> startProperties,
                                                 long startTime,
                                                 long? endTime,
                                                 int subPathId)
        {
            InstanceHandle = instanceHandle;
            StartProperties = startProperties;
            StartTime = startTime;
            EndTime = endTime;
            SubPathId = subPathId;
        }

        public ContextControllerInstanceHandle InstanceHandle { get; private set; }

        public IDictionary<string, object> StartProperties { get; private set; }

        public long StartTime { get; private set; }

        public long? EndTime { get; private set; }

        public int SubPathId { get; private set; }
    }
}