///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.codegen.core
{
    public class CodegenClass : ICodegenClass
    {
        public CodegenClass(
            string @namespace,
            string className,
            Type interfaceImplemented,
            IList<ICodegenMember> members,
            IList<ICodegenMethod> publicMethods,
            IList<ICodegenMethod> privateMethods)
        {
            Namespace = @namespace;
            ClassName = className;
            InterfaceImplemented = interfaceImplemented;
            Members = members;
            PublicMethods = publicMethods;
            PrivateMethods = privateMethods;
        }

        public string Namespace { get; }

        public string ClassName { get; }

        public Type InterfaceImplemented { get; }

        public IList<ICodegenMember> Members { get; }

        public IList<ICodegenMethod> PublicMethods { get; }

        public IList<ICodegenMethod> PrivateMethods { get; }

        public ICollection<Type> GetReferencedClasses()
        {
            var classes = new HashSet<Type>();
            classes.Add(InterfaceImplemented);

            foreach (var member in Members)
            {
                member.MergeClasses(classes);
            }

            foreach (var publicMethod in PublicMethods)
            {
                publicMethod.MergeClasses(classes);
            }

            foreach (var privateMethod in PrivateMethods)
            {
                privateMethod.MergeClasses(classes);
            }

            return classes;
        }
    }
} // end of namespace