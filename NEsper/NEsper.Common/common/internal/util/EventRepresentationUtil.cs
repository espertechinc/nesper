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
            // assigned type has priority
            if (assignedType == AssignedType.OBJECTARRAY) {
                return EventUnderlyingType.OBJECTARRAY;
            }

            if (assignedType == AssignedType.MAP) {
                return EventUnderlyingType.MAP;
            }

            if (assignedType == AssignedType.AVRO) {
                return EventUnderlyingType.AVRO;
            }

            if (assignedType == AssignedType.VARIANT ||
                assignedType != AssignedType.NONE) {
                throw new IllegalStateException("Not handled by event representation: " + assignedType);
            }

            // annotation has second priority
            var annotation = AnnotationUtil.FindAnnotation(annotations, typeof(EventRepresentationAttribute));
            if (annotation != null) {
                var eventRepresentation = (EventRepresentationAttribute) annotation;
                if (eventRepresentation.Value == EventUnderlyingType.AVRO) {
                    return EventUnderlyingType.AVRO;
                }

                if (eventRepresentation.Value == EventUnderlyingType.OBJECTARRAY) {
                    return EventUnderlyingType.OBJECTARRAY;
                }

                if (eventRepresentation.Value == EventUnderlyingType.MAP) {
                    return EventUnderlyingType.MAP;
                }

                throw new IllegalStateException("Unrecognized enum " + eventRepresentation.Value);
            }

            // use runtime-wide default
            var configured = configs.Common.EventMeta.DefaultEventRepresentation;
            if (configured == EventUnderlyingType.OBJECTARRAY) {
                return EventUnderlyingType.OBJECTARRAY;
            }

            if (configured == EventUnderlyingType.MAP) {
                return EventUnderlyingType.MAP;
            }

            if (configured == EventUnderlyingType.AVRO) {
                return EventUnderlyingType.AVRO;
            }

            return EventUnderlyingType.MAP;
        }
    }
} // end of namespace