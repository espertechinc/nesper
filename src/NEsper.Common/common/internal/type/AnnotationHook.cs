///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.annotation;

namespace com.espertech.esper.common.@internal.type
{
    [AttributeUsage(AttributeTargets.All)]
    public class AnnotationHook : HookAttribute
    {
        public AnnotationHook(
            HookType type,
            string hook)
        {
            Hook = hook;
            HookType = type;
        }

        public Type AnnotationType => typeof(HookAttribute);
    }
} // end of namespace