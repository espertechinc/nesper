///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.annotation;

namespace com.espertech.esper.common.@internal.type
{
    public class AnnotationDrop : DropAttribute
    {
        public Type AnnotationType()
        {
            return typeof(DropAttribute);
        }

        public override string ToString()
        {
            return "@Drop";
        }
    }
} // end of namespace