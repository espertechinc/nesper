///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;

namespace com.espertech.esper.util
{
    public enum EventRepresentationEnum
    {
        OBJECTARRAY,
        MAP,
        DEFAULT
    }

    public static class EventRepresentationEnumExtensions {
        public static string GetAnnotationText(this EventRepresentationEnum value)
        {
            switch (value)
            {
                case EventRepresentationEnum.OBJECTARRAY:
                    return "@EventRepresentation(Array=True)";
                case EventRepresentationEnum.MAP:
                    return "@EventRepresentation(Array=False)";
                default:
                    return string.Empty;
            }
        }

        public static Type GetOutputClass(this EventRepresentationEnum value) {
            switch (value)
            {
                case EventRepresentationEnum.OBJECTARRAY:
                    return typeof (object[]);
                case EventRepresentationEnum.MAP:
                    return typeof (IDictionary<string, object>);
                default:
                    return EventRepresentationExtensions.Default == EventRepresentation.OBJECTARRAY
                               ? typeof (object[])
                               : typeof (IDictionary<string, object>);
            }
        }

        public static string GetOutputTypeCreateSchemaName(this EventRepresentationEnum value)
        {
            switch (value)
            {
                case EventRepresentationEnum.OBJECTARRAY:
                    return " objectarray";
                case EventRepresentationEnum.MAP:
                    return " map";
                default:
                    return null;
            }
        }

        public static bool IsObjectArrayEvent(this EventRepresentationEnum value) 
        {
            switch (value)
            {
                case EventRepresentationEnum.OBJECTARRAY:
                    return true;
                case EventRepresentationEnum.MAP:
                    return false;
                default:
                    return false;
            }
        }

        public static bool MatchesClass(this EventRepresentationEnum value, Type representationType)
        {
            return TypeHelper.IsSubclassOrImplementsInterface(
                representationType, GetOutputClass(value));
        }

        public static EventRepresentationEnum GetEngineDefault(EPServiceProvider engine)
        {
            var spi = (EPServiceProviderSPI) engine;
            if (spi.ConfigurationInformation.EngineDefaults.EventMetaConfig.DefaultEventRepresentation == EventRepresentation.OBJECTARRAY) {
                return EventRepresentationEnum.OBJECTARRAY;
            }

            return EventRepresentationEnum.MAP;
        }
    
        public static void AddAnnotation(this EventRepresentationEnum value, EPStatementObjectModel model) {
            if (value == EventRepresentationEnum.DEFAULT) {
                return;
            }
            AnnotationPart part = new AnnotationPart(typeof(EventRepresentation).Name);
            part.AddValue("Array", value == EventRepresentationEnum.OBJECTARRAY);
            model.Annotations = Collections.SingletonList(part);
        }
    }
}
