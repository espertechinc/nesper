///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public class CodegenClass
    {
        private readonly CodegenClassMethods methods;

        public CodegenClass(
            Type interfaceClass,
            string packageName,
            string className,
            CodegenClassScope codegenClassScope,
            IList<CodegenTypedParam> explicitMembers,
            CodegenCtor optionalCtor,
            CodegenClassMethods methods,
            IList<CodegenInnerClass> innerClasses)
        {
            PackageName = packageName;
            ClassName = className;
            InterfaceImplemented = interfaceClass;
            ExplicitMembers = explicitMembers;
            OptionalCtor = optionalCtor;
            this.methods = methods;

            IList<CodegenInnerClass> allInnerClasses = new List<CodegenInnerClass>(innerClasses);
            allInnerClasses.AddAll(codegenClassScope.AdditionalInnerClasses);
            InnerClasses = allInnerClasses;
        }

        public string PackageName { get; }

        public string ClassName { get; }

        public Type InterfaceImplemented { get; }

        public IList<CodegenTypedParam> ExplicitMembers { get; }

        public IList<CodegenMethodWGraph> PublicMethods => methods.PublicMethods;

        public IList<CodegenMethodWGraph> PrivateMethods => methods.PrivateMethods;

        public IList<CodegenInnerClass> InnerClasses { get; }

        public CodegenCtor OptionalCtor { get; }

        public ISet<Type> GetReferencedClasses()
        {
            ISet<Type> classes = new HashSet<Type>();
            AddReferencedClasses(InterfaceImplemented, methods, classes);
            AddReferencedClasses(ExplicitMembers, classes);
            if (OptionalCtor != null) {
                OptionalCtor.MergeClasses(classes);
            }

            foreach (var inner in InnerClasses) {
                AddReferencedClasses(inner.InterfaceImplemented, inner.Methods, classes);
                AddReferencedClasses(inner.ExplicitMembers, classes);
                if (inner.Ctor != null) {
                    inner.Ctor.MergeClasses(classes);
                }
            }

            return classes;
        }

        private static void AddReferencedClasses(
            Type interfaceImplemented,
            CodegenClassMethods methods,
            ISet<Type> classes)
        {
            if (interfaceImplemented != null) {
                classes.Add(interfaceImplemented);
            }

            foreach (var publicMethod in methods.PublicMethods) {
                publicMethod.MergeClasses(classes);
            }

            foreach (var privateMethod in methods.PrivateMethods) {
                privateMethod.MergeClasses(classes);
            }
        }

        private static void AddReferencedClasses(
            IList<CodegenTypedParam> names,
            ISet<Type> classes)
        {
            foreach (var param in names) {
                param.MergeClasses(classes);
            }
        }
    }
} // end of namespace