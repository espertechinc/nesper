///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.util;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationHelper;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public class CodegenTypedParam
    {
        public CodegenTypedParam(
            string typeName,
            Type type,
            string name,
            bool memberWhenCtorParam,
            bool isPublic)
        {
            if (type == null && typeName == null) {
                throw new ArgumentException("Invalid null type");
            }

            TypeName = typeName?.CodeInclusionTypeName();
            Type = type;
            Name = name;
            IsMemberWhenCtorParam = memberWhenCtorParam;
            IsPublic = isPublic;
        }

        public CodegenTypedParam(
            string typeName,
            Type type,
            string name)
            : this(typeName, type, name, true, false)
        {
        }

        public CodegenTypedParam(
            Type type,
            string name)
            : this(null, type, name)
        {
        }

        public CodegenTypedParam(
            Type type,
            string name,
            bool memberWhenCtorParam)
            : this(null, type, name, memberWhenCtorParam, false)
        {
        }

        public CodegenTypedParam(
            Type type,
            string name,
            bool memberWhenCtorParam,
            bool isPublic)
            : this(null, type, name, memberWhenCtorParam, isPublic)
        {
        }

        public CodegenTypedParam(
            string typeName,
            string name,
            bool memberWhenCtorParam,
            bool isPublic)
            : this(typeName, null, name, memberWhenCtorParam, isPublic)
        {
        }

        public CodegenTypedParam(
            string type,
            string name)
            : this(type, null, name)
        {
        }

        public Type Type { get; }

        public string TypeName { get; }

        public string Name { get; }

        public bool IsMemberWhenCtorParam { get; }

        public bool IsPublic { get; }

        public bool IsReadonly { get; set; } = true;

        public bool IsStatic { get; set; }

        public CodegenTypedParam WithFinal(bool aFinal)
        {
            IsReadonly = aFinal;
            return this;
        }

        public CodegenTypedParam WithStatic(bool aStatic)
        {
            IsStatic = aStatic;
            return this;
        }

        public void RenderAsParameter(StringBuilder builder)
        {
            if (Type != null) {
                AppendClassName(builder, Type);
            }
            else {
                builder.Append(TypeName);
            }

            builder.Append(" ").Append(Name);
        }

        public void MergeClasses(ISet<Type> classes)
        {
            if (Type != null) {
                classes.AddToSet(Type);
            }
        }

        public void RenderAsMember(
            StringBuilder builder)
        {
            if (Type != null) {
                AppendClassName(builder, Type);
            }
            else {
                builder.Append(TypeName);
            }

            builder.Append(" ").Append(Name);
        }

        public void RenderType(
            StringBuilder builder)
        {
            if (Type != null) {
                AppendClassName(builder, Type);
            }
            else {
                builder.Append(TypeName);
            }
        }

        public override string ToString()
        {
            return "CodegenTypedParam{" +
                   "typeName='" +
                   TypeName +
                   '\'' +
                   ", type=" +
                   Type +
                   ", name='" +
                   Name +
                   '\'' +
                   ", memberWhenCtorParam=" +
                   IsMemberWhenCtorParam +
                   '}';
        }

        public ParameterSyntax CodegenSyntaxAsParameter()
        {
            var parameterType = ParseTypeName(TypeName);
            var parameter = Parameter(Identifier(Name))
                .WithType(parameterType);
            return parameter;
        }

        public MemberDeclarationSyntax CodegenSyntaxAsField()
        {
            var fieldType = ParseTypeName(TypeName);
            var variableDecl = VariableDeclaration(fieldType)
                .WithVariables(
                    SingletonSeparatedList(
                        VariableDeclarator(
                            Identifier(Name))));

            return FieldDeclaration(variableDecl);
        }
    }
} // end of namespace