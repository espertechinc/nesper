///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.plugin;

namespace com.espertech.esper.supportregression.events
{
    public class SupportEventRepresentation : PlugInEventRepresentation
    {
        public void Init(PlugInEventRepresentationContext eventRepresentationContext)
        {
            InitContext = eventRepresentationContext;
        }
    
        public bool AcceptsType(PlugInEventTypeHandlerContext acceptTypeContext)
        {
            AcceptTypeContext = acceptTypeContext;
            return true;
        }
    
        public PlugInEventTypeHandler GetTypeHandler(PlugInEventTypeHandlerContext eventTypeContext)
        {
            EventTypeContext = eventTypeContext;
            return new ProxyPlugInEventTypeHandler
            {
                EventTypeFunc = () => null,
                GetSenderFunc = r => null
            };
        }
    
        public bool AcceptsEventBeanResolution(PlugInEventBeanReflectorContext context)
        {
            AcceptBeanContext = context;
            return true;
        }
    
        public PlugInEventBeanFactory GetEventBeanFactory(PlugInEventBeanReflectorContext context)
        {
            EventBeanContext = context;
            return (e, resolutionURI) => null;
        }

        public static PlugInEventRepresentationContext InitContext { get; private set; }

        public static PlugInEventTypeHandlerContext AcceptTypeContext { get; private set; }

        public static PlugInEventTypeHandlerContext EventTypeContext { get; private set; }

        public static PlugInEventBeanReflectorContext AcceptBeanContext { get; private set; }

        public static PlugInEventBeanReflectorContext EventBeanContext { get; private set; }
    }
}
