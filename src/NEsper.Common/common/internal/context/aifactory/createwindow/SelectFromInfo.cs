///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.context.aifactory.createwindow
{
    internal class SelectFromInfo
    {
        private readonly EventType eventType;
        private readonly string typeName;

        public SelectFromInfo(
            EventType eventType,
            string typeName)
        {
            this.eventType = eventType;
            this.typeName = typeName;
        }

        public EventType EventType => eventType;

        public string TypeName => typeName;
    }
} // end of namespace