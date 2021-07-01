///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.support
{
    public enum EventRepresentationChoice
    {
        OBJECTARRAY,
        MAP,
        AVRO,
        JSON,
        JSONCLASSPROVIDED,
        DEFAULT
    }

    public static class EventRepresentationChoiceExtensions
    {
        public static IEnumerable<EventRepresentationChoice> Values()
        {
            return EnumHelper.GetValues<EventRepresentationChoice>()
                .Where(_ => _ != EventRepresentationChoice.DEFAULT);
        }
        
        public static string GetPublicName(this EventRepresentationChoice enumValue)
        {
            if (enumValue == EventRepresentationChoice.DEFAULT) {
                return GetUnderlyingType(enumValue).GetName();
            }
            else {
                return enumValue.GetName();
            }
        }

        public static string GetAnnotationText(this EventRepresentationChoice enumValue)
        {
            return enumValue switch {
                EventRepresentationChoice.OBJECTARRAY => "@EventRepresentation('objectarray')",
                EventRepresentationChoice.MAP => "@EventRepresentation('map')",
                EventRepresentationChoice.AVRO => "@EventRepresentation('avro')",
                EventRepresentationChoice.JSON => "@EventRepresentation('json')",
                EventRepresentationChoice.JSONCLASSPROVIDED => throw new UnsupportedOperationException("For Json-Provided please use getAnnotationTextWJsonProvided(class)"),
                EventRepresentationChoice.DEFAULT => "",
                _ => throw new ArgumentException("invalid value for EnumValue", nameof(enumValue))
            };
        }

        public static string GetAnnotationTextWJsonProvided<T>(this EventRepresentationChoice enumValue)
        {
            return GetAnnotationTextWJsonProvided(enumValue, typeof(T));
        }

        public static string GetAnnotationTextWJsonProvided(this EventRepresentationChoice enumValue, Type jsonProvidedClass) {
            if (enumValue == EventRepresentationChoice.JSONCLASSPROVIDED) {
                return "@JsonSchema(ClassName='" + jsonProvidedClass.FullName + "') " + 
                       "@EventRepresentation('json')";
            }

            return GetAnnotationText(enumValue);
        }

        public static EventUnderlyingType GetUnderlyingType(this EventRepresentationChoice enumValue)
        {
            return enumValue switch {
                EventRepresentationChoice.OBJECTARRAY => EventUnderlyingType.OBJECTARRAY,
                EventRepresentationChoice.MAP => EventUnderlyingType.MAP,
                EventRepresentationChoice.AVRO => EventUnderlyingType.AVRO,
                EventRepresentationChoice.JSON => EventUnderlyingType.JSON,
                EventRepresentationChoice.JSONCLASSPROVIDED => EventUnderlyingType.JSON,
                EventRepresentationChoice.DEFAULT => EventUnderlyingTypeExtensions.GetDefault(),
                _ => throw new ArgumentException("invalid value for EnumValue", nameof(enumValue))
            };
        }

        public static Type GetOutputTypeClass(this EventRepresentationChoice enumValue)
        {
            return GetUnderlyingType(enumValue).GetUnderlyingClass();
        }

        public static string GetOutputTypeClassName(this EventRepresentationChoice enumValue)
        {
            return GetUnderlyingType(enumValue).GetUnderlyingClassName();
        }

        public static bool MatchesClass(
            this EventRepresentationChoice enumValue,
            Type representationType)
        {
            var underlyingType = GetUnderlyingType(enumValue);
            var outputTypeClass = underlyingType.GetUnderlyingClass();
            var outputTypeClassName = underlyingType.GetUnderlyingClassName();
            var supers = new HashSet<Type>();
            TypeHelper.GetBase(representationType, supers);
            supers.Add(representationType);
            foreach (Type clazz in supers) {
                if ((clazz.FullName == outputTypeClassName) ||
                    (outputTypeClass != null && TypeHelper.IsSubclassOrImplementsInterface(clazz, outputTypeClass))) {
                    return true;
                }
            }

            return false;
        }

        public static EventRepresentationChoice GetEngineDefault(Configuration configuration)
        {
            var configured = configuration.Common.EventMeta.DefaultEventRepresentation;
            if (configured == EventUnderlyingType.OBJECTARRAY) {
                return EventRepresentationChoice.OBJECTARRAY;
            }
            else if (configured == EventUnderlyingType.AVRO) {
                return EventRepresentationChoice.AVRO;
            }

            return EventRepresentationChoice.MAP;
        }

        public static bool IsObjectArrayEvent(this EventRepresentationChoice enumValue)
        {
            return enumValue == EventRepresentationChoice.OBJECTARRAY;
        }

        public static bool IsMapEvent(this EventRepresentationChoice enumValue)
        {
            return enumValue == EventRepresentationChoice.DEFAULT || enumValue == EventRepresentationChoice.MAP;
        }

        public static string GetAnnotationTextForNonMap(this EventRepresentationChoice enumValue)
        {
            return enumValue switch {
                EventRepresentationChoice.OBJECTARRAY => "@EventRepresentation('objectarray')",
                EventRepresentationChoice.MAP => "",
                EventRepresentationChoice.AVRO => "@EventRepresentation('avro')",
                EventRepresentationChoice.JSON => "@EventRepresentation('json')",
                EventRepresentationChoice.JSONCLASSPROVIDED => "@EventRepresentation('json')",
                EventRepresentationChoice.DEFAULT => "",
                _ => throw new ArgumentException("invalid value for EnumValue", nameof(enumValue))
            };
        }

        public static void AddAnnotationForNonMap(
            this EventRepresentationChoice enumValue,
            EPStatementObjectModel model)
        {
            if (enumValue == EventRepresentationChoice.DEFAULT || enumValue == EventRepresentationChoice.MAP) {
                return;
            }

            var part = new AnnotationPart("EventRepresentation");
            switch (enumValue) {
                case EventRepresentationChoice.OBJECTARRAY:
                    part.AddValue("objectarray");
                    break;

                case EventRepresentationChoice.AVRO:
                    part.AddValue("avro");
                    break;

                case EventRepresentationChoice.JSON:
                case EventRepresentationChoice.JSONCLASSPROVIDED:
                    part.AddValue("json");
                    break;

            }

            model.Annotations = Collections.SingletonList(part);
        }

        public static bool IsAvroOrJsonEvent(this EventRepresentationChoice enumValue)
        {
            switch (enumValue) {
                case EventRepresentationChoice.AVRO:
                case EventRepresentationChoice.JSON:
                case EventRepresentationChoice.JSONCLASSPROVIDED:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsJsonEvent(this EventRepresentationChoice enumValue)
        {
            return enumValue == EventRepresentationChoice.JSON;
        }

        public static bool IsJsonProvidedClassEvent(this EventRepresentationChoice enumValue)
        {
            return enumValue == EventRepresentationChoice.JSONCLASSPROVIDED;
        }

        public static bool IsAvroEvent(this EventRepresentationChoice enumValue)
        {
            return enumValue == EventRepresentationChoice.AVRO;
        }
    }
} // end of namespace