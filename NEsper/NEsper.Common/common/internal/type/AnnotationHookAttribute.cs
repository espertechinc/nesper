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
    [AttributeUsage(AttributeTargets.All)]
    public class AnnotationHookAttribute : HookAttribute
    {
        private readonly string hook;
        private readonly HookType type;

        public AnnotationHookAttribute(HookType type, string hook)
        {
            this.type = type;
            this.hook = hook;
        }

        public override string Hook => hook;

        public override HookType HookType => type;

        public Type AnnotationType => typeof(HookAttribute);
    }
} // end of namespace