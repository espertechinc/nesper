///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.compat;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.settings
{
    public interface ImportService
    {
        IContainer Container { get; }
        
        TimeAbacus TimeAbacus { get; }

#if DEPRECATED
        ClassForNameProvider ClassForNameProvider { get; }
#endif

        TypeResolver TypeResolver { get; }

        Type ResolveType(
            string className,
            bool forAnnotation,
            ExtensionClass extensionClass);

        ConstructorInfo ResolveCtor(
            Type clazz,
            Type[] paramTypes);

        MethodInfo ResolveMethod(
            Type clazz,
            string methodName,
            Type[] paramTypes,
            bool[] allowEventBeanType,
            bool[] allowEventBeanCollType);

        MethodInfo ResolveMethodOverloadChecked(
            string className,
            string methodName,
            Type[] paramTypes,
            bool[] allowEventBeanType,
            bool[] allowEventBeanCollType,
            ExtensionClass extensionClass);

        Type ResolveClassForBeanEventType(string fullyQualClassName);
    }
} // end of namespace