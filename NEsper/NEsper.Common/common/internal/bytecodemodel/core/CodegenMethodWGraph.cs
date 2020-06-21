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
            MemberModifier modifiers,
            CodegenMethod originator)
        {
            Name = name;
            Footprint = footprint;
            Block = block;
            IsPublic = isPublic;
            Modifiers = modifiers;
            Originator = originator;
        }

        public string Name { get; }

        public CodegenMethodFootprint Footprint { get; }

        public CodegenBlock Block { get; }

        public MemberModifier Modifiers { get; private set; }

        public bool IsPublic { get; set; }

        public CodegenMethod Originator { get; set;  }

        public CodegenMethodWGraph WithStatic()
        {
            Modifiers = Modifiers.Enable(MemberModifier.STATIC);
            return this;
        }
        
        public void MergeClasses(ISet<Type> classes)
        {
            Footprint.MergeClasses(classes);
            Block.MergeClasses(classes);
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
            else
            {
                // We found that there's an important distinction with internal classes between Java and C#.  In Java
                // private methods in an inner class are still visible and callable between parent and child.  In C#
                // this is not true.  We alleviate this by making the default method access internal.
                builder.Append("internal ");
            }

            if (Modifiers.IsStatic())
            {
                builder.Append("static ");
            }

            if (Modifiers.IsOverride())
            {
                builder.Append("override ");
            }

            if (Modifiers.IsVirtual())
            {
                builder.Append("virtual ");
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
            if (Modifiers.IsStatic()) {
                tokenList = tokenList.Add(Token(SyntaxKind.StaticKeyword));
            }

            // Override modifier
            if (Modifiers.IsOverride()) {
                tokenList = tokenList.Add(Token(SyntaxKind.OverrideKeyword));
            }

            // Virtual modifier
            if (Modifiers.IsVirtual()) {
                tokenList = tokenList.Add(Token(SyntaxKind.VirtualKeyword));
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