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
    public class AnnotationHint : HintAttribute
    {
        public AnnotationHint(
            string value,
            AppliesTo applies,
            string model) : base(value, model, applies)
        {
        }

        public Type AnnotationType => typeof(HintAttribute);
    }
} // end of namespace