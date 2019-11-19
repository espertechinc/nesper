///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.compat.collections;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationExtensions;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public class CodegenClass
    {
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
            OptionalCtor = optionalCtor;
            ExplicitMembers = explicitMembers;
            Methods = methods;
            Properties = properties;

            IList<CodegenInnerClass> allInnerClasses = new List<CodegenInnerClass>(innerClasses);
            allInnerClasses.AddAll(codegenClassScope.AdditionalInnerClasses);
            InnerClasses = allInnerClasses;
        }

        public string Namespace { get; }

        public string ClassName { get; }

        public Type InterfaceImplemented { get; }

        public IList<CodegenTypedParam> ExplicitMembers { get; }

        public CodegenClassProperties Properties { get; }

        public IList<CodegenPropertyWGraph> PublicProperties => Properties.PublicProperties;

        public IList<CodegenPropertyWGraph> PrivateProperties => Properties.PrivateProperties;

        public CodegenClassMethods Methods { get; }

        public IList<CodegenMethodWGraph> PublicMethods => Methods.PublicMethods;

        public IList<CodegenMethodWGraph> PrivateMethods => Methods.PrivateMethods;

        public IList<CodegenInnerClass> InnerClasses { get; }

        public CodegenCtor OptionalCtor { get; }

        public ISet<Type> GetReferencedClasses()
        {
            ISet<Type> classes = new HashSet<Type>();
            AddReferencedClasses(
                InterfaceImplemented, 
                Methods, 
                Properties,
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

        private SyntaxTokenList GetModifiersSyntax()
        {
            var modifiers = TokenList();
            modifiers.Add(Token(SyntaxKind.PublicKeyword));
            return modifiers;
        }

        private BaseListSyntax GetBaseListSyntax()
        {
            var baseList = BaseList();
            if (InterfaceImplemented != null)
            {
                var baseTypeName = GetNameForType(InterfaceImplemented);
                baseList.Types.Add(SimpleBaseType(baseTypeName));
            }
            return baseList;
        }

        public ClassDeclarationSyntax CodegenSyntax()
        {
            return ClassDeclaration(ClassName)
                .WithModifiers(GetModifiersSyntax())
                .WithBaseList(GetBaseListSyntax())
                .WithMembers(GetMembers());
        }

        /// <summary>
        /// Creates the syntax tree for the fields.
        /// </summary>
        private SyntaxList<MemberDeclarationSyntax> GetFields()
        {
            var fieldList = List<MemberDeclarationSyntax>();

            // Fields
            foreach (var member in ExplicitMembers)
            {
                fieldList = fieldList.Add(member.CodegenSyntaxAsField());
            }
            // Add members that are captured via the optional constructor's parameters
            OptionalCtor?.CtorParams
                .Where(member => member.IsMemberWhenCtorParam)
                .For(member => fieldList.Add(member.CodegenSyntaxAsField()));

            return fieldList;
        }

        /// <summary>
        /// Creates the syntax tree for the constructors.
        /// </summary>
        private SyntaxList<MemberDeclarationSyntax> GetConstructors()
        {
            var ctorList = List<MemberDeclarationSyntax>();
            if (OptionalCtor != null) {
                ctorList = ctorList.Add(OptionalCtor.CodegenSyntax());
            }

            return ctorList;
        }

        /// <summary>
        /// Creates the syntax tree for the properties.
        /// </summary>
        private SyntaxList<MemberDeclarationSyntax> GetProperties()
        {
            var propertyList = List<MemberDeclarationSyntax>();

            foreach (var property in PublicProperties) {
                propertyList = propertyList.Add(property.CodegenSyntax());
            }

            foreach (var property in PrivateProperties) {
                propertyList = propertyList.Add(property.CodegenSyntax());
            }

            return propertyList;
        }

        /// <summary>
        /// Creates the syntax tree for the methods.
        /// </summary>
        private SyntaxList<MemberDeclarationSyntax> GetMethods()
        {
            var methodList = List<MemberDeclarationSyntax>();

            foreach (var method in PublicMethods) {
                methodList = methodList.Add(method.CodegenSyntax());
            }

            foreach (var method in PrivateMethods) {
                methodList = methodList.Add(method.CodegenSyntax());
            }

            return methodList;
        }

        /// <summary>
        /// Creates the syntax tree for the nested classes.
        /// </summary>
        private SyntaxList<MemberDeclarationSyntax> GetInnerClasses()
        {
            var innerClassList = List<MemberDeclarationSyntax>();

            foreach (var innerClass in InnerClasses)
            {
            }

            return innerClassList;
        }

        private SyntaxList<MemberDeclarationSyntax> GetMembers()
        {
            return List<MemberDeclarationSyntax>()
                .AddRange(GetFields())
                .AddRange(GetConstructors())
                .AddRange(GetProperties())
                .AddRange(GetMethods())
                .AddRange(GetInnerClasses());
        }
    }
} // end of namespace