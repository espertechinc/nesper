///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public class CodegenInnerClass
    {
        public CodegenInnerClass(
            string className,
            Type interfaceImplemented,
            CodegenCtor ctor,
            IList<CodegenTypedParam> explicitMembers,
            CodegenClassMethods methods,
            CodegenClassProperties properties)
        {
            ClassName = className;
            InterfaceImplemented = interfaceImplemented;
            Ctor = ctor;
            ExplicitMembers = explicitMembers;
            Methods = methods;
            Properties = properties;
        }

        public string ClassName { get; }

        public Type InterfaceImplemented { get; }

        public IList<CodegenTypedParam> ExplicitMembers { get; }

        public CodegenClassProperties Properties { get; }

        public CodegenClassMethods Methods { get; }

        public CodegenCtor Ctor { get; }

        public string InterfaceGenericClass { get; set; }
    }
} // end of namespace