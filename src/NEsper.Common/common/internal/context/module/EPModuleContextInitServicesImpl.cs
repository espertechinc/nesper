///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.@event.path;

namespace com.espertech.esper.common.@internal.context.module
{
    public class EPModuleContextInitServicesImpl : EPModuleContextInitServices
    {
        public EPModuleContextInitServicesImpl(
            ContextCollector contextCollector,
            EventTypeResolver eventTypeResolver)
        {
            ContextCollector = contextCollector;
            EventTypeResolver = eventTypeResolver;
        }

        public ContextCollector ContextCollector { get; }

        public EventTypeResolver EventTypeResolver { get; }
    }
} // end of namespace