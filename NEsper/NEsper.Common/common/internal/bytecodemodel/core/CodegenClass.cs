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
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public class CodegenClass
    {
        private readonly CodegenClassMethods _methods;
        private readonly CodegenClassProperties _properties;

        public CodegenClass(
            Type interfaceClass,
            string @namespace,
            string className,
            CodegenClassScope codegenClassScope,
            IList<CodegenTypedParam> explicitMembers,
            CodegenCtor optionalCtor,
            CodegenClassMethods methods,
            CodegenClassProperties properties,
            IList<CodegenInnerClass> innerClasses)
        {
            Namespace = @namespace;
            ClassName = className;
            InterfaceImplemented = interfaceClass;
            ExplicitMembers = explicitMembers;
            OptionalCtor = optionalCtor;
            _methods = methods;
            _properties = properties;

            IList<CodegenInnerClass> allInnerClasses = new List<CodegenInnerClass>(innerClasses);
            allInnerClasses.AddAll(codegenClassScope.AdditionalInnerClasses);
            InnerClasses = allInnerClasses;
        }

        public string Namespace { get; }

        public string ClassName { get; }

        public Type InterfaceImplemented { get; }

        public IList<CodegenTypedParam> ExplicitMembers { get; }

        public CodegenClassProperties Properties => _properties;

        public IList<CodegenPropertyWGraph> PublicProperties => _properties.PublicProperties;

        public IList<CodegenPropertyWGraph> PrivateProperties => _properties.PrivateProperties;

        public IList<CodegenMethodWGraph> PublicMethods => _methods.PublicMethods;

        public IList<CodegenMethodWGraph> PrivateMethods => _methods.PrivateMethods;

        public IList<CodegenInnerClass> InnerClasses { get; }

        public CodegenCtor OptionalCtor { get; }

        public ISet<Type> GetReferencedClasses()
        {
            ISet<Type> classes = new HashSet<Type>();
            AddReferencedClasses(
                InterfaceImplemented, 
                _methods, 
                _properties,
                classes);
            AddReferencedClasses(ExplicitMembers, classes);
            OptionalCtor?.MergeClasses(classes);

            foreach (var inner in InnerClasses) {
                AddReferencedClasses(
                    inner.InterfaceImplemented, 
                    inner.Methods, 
                    inner.Properties,
                    classes);
                AddReferencedClasses(inner.ExplicitMembers, classes);
                inner.Ctor?.MergeClasses(classes);
            }

            return classes;
        }

        private static void AddReferencedClasses(
            Type interfaceImplemented,
            CodegenClassMethods methods,
            CodegenClassProperties properties,
            ISet<Type> classes)
        {
            if (interfaceImplemented != null) {
                classes.AddToSet(interfaceImplemented);
            }

            methods.PublicMethods.ForEach(m => m.MergeClasses(classes));
            methods.PrivateMethods.ForEach(m => m.MergeClasses(classes));

            properties.PublicProperties.ForEach(m => m.MergeClasses(classes));
            properties.PrivateProperties.ForEach(m => m.MergeClasses(classes));
        }

        private static void AddReferencedClasses(
            IList<CodegenTypedParam> names,
            ISet<Type> classes)
        {
            names.ForEach(param => param.MergeClasses(classes));
        }
    }
} // end of namespace