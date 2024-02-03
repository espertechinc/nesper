///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.@event.xml
{
    public class EventTypeXMLXSDHandlerFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static EventTypeXMLXSDHandler Resolve(
            ImportService importService,
            ConfigurationCommonEventTypeMeta config,
            string handlerClass)
        {
            // Make services that depend on snapshot config entries
            EventTypeXMLXSDHandler xmlxsdHandler = EventTypeXMLXSDHandlerUnsupported.INSTANCE;
            if (config.IsEnableXmlXsd) {
                try {
                    xmlxsdHandler = TypeHelper.Instantiate<EventTypeXMLXSDHandler>(
                        handlerClass,
                        importService.TypeResolver);
                }
                catch (Exception t) {
                    Log.Warn(
                        "XML-XSD provider {} not instantiated, not enabling XML-XSD support: {}",
                        handlerClass,
                        t.Message);
                }
            }

            return xmlxsdHandler;
        }
    }
} // end of namespace