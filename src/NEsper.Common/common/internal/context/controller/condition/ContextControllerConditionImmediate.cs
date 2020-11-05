///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.context.controller.condition
{
    public class ContextControllerConditionImmediate : ContextControllerConditionNonHA
    {
        public static readonly ContextControllerConditionImmediate INSTANCE = new ContextControllerConditionImmediate();

        private ContextControllerConditionImmediate()
        {
        }

        public ContextConditionDescriptor Descriptor => ContextConditionDescriptorImmediate.INSTANCE;

        public bool Activate(
            EventBean optionalTriggeringEvent,
            ContextControllerEndConditionMatchEventProvider endConditionMatchEventProvider,
            IDictionary<string, object> optionalTriggeringPattern)
        {
            return false;
        }

        public void Deactivate()
        {
        }

        public void Transfer(AgentInstanceTransferServices xfer)
        {
        }

        public bool IsImmediate => true;

        public bool IsRunning => false;

        public long? ExpectedEndTime => null;
    }
} // end of namespace