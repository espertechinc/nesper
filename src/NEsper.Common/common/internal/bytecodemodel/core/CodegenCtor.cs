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

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public class CodegenCtor : CodegenMethod
    {
        public CodegenCtor(
            Type generator,
            bool includeDebugSymbols,
            IList<CodegenTypedParam> @params) : this(generator, null, includeDebugSymbols, @params)
        {
        }

        public CodegenCtor(
            Type generator,
            string className,
            bool includeDebugSymbols,
            IList<CodegenTypedParam> @params) : base(
            null,
            null,
            generator,
            CodegenSymbolProviderEmpty.INSTANCE,
            new CodegenScope(includeDebugSymbols))
        {
            CtorParams = @params;
            ClassName = className;
        }

        public CodegenCtor(
            Type generator,
            CodegenClassScope classScope,
            IList<CodegenTypedParam> @params) : base(
            null,
            null,
            generator,
            CodegenSymbolProviderEmpty.INSTANCE,
            new CodegenScope(classScope.IsDebug))
        {
            CtorParams = @params;
            ClassName = classScope.ClassName;
        }

        public IList<CodegenTypedParam> CtorParams { get; }
        public string ClassName { get; }

        public override void MergeClasses(ISet<Type> classes)
        {
            base.MergeClasses(classes);
            foreach (var param in CtorParams) {
                param.MergeClasses(classes);
            }
        }

        public MemberDeclarationSyntax CodegenSyntax()
        {
            return ConstructorDeclaration(Identifier(ClassName))
                .WithModifiers(GetModifiers())
                .WithParameterList(ParameterList)
                .WithBody(Body);
        }

        private StatementSyntax[] AssignConstructorParametersToFields()
        {
            var statements = List<StatementSyntax>();
            foreach (var ctorParam in CtorParams) {
                if (ctorParam.IsMemberWhenCtorParam) {
                    var paramIdentifier = IdentifierName(ctorParam.Name);
                    var memberAccess = MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ThisExpression(),
                        paramIdentifier);
                    var assignment = AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        memberAccess,
                        paramIdentifier);
                    statements = statements.Add(ExpressionStatement(assignment));
                }
            }

            return statements.ToArray();
        }

        public BlockSyntax Body {
            get {
                var block = Block();
                // Assign constructor parameters to fields
                block = block.AddStatements(AssignConstructorParametersToFields());
                // Render the remainder of the constructor body
                block = block.AddStatements(Block.CodegenSyntax());
                // Done
                return block;
            }
        }

        public ParameterListSyntax ParameterList {
            get {
                return ParameterList(
                    SeparatedList<ParameterSyntax>(CtorParams.Select(param => param.CodegenSyntaxAsParameter())));
            }
        }
    }
} // end of namespace