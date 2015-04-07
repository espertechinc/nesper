///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.compat;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.util
{
    public class EventRepresentationUtil
    {
        public static bool IsMap(Attribute[] annotations, ConfigurationInformation configs, AssignedType assignedType)
        {
            // assigned type has priority
            if (assignedType == AssignedType.OBJECTARRAY)
            {
                return false;
            }
            if (assignedType == AssignedType.MAP)
            {
                return true;
            }
            if (assignedType == AssignedType.VARIANT || assignedType != AssignedType.NONE)
            {
                throw new IllegalStateException("Not handled by event representation: " + assignedType);
            }

            // annotation has second priority
            Attribute annotation = epl.annotation.AnnotationUtil.FindAttribute((IEnumerable<Attribute>)annotations, typeof(EventRepresentationAttribute));
            if (annotation != null)
            {
                var eventRepresentation = (EventRepresentationAttribute)annotation;
                return !eventRepresentation.Array;
            }

            // use engine-wide default
            return configs.EngineDefaults.EventMetaConfig.DefaultEventRepresentation == EventRepresentation.MAP;
        }
    }
}
