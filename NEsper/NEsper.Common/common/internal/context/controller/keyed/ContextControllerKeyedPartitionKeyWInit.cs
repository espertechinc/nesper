///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.context.controller.keyed
{
    /// <summary>
    ///     Keyed-context partition key for the case when there are init-conditions and we must keep the initiating event
    /// </summary>
    public class ContextControllerKeyedPartitionKeyWInit
    {
        public ContextControllerKeyedPartitionKeyWInit(
            object getterKey, string optionalInitAsName, EventBean optionalInitBean)
        {
            GetterKey = getterKey;
            OptionalInitAsName = optionalInitAsName;
            OptionalInitBean = optionalInitBean;
        }

        public object GetterKey { get; }

        public string OptionalInitAsName { get; }

        public EventBean OptionalInitBean { get; }
    }
} // end of namespace