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
using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.@base;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationHelper;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public class CodegenMethodWGraph
    {
        public CodegenMethodWGraph(
            string name,
            CodegenMethodFootprint footprint,
            CodegenBlock block,
            bool isPublic,
            bool isOverride,
            bool isStatic)
        {
            Name = name;
            Footprint = footprint;
            Block = block;
            IsPublic = isPublic;
            IsOverride = isOverride;
            IsStatic = isStatic;
        }

        public string Name { get; }

        public CodegenMethodFootprint Footprint { get; }

        public CodegenBlock Block { get; }

        public bool IsOverride { get; set; }

        public bool IsPublic { get; set; }

        public bool IsStatic { get; set; }

        public void MergeClasses(ISet<Type> classes)
        {
            Footprint.MergeClasses(classes);
            Block.MergeClasses(classes);
        }

        public CodegenMethodWGraph WithOverride(bool isOverride)
        {
            IsPublic = isOverride;
            return this;
        }

        public CodegenMethodWGraph WithPublic(bool isPublic)
        {
            IsPublic = isPublic;
            return this;
        }

        public CodegenMethodWGraph WithStatic(bool isStatic)
        {
            IsStatic = isStatic;
            return this;
        }

        public void Render(
            StringBuilder builder,
            bool isPublic,
            bool isInnerClass,
            CodegenIndent indent,
            int additionalIndent)
        {
            if (Footprint.OptionalComment != null)
            {
                indent.Indent(builder, 1 + additionalIndent);
                builder
                    .Append("// ")
                    .Append(Footprint.OptionalComment)
                    .Append("\n");
            }

            indent.Indent(builder, 1 + additionalIndent);
            if (isPublic)
            {
                builder.Append("public ");
            }

            if (IsStatic)
            {
                builder.Append("static ");
            }

            if (IsOverride)
            {
                builder.Append("override ");
            }

            if (Footprint.ReturnType != null)
            {
                AppendClassName(builder, Footprint.ReturnType);
            }
            else
            {
                builder.Append(Footprint.ReturnTypeName);
            }

            builder
                .Append(" ")
                .Append(Name)
                .Append("(");

            var delimiter = "";
            foreach (var param in Footprint.Params)
            {
                builder.Append(delimiter);
                param.Render(builder);
                delimiter = ",";
            }

            builder.Append(")");

            builder.Append("{\n");
            Block.Render(builder, isInnerClass, 2 + additionalIndent, indent);
            indent.Indent(builder, 1 + additionalIndent);
            builder.Append("}\n");
        }

        /// <summary>
        /// Gets the modifiers for the method.
        /// </summary>
        public SyntaxTokenList GetModifiers()
        {
            var tokenList = TokenList();

            // Access modifier
            var token = IsPublic ? Token(SyntaxKind.PublicKeyword) : Token(SyntaxKind.InternalKeyword);

            var optionalComment = Footprint.OptionalComment;
            if (optionalComment != null) {
                token = token.WithLeadingTrivia(
                    TriviaList(
                        Comment(optionalComment)
                    )
                );
            }

            tokenList = tokenList.Add(token);

            // Static modifier
            if (IsStatic) {
                tokenList = tokenList.Add(Token(SyntaxKind.StaticKeyword));
            }

            // Override modifier
            if (IsOverride) {
                tokenList = tokenList.Add(Token(SyntaxKind.OverrideKeyword));
            }

            return tokenList;
        }

        /// <summary>
        /// Gets the syntax for the return type.
        /// </summary>
        private TypeSyntax GetReturnTypeSyntax()
        {
            return ParseTypeName(Footprint.ReturnTypeName);
        }

        /// <summary>
        /// Generates the syntax for the parameter list.
        /// </summary>
        private ParameterListSyntax GetParameterListSyntax()
        {
            var parameters = Footprint.Params
                .Select(param => param.CodegenSyntaxAsParameter())
                .ToArray();
            return ParameterList(SeparatedList<ParameterSyntax>(parameters));
        }

        /// <summary>
        /// Gets the syntax tree for the body.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private BlockSyntax GetBody()
        {
            return Block.CodegenSyntax();
        }

        /// <summary>
        /// Generates the syntax tree for the property.
        /// </summary>
        public MemberDeclarationSyntax CodegenSyntax()
        {
            return MethodDeclaration(
                    GetReturnTypeSyntax(),
                    Identifier(Name))
                .WithModifiers(GetModifiers())
                .WithParameterList(GetParameterListSyntax())
                .WithBody(GetBody());
        }
    }
} // end of namespace