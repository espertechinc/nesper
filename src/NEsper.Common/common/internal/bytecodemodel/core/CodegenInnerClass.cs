///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.util;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public class CodegenInnerClass
    {
        public CodegenInnerClass(
            string className,
            Type optionalInterfaceImplemented,
            CodegenCtor ctor,
            IList<CodegenTypedParam> explicitMembers,
            CodegenClassMethods methods,
            CodegenClassProperties properties)
            : this(className, ctor, explicitMembers, methods, properties)
        {
            if (optionalInterfaceImplemented != null) {
            }

            ClassName = className.CodeInclusionTypeName();
            BaseList.AssignType(optionalInterfaceImplemented);
            Ctor = ctor;
            ExplicitMembers = explicitMembers;
            Methods = methods;
            Properties = properties;
        }

        public CodegenInnerClass(
            string className,
            CodegenCtor ctor,
            IList<CodegenTypedParam> explicitMembers,
            CodegenClassMethods methods,
            CodegenClassProperties properties)
        {
            ClassName = className.CodeInclusionTypeName();
            BaseList = new CodegenClassBaseList();
            Ctor = ctor;
            ExplicitMembers = explicitMembers;
            Methods = methods;
            Properties = properties;
        }

        public string ClassName { get; }

        public CodegenClassBaseList BaseList { get; }

        public IList<CodegenTypedParam> ExplicitMembers { get; }

        public CodegenClassProperties Properties { get; }

        public CodegenClassMethods Methods { get; }

        public CodegenCtor Ctor { get; }

        public string InterfaceGenericClass { get; set; }

        public void AddInterfaceImplemented(Type type)
        {
            BaseList.AddInterface(type);
        }
    }
} // end of namespace