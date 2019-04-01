///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.util;

namespace com.espertech.esper.common.@internal.type
{
    public class AnnotationEventRepresentation : EventRepresentationAttribute
    {
        private readonly EventUnderlyingType value;

        public AnnotationEventRepresentation(EventUnderlyingType value)
        {
            this.value = value;
        }

        public EventUnderlyingType Value()
        {
            return value;
        }

        public T AnnotationType<T>() where T : Attribute
        {
            return typeof(EventRepresentation);
        }
    }
} // end of namespace