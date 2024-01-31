///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.path;

namespace com.espertech.esper.common.@internal.serde.runtime.eventtype
{
    public interface EventTypeSerdeRepository
    {
        void AddSerdes(
            string deploymentId,
            IList<EventTypeCollectedSerde> serdes,
            IDictionary<string, EventType> moduleEventTypes,
            BeanEventTypeFactoryPrivate beanEventTypeFactory);

        void RemoveSerdes(string deploymentId);
    }
} // end of namespace