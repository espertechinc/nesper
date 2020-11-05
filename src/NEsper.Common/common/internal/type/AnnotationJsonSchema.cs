///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.annotation;

namespace com.espertech.esper.common.@internal.type
{
    public class AnnotationJsonSchema : JsonSchemaAttribute
    {
        public AnnotationJsonSchema(
            bool dynamic,
            string className)
        {
            Dynamic = dynamic;
            ClassName = className;
        }

        public Type AnnotationType => typeof(JsonSchemaAttribute);
    }
} // end of namespace