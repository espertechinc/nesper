///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.namedwindow.path;

namespace com.espertech.esper.common.@internal.epl.namedwindow.consume
{
    public class NamedWindowFactoryServiceImpl : NamedWindowFactoryService
    {
        public readonly static NamedWindowFactoryServiceImpl INSTANCE = new NamedWindowFactoryServiceImpl();

        private NamedWindowFactoryServiceImpl()
        {
        }

        public NamedWindow CreateNamedWindow(
            NamedWindowMetaData metadata,
            EPStatementInitServices services)
        {
            return new NamedWindowImpl(metadata, services);
        }

        public NamedWindowTailView CreateNamedWindowTailView(
            EventType eventType,
            bool isParentBatchWindow,
            EPStatementInitServices services,
            string contextNameWindow)
        {
            return new NamedWindowTailViewImpl(eventType, isParentBatchWindow, services);
        }
    }
} // end of namespace