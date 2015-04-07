///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.core.service
{
    public class InternalEventRouterDesc
    {
        public InternalEventRouterDesc(UpdateDesc updateDesc,
                                       EventBeanCopyMethod copyMethod,
                                       TypeWidener[] wideners,
                                       EventType eventType,
                                       Attribute[] annotations)
        {
            UpdateDesc = updateDesc;
            CopyMethod = copyMethod;
            Wideners = wideners;
            EventType = eventType;
            Annotations = annotations;
        }

        public UpdateDesc UpdateDesc { get; private set; }

        public EventBeanCopyMethod CopyMethod { get; private set; }

        public TypeWidener[] Wideners { get; private set; }

        public EventType EventType { get; private set; }

        public Attribute[] Annotations { get; private set; }
    }
}