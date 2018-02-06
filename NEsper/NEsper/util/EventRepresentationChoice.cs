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
using com.espertech.esper.client.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;

namespace com.espertech.esper.util
{
    public enum EventRepresentationChoice
    {
        ARRAY,
        MAP,
        AVRO,
        DEFAULT
    }

    public static class EventRepresentationChoiceExtensions
    {
        public static string GetAnnotationText(this EventRepresentationChoice enumValue)
        {
            switch (enumValue)
            {
                case EventRepresentationChoice.ARRAY:
                    return "@EventRepresentation('objectarray')";
                case EventRepresentationChoice.MAP:
                    return "@EventRepresentation('map')";
                case EventRepresentationChoice.AVRO:
                    return "@EventRepresentation('avro')";
                case EventRepresentationChoice.DEFAULT:
                    return "";
            }

            throw new ArgumentException("invalid value for enumValue", "enumValue");
        }

        public static string GetOutputTypeCreateSchemaName(this EventRepresentationChoice enumValue)
        {
            switch (enumValue)
            {
                case EventRepresentationChoice.ARRAY:
                    return " objectarray";
                case EventRepresentationChoice.MAP:
                    return " map";
                case EventRepresentationChoice.AVRO:
                    return " avro";
                case EventRepresentationChoice.DEFAULT:
                    return "";
            }

            throw new ArgumentException("invalid value for enumValue", "enumValue");
        }

        public static string GetOutputTypeClassName(this EventRepresentationChoice enumValue)
        {
            switch (enumValue)
            {
                case EventRepresentationChoice.ARRAY:
                    return EventUnderlyingType.OBJECTARRAY.GetUnderlyingClassName();
                case EventRepresentationChoice.MAP:
                    return EventUnderlyingType.MAP.GetUnderlyingClassName();
                case EventRepresentationChoice.AVRO:
                    return EventUnderlyingType.AVRO.GetUnderlyingClassName();
                case EventRepresentationChoice.DEFAULT:
                    return EventUnderlyingTypeExtensions.GetDefault().GetUnderlyingClassName();
            }

            throw new ArgumentException("invalid value for enumValue", "enumValue");
        }


        public static EventUnderlyingType GetUnderlyingType(this EventRepresentationChoice enumValue)
        {
            switch (enumValue)
            {
                case EventRepresentationChoice.ARRAY:
                    return EventUnderlyingType.OBJECTARRAY;
                case EventRepresentationChoice.MAP:
                    return EventUnderlyingType.MAP;
                case EventRepresentationChoice.AVRO:
                    return EventUnderlyingType.AVRO;
                case EventRepresentationChoice.DEFAULT:
                    return EventUnderlyingTypeExtensions.GetDefault();
            }

            throw new ArgumentException("invalid value for enumValue", "enumValue");
        }


        public static bool MatchesClass(this EventRepresentationChoice enumValue, Type representationType)
        {
            var outputTypeClassName = GetOutputTypeClassName(enumValue);
            var supers = new HashSet<Type>();
            TypeHelper.GetBase(representationType, supers);
            supers.Add(representationType);
            foreach (Type clazz in supers)
            {
                if (clazz.FullName == outputTypeClassName)
                {
                    return true;
                }
            }
            return false;
        }

        public static EventRepresentationChoice GetEngineDefault(EPServiceProvider engine)
        {
            var spi = (EPServiceProviderSPI) engine;
            var configured = spi.ConfigurationInformation.EngineDefaults.EventMeta.DefaultEventRepresentation;
            if (configured == EventUnderlyingType.OBJECTARRAY)
            {
                return EventRepresentationChoice.ARRAY;
            }
            else if (configured == EventUnderlyingType.AVRO)
            {
                return EventRepresentationChoice.AVRO;
            }
            return EventRepresentationChoice.MAP;
        }

        public static bool IsObjectArrayEvent(this EventRepresentationChoice enumValue)
        {
            return enumValue == EventRepresentationChoice.ARRAY;
        }

        public static bool IsMapEvent(this EventRepresentationChoice enumValue)
        {
            return enumValue == EventRepresentationChoice.DEFAULT || enumValue == EventRepresentationChoice.MAP;
        }

        public static string GetAnnotationTextForNonMap(this EventRepresentationChoice enumValue)
        {
            if (enumValue == EventRepresentationChoice.DEFAULT || enumValue == EventRepresentationChoice.MAP)
            {
                return "";
            }
            return GetAnnotationText(enumValue);
        }

        public static void AddAnnotationForNonMap(this EventRepresentationChoice enumValue, EPStatementObjectModel model)
        {
            if (enumValue == EventRepresentationChoice.DEFAULT || enumValue == EventRepresentationChoice.MAP)
            {
                return;
            }
            var part = new AnnotationPart("EventRepresentation");
            if (enumValue == EventRepresentationChoice.ARRAY)
            {
                part.AddValue("objectarray");
            }
            if (enumValue == EventRepresentationChoice.AVRO)
            {
                part.AddValue("avro");
            }
            model.Annotations = Collections.SingletonList(part);
        }

        public static bool IsAvroEvent(this EventRepresentationChoice enumValue)
        {
            return enumValue == EventRepresentationChoice.AVRO;
        }
    }
} // end of namespace
