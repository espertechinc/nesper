///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.epl.annotation;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.util
{
    public class EventRepresentationUtil
    {

        public static EventUnderlyingType GetRepresentation(
            Attribute[] annotations,
            ConfigurationInformation configs,
            AssignedType assignedType)
        {
            // assigned type has priority
            if (assignedType == AssignedType.OBJECTARRAY)
            {
                return EventUnderlyingType.OBJECTARRAY;
            }
            else if (assignedType == AssignedType.MAP)
            {
                return EventUnderlyingType.MAP;
            }
            else if (assignedType == AssignedType.AVRO)
            {
                return EventUnderlyingType.AVRO;
            }
            if (assignedType == AssignedType.VARIANT ||
                assignedType != AssignedType.NONE)
            {
                throw new IllegalStateException("Not handled by event representation: " + assignedType);
            }

            // annotation has second priority
            var annotation = AnnotationUtil.FindAnnotation(annotations, typeof (EventRepresentationAttribute));
            if (annotation != null)
            {
                EventRepresentationAttribute eventRepresentation = (EventRepresentationAttribute) annotation;
                if (eventRepresentation.Value == EventUnderlyingType.AVRO)
                {
                    return EventUnderlyingType.AVRO;
                }
                else if (eventRepresentation.Value == EventUnderlyingType.OBJECTARRAY)
                {
                    return EventUnderlyingType.OBJECTARRAY;
                }
                else if (eventRepresentation.Value == EventUnderlyingType.MAP)
                {
                    return EventUnderlyingType.MAP;
                }
                else
                {
                    throw new IllegalStateException("Unrecognized enum " + eventRepresentation.Value);
                }
            }

            // use engine-wide default
            EventUnderlyingType configured = configs.EngineDefaults.EventMeta.DefaultEventRepresentation;
            if (configured == EventUnderlyingType.OBJECTARRAY)
            {
                return EventUnderlyingType.OBJECTARRAY;
            }
            else if (configured == EventUnderlyingType.MAP)
            {
                return EventUnderlyingType.MAP;
            }
            else if (configured == EventUnderlyingType.AVRO)
            {
                return EventUnderlyingType.AVRO;
            }
            return EventUnderlyingType.MAP;
        }
    }
} // end of namespace
