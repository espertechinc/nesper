///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.annotation;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.util
{
    public class EventRepresentationUtil
    {
        public static EventUnderlyingType GetRepresentation(
            Attribute[] annotations,
            Configuration configs,
            AssignedType assignedType)
        {
            switch (assignedType) {
                // assigned type has priority
                case AssignedType.OBJECTARRAY:
                    return EventUnderlyingType.OBJECTARRAY;

                case AssignedType.MAP:
                    return EventUnderlyingType.MAP;

                case AssignedType.AVRO:
                    return EventUnderlyingType.AVRO;

                case AssignedType.JSON:
                    return EventUnderlyingType.JSON;
            }

            if (assignedType == AssignedType.VARIANT ||
                     assignedType != AssignedType.NONE) {
                throw new IllegalStateException("Not handled by event representation: " + assignedType);
            }

            // annotation has second priority
            var annotation = AnnotationUtil.FindAnnotation(annotations, typeof(EventRepresentationAttribute));
            if (annotation != null) {
                var eventRepresentation = (EventRepresentationAttribute) annotation;
                return eventRepresentation.Value switch {
                    EventUnderlyingType.AVRO => EventUnderlyingType.AVRO,
                    EventUnderlyingType.JSON => EventUnderlyingType.JSON,
                    EventUnderlyingType.OBJECTARRAY => EventUnderlyingType.OBJECTARRAY,
                    EventUnderlyingType.MAP => EventUnderlyingType.MAP,
                    _ => throw new IllegalStateException("Unrecognized enum " + eventRepresentation.Value)
                };
            }

            // use runtime-wide default
            var configured = configs.Common.EventMeta.DefaultEventRepresentation;
            return configured switch {
                EventUnderlyingType.OBJECTARRAY => EventUnderlyingType.OBJECTARRAY,
                EventUnderlyingType.MAP => EventUnderlyingType.MAP,
                EventUnderlyingType.AVRO => EventUnderlyingType.AVRO,
                EventUnderlyingType.JSON => EventUnderlyingType.JSON,
                _ => EventUnderlyingType.MAP
            };
        }
    }
} // end of namespace