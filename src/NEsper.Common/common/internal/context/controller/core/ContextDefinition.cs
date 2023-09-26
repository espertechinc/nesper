///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;

namespace com.espertech.esper.common.@internal.context.controller.core
{
    public class ContextDefinition
    {
        public string ContextName { get; set; }

        public ContextControllerFactory[] ControllerFactories { get; set; }

        public EventType EventTypeContextProperties { get; set; }

        public StateMgmtSetting PartitionIdSvcStateMgmtSettings { get; set; }
    }
} // end of namespace